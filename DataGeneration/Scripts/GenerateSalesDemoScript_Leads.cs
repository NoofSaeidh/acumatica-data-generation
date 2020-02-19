using Bogus;
using DataGeneration.Core;
using DataGeneration.Core.Api;
using DataGeneration.Core.Extensions;
using DataGeneration.Core.Settings;
using DataGeneration.GenerationInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using static DataGeneration.Soap.SoapExtensions;


namespace DataGeneration.Scripts
{
    public class GenerateSalesDemoScript_Leads : GenerateSalesDemoScriptBase
    {
        public GeneratorConfig GetConfig_Stage1()
        {
            return GetConfig(GetSettings_Stage1());
        }

        public GeneratorConfig GetConfig_Stage2()
        {
            return GetConfig(GetSettings_Stage2());
        }

        public GeneratorConfig GetConfig_Stage3()
        {
            return GetConfig(GetSettings_Stage3());
        }


        protected IGenerationSettings GetSettings_Stage1()
        {
            var campaigns = new Dictionary<string, List<int>>();
            return DelegateGenerationSettings.Create<Soap.Lead>(
                (@this, faker) =>
                {
                    return faker
                        .RuleFor(e => e.FirstName, (f, e) => f.Name.FirstName().ToValue())
                        .RuleFor(e => e.LastName, (f, e) => f.Name.LastName().ToValue())
                        .RuleFor(e => e.CompanyName, (f, e) =>
                            f.Random.WeightedRandom(
                                ("Maurices", 0.02f),
                                ("El Centro Books", 0.02f),
                                ("Virtual Management Corp", 0.01f),
                                (f.Company.CompanyName(), 0.95f))
                                .ToValue())
                        .RuleFor(e => e.Email, GetEmail)
                        .RuleFor(e => e.Source, (f, e) => f.Random.WeightedRandom(("Campaign", 0.8f), (null, 0.2f))?.ToValue())
                        .RuleFor(e => e.SourceCampaign, (f, e) =>
                            e.Source?.Value == "Campaign"
                                ? f.Random.WeightedRandom(("GADS2020", 0.8f), ("HAB2020", 0.2f)).ToValue()
                                : null)
                        .RuleFor(e => e.LeadClass, "LEADCON".ToValue())
                        .RuleFor(e => e.Status, "Open".ToValue());
                },
                async (@this, client, lead, ct) =>
                {
                    var resultLead = await client.PutAsync(lead, ct);
                    if (resultLead.SourceCampaign?.Value is string campaign)
                    {
                        if (!campaigns.TryGetValue(campaign, out var leads))
                            campaigns[campaign] = leads = new List<int>();
                        leads.Add(resultLead.LeadID.Value.Value);
                    }
                },
                afterGenerateDelegate: async (@this, client, ct) =>
                {
                    foreach (var (campaignId, leads) in campaigns)
                    {
                        var campaign = await client.GetAsync(
                            new Soap.Campaign
                            {
                                CampaignID = campaignId,
                                Members = new Soap.CampaignMember[0]
                            });
                        campaign.Members = leads
                            .Select(l =>
                                new Soap.CampaignMember
                                {
                                    ContactID = l
                                })
                            .ToArray();
                        await client.PutAsync(campaign);
                    }
                })
                .ChangeSettings(s =>
                {
                    s.Count = 100;
                    s.ExecutionTypeSettings = ExecutionTypeSettings.Sequent();
                    //s.ExecutionTypeSettings = ExecutionTypeSettings.Parallel(4);
                });

            Soap.StringValue GetEmail(Faker f, Soap.Lead l)
            {
                var company = l.CompanyName.Value;
                var match = Regex.Match(company,
                    @"^(?:(?<word1>[\w']+)\s?,?(?<hypen>-?)(?<and1>and)?\s+(?<word2>[\w']+)\s?(?<and2>and)?\s?(?<word3>[\w']+)?)$",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled);

                string domain = null;
                if (company == "Maurices")
                {
                    domain = "maurices";
                }
                else if (company == "El Centro Books")
                {
                    domain = "centrobooks";
                }
                else if (company == "Virtual Management Corp")
                {
                    domain = "virtual-management";
                }
                else if (match.Success)
                {
                    var word1 = match.Groups["word1"];
                    var word2 = match.Groups["word2"];
                    var word3 = match.Groups["word3"];
                    var and1 = match.Groups["and1"];
                    var and2 = match.Groups["and2"];
                    var hypen = match.Groups["hypen"];

                    if (!word2.Success)
                    {
                        domain = match.Groups["word1"].Value;
                    }
                    else if (word2.Value.ToLower().IsIn("llc", "inc"))
                    {
                        domain = word1.Value;
                    }
                    else if (word3.Success)
                    {
                        int type = and2.Success ? f.PickRandom(0, 1, 2, 3) : f.PickRandom(0, 1, 2);
                        switch (type)
                        {
                            case 0:
                                domain = word1.Value + word2.Value + word3.Value;
                                if (domain.Length > 12)
                                    goto case 2;
                                break;
                            case 1:
                                domain = word1.Value + "-" + word2.Value + "-" + word3.Value;
                                if (domain.Length > 16)
                                    goto case 2;
                                break;
                            case 2:
                                domain = word1.Value.Substring(0, 1) + word2.Value.Substring(0, 1) + word3.Value.Substring(0, 1);
                                break;
                            case 3:
                                domain = word1.Value + "-" + word2.Value + "-and-" + word3.Value;
                                if (domain.Length > 19)
                                    goto case 2;
                                break;
                        }
                    }
                    else if (and1.Success)
                    {
                        int type = f.PickRandom(0, 1, 2);
                        switch (type)
                        {
                            case 0:
                                domain = word1.Value + word2.Value;
                                break;
                            case 1:
                                domain = word1.Value + "-" + word2.Value;
                                break;
                            case 2:
                                domain = word1.Value + "-and-" + word2.Value;
                                break;
                        }
                    }
                    else if (hypen.Success)
                    {
                        if (f.PickRandom(true, false))
                        {
                            domain = word1.Value + word2.Value;
                        }
                        else
                        {
                            domain = word1.Value + "-" + word2.Value;
                        }
                    }
                }
                if (domain == null)
                {
                    System.Diagnostics.Debugger.Break();
                    domain = "domain";
                }



                var result = $"{l.FirstName}.{l.LastName}@{domain.Replace("'", "")}.example.com".ToLower().ToValue();
                //System.Diagnostics.Debugger.Log(0, "info", "email = " + result + "\r\n");
                return result;
            }

        }

