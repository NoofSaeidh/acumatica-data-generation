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
    public class GenerateSalesDemoScript
    {
        public GeneratorConfig GetConfig_Stage1()
        {
            return GetConfig(GetSettings_Stage1());
        }

        protected GeneratorConfig GetConfig(IGenerationSettings settings)
        {
            return new GeneratorConfig
            {
                ApiConnectionConfig = new ApiConnectionConfig
                (
                    new EndpointSettings("http://localhost:201", "datagen", "18.200.001"),
                    new LoginInfo { Username = "admin", Password = "123" }
                ),
                ServicePointSettings = new ServicePointSettings
                {
                    DefaultConnectionLimit = 6,
                },
                NestedSettings = new BatchSettings
                {
                    StopProcessingAtException = true,
                    GenerationSettings = new[] { settings },
                },
            };
        }

        public IGenerationSettings GetSettings_Stage1()
        {
            var campaigns = new Dictionary<string, List<int>>();
            return DelegateGenerationSettings.Create<Soap.Lead>(
                faker =>
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
                        .RuleFor(e => e.LeadClass, "LEADCON".ToValue());
                },
                async (client, lead, ct) =>
                {
                    var resultLead = await client.PutAsync(lead, ct);
                    if(resultLead.SourceCampaign?.Value is string campaign)
                    {
                        if (!campaigns.TryGetValue(campaign, out var leads))
                            campaigns[campaign] = leads = new List<int>();
                        leads.Add(resultLead.LeadID.Value.Value);
                    }
                },
                afterGenerateDelegate: async (client, ct) =>
                {
                    foreach (var (campaignId, leads) in campaigns)
                    {
                        var campaign = await client.GetAsync(
                            new Soap.Campaign
                            {
                                CampaignID = campaignId
                            });
                        campaign.Members = leads
                            .Select(l =>
                                new Soap.CampaignMember
                                {
                                    Type = "Lead",
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
                    else if(word2.Value.ToLower().IsIn("llc", "inc"))
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
                    else if(and1.Success)
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
                    else if(hypen.Success)
                    {
                        if(f.PickRandom(true, false))
                        {
                            domain = word1.Value + word2.Value;
                        }
                        else
                        {
                            domain = word1.Value + "-" + word2.Value;
                        }
                    }
                }
                if(domain == null)
                {
                    System.Diagnostics.Debugger.Break();
                    domain = "domain";
                }



                var result =  $"{l.FirstName}.{l.LastName}@{domain.Replace("'", "")}.example.com".ToLower().ToValue();
                //System.Diagnostics.Debugger.Log(0, "info", "email = " + result + "\r\n");
                return result;
            }

        }
    }
}