        protected IGenerationSettings GetSettings_Stage2()
        {
            IList<Soap.Lead> leads = null;
            return DelegateGenerationSettings.Create(
                new
                {
                    Lead = default(Soap.Lead),
                    Activity = default(Soap.Activity),
                },
                (@this, faker) =>
                {
                    int i = 0;
                    return faker.CustomInstantiator(f =>
                    {
                        return i < leads.Count
                            ? new
                            {
                                Lead = leads[i++],
                                Activity = new Soap.Activity
                                {
                                    Type = "P",
                                    Summary = "Campaign response",
                                },
                            }
                            : null;
                    });
                },
                async (@this, client, entity, ct) =>
                {
                    var activity = await client.PutAsync(entity.Activity, ct);
                    await client.InvokeAsync(
                        activity,
                        new Soap.LinkEntityToActivity
                        {
                            Type = "PX.Objects.CR.CRLead",
                            RelatedEntity = entity.Lead.LeadDisplayName
                        }, ct);
                    entity.Lead.Status = "Sales-Ready";
                    await client.PutAsync(entity.Lead);
                },
                beforeGenerateDelegate: async (@this, client, ct) =>
                {
                    var rand = @this.GenerationSettings.RandomizerSettings.GetRandomizer();
                    var leadIds =
                        (await client.GetAsync(
                            new Soap.Campaign
                            {
                                CampaignID = new Soap.StringSearch("HAB2020"),
                                Members = new[]
                                {
                                    new Soap.CampaignMember()
                                }
                            }, ct)
                        ).Members
                        .Union(
                            (await client.GetAsync(
                                new Soap.Campaign
                                {
                                    CampaignID = new Soap.StringSearch("GADS2020"),
                                    Members = new[]
                                    {
                                        new Soap.CampaignMember()
                                    }
                                }, ct)
                            ).Members)
                        .Select(m => m.ContactID.Value.Value)
                        .ToList();

                    leads = (await client.GetListAsync(
                                new Soap.Lead
                                {
                                    SourceCampaign = new Soap.StringSearch("HAB2020")
                                }, ct)
                         ).Union(
                            (await client.GetListAsync(
                                new Soap.Lead
                                {
                                    SourceCampaign = new Soap.StringSearch("GADS2020")
                                }, ct)
                            ))
                        .Where(l => leadIds.Contains(l.LeadID.Value.Value))
                        .Where(m => rand.WeightedRandom((true, 0.15f), (false, 0.85f)))
                        .ToList();

                    @this.ChangeGenerationCount(leads.Count, "To fit campaign members count");
                }
                //afterGenerateDelegate: async (@this, client, ct) =>
                //{

                //}
                )
                .ChangeSettings(s =>
                {
                    s.Count = 100;
                    s.ExecutionTypeSettings = ExecutionTypeSettings.Sequent();
                    //s.ExecutionTypeSettings = ExecutionTypeSettings.Parallel(4);
                });
        }

        protected IGenerationSettings GetSettings_Stage3()
        {
            var now = DateTime.UtcNow;
            var yesterday = now.AddDays(-1);
            var now30 = now.AddDays(30);
            var now90 = now.AddDays(90);
            IList<Soap.Lead> leads = null;
            return DelegateGenerationSettings.Create(
                new
                {
                    Lead = default(Soap.Lead),
                    Activity = default(Soap.Activity),
                    Action = default(Soap.Action),
                },
                (@this, faker) =>
                {
                    int i = 0;
                    return faker.CustomInstantiator(f =>
                    {
                        if (i < leads.Count)
                        {
                            var actionId = f.Random.WeightedRandom((0, 0.5f), (1, 0.05f), (2, 0.45f));
                            Soap.Action action = actionId switch
                            {
                                0 => new Soap.Disqualify
                                {
                                    Reason = "No Interest"
                                },
                                1 => new Soap.Disqualify
                                {
                                    Reason = "Unable to Contact"
                                },
                                2 => new Soap.ConvertToOpportunity
                                {
                                    Class = "PRODUCT",
                                    Subject = "Product Interest",
                                    CloseDate = f.Date.Between(now30, now90),
                                },
                                _ => null,
                            };
                            return new
                            {
                                Lead = leads[i++],
                                Activity = new Soap.Activity
                                {
                                    Type = "P",
                                    Summary = "Qualification Call",
                                },
                                Action = action
                            };
                        }
                        return null;
                    });
                },
                async (@this, client, entity, ct) =>
                {
                    var activity = await client.PutAsync(entity.Activity, ct);
                    await client.InvokeAsync(
                        activity,
                        new Soap.LinkEntityToActivity
                        {
                            Type = "PX.Objects.CR.CRLead",
                            RelatedEntity = entity.Lead.LeadDisplayName.Value,
                        }, ct);
                    await client.InvokeAsync(entity.Lead, entity.Action, ct);
                },
                beforeGenerateDelegate: async (@this, client, ct) =>
                {
                    leads = await client.GetListAsync(
                            new Soap.Lead
                            {
                                Workgroup = new Soap.StringSearch("Inside Sales - Qualify"),
                                CreatedDate = new Soap.DateTimeSearch(yesterday, Soap.DateTimeCondition.IsGreaterThanOrEqualsTo)
                            }, ct);

                    @this.ChangeGenerationCount(leads.Count, "To fit campaign members count");
                },
                afterGenerateDelegate: async (@this, client, ct) =>
                {
                    var opps = await client.GetListAsync(
                        new Soap.Opportunity
                        {
                            Subject = new Soap.StringSearch("Product Interest"),
                            CreatedDate = new Soap.DateTimeSearch(yesterday, Soap.DateTimeCondition.IsGreaterThanOrEqualsTo)
                        }, ct);
                    foreach (var op in opps)
                    {
                        op.ManualAmount = true;
                        op.Amount = 2000;
                        await client.PutAsync(op);
                    }
                })
                .ChangeSettings(s =>
                {
                    s.Count = 100;
                    s.ExecutionTypeSettings = ExecutionTypeSettings.Sequent();
                    //s.ExecutionTypeSettings = ExecutionTypeSettings.Parallel(4);
                });
        }
    }
}
