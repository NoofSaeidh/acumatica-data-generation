using Bogus;
using CrmDataGeneration.Common;
using CrmDataGeneration.Entities.Emails;
using CrmDataGeneration.Soap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VoidTask = System.Threading.Tasks.Task;

namespace CrmDataGeneration.Entities.Leads
{
    public class LeadGenerationRunner : GenerationRunner<Lead, LeadGenerationSettings>
    {
        public LeadGenerationRunner(ApiConnectionConfig apiConnectionConfig, LeadGenerationSettings generationSettings)
            : base(apiConnectionConfig, generationSettings)
        {
        }

        protected override async VoidTask RunGenerationSequentRaw(int count, CancellationToken cancellationToken)
        {
            var seed = GenerationSettings.RandomizerSettings.Seed;
            var leads = GenerateRandomizedList(count);

            cancellationToken.ThrowIfCancellationRequested();

            using (var client = await GetLoginLogoutClient())
            {
                foreach (var lead in leads)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    lead.ReturnBehavior = ReturnBehavior.OnlySystem;
                    var resultLead = await client.PutAsync(lead, cancellationToken);
                    lead.ID = resultLead.ID;
                }

                cancellationToken.ThrowIfCancellationRequested();

                if (!GenerationSettings.ConvertByStatuses.IsNullOrEmpty())
                {
                    var toConvertLeads = PrepareLeadsForConvertionByStatuses(leads).ToArray();

                    if (toConvertLeads.Any())
                    {
                        // convert to opportunities
                        var convertLeadsToOpportunities = GetLeadsByConvertFlag(toConvertLeads, ConvertLead.ToOpportunity);
                        await ConvertLeadsToOpportunities(client, convertLeadsToOpportunities, cancellationToken);
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();

                var pairs = PrepareEmailsForCreation(leads).ToArray();
                if (pairs.Any())
                {
                    await CreateEmailsAndLinkToLeads(client, pairs, cancellationToken);
                }
            }
        }

        public async VoidTask ConvertLeadsToOpportunities(IApiClient client, IEnumerable<Lead> leads, CancellationToken cancellationToken = default)
        {
            foreach (var lead in leads)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await client.InvokeAsync(lead, new ConvertLeadToOpportunity(), cancellationToken);
            }
        }

        public async VoidTask CreateEmailsAndLinkToLeads(IApiClient client, IEnumerable<KeyValuePair<Lead, IEnumerable<Email>>> pairs, CancellationToken cancellationToken = default)
        {
            foreach (var pair in pairs)
            {
                foreach (var email in pair.Value)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    email.ReturnBehavior = ReturnBehavior.OnlySystem;
                    var createdEmail = await client.PutAsync(email, cancellationToken);
                    await client.InvokeAsync(
                        createdEmail,
                        new LinkEntityToEmail
                        {
                            RelatedEntity = pair.Key.LeadID?.ToString(),
                            Type = GenerationSettings.PxTypeName
                        }
                    );
                }
            }
        }


        protected IEnumerable<Lead> GetLeadsByConvertFlag(IEnumerable<KeyValuePair<ConvertLead, IEnumerable<Lead>>> leads, ConvertLead flag)
        {
            return leads
                .Where(x => x.Key.HasFlag(flag))
                .SelectMany(x => x.Value);
        }

        protected IEnumerable<KeyValuePair<ConvertLead, IEnumerable<Lead>>> PrepareLeadsForConvertionByStatuses(IEnumerable<Lead> leads)
        {
            // convert depending on Probability defined in ConvertToOpportunityByStatus

            var leadsList = leads.ToArray();

            var convertToOpportunity = new List<Lead>(leadsList.Length);

            var byConversion = GenerationSettings
                .ConvertByStatuses
                ?.SelectMany(x => x.Value.AsDictionary,
                    (x, y) => new { conversion = y.Key, status = x.Key, probability = y.Value })
                .GroupBy(x => x.conversion);
            //.ToDictionary(x => x.conversion, x => new { x.status, x.probability });
            if (byConversion == null)
                yield break;

            foreach (var conversion in byConversion)
            {
                yield return new KeyValuePair<ConvertLead, IEnumerable<Lead>>(
                    conversion.Key,
                    leadsList
                        .Where(l =>
                        {
                            var conv = conversion.FirstOrDefault(c => c.status == l.Status);
                            if (conv == null)
                                return false;
                            if (Randomizer.Bool((float)conv.probability))
                                return true;
                            return false;
                        })
                );
            }
        }

        protected IEnumerable<KeyValuePair<Lead, IEnumerable<Email>>> PrepareEmailsForCreation(IEnumerable<Lead> leads)
        {
            var emailSettings = GenerationSettings.EmailsGenerationSettings;
            if (emailSettings == null
                || emailSettings.EmailRandomizerSettings == null
                || emailSettings.EmailsForSingleLeadCounts == null
            )
                yield break;


            foreach (var lead in leads)
            {
                var emailsCount = Randomizer.ProbabilityRandomIfAny(emailSettings.EmailsForSingleLeadCounts);
                if (emailsCount == 0)
                    continue;

                var emails = emailSettings.EmailRandomizerSettings.GetStatefullDataGenerator().GenerateList(emailsCount);
                var resultEmails = new List<Email>(emails.Count * 2);
                foreach (var email in emails)
                {
                    email.Incoming = true;
                    email.From = lead.Email;
                    email.To = Randomizer.ProbabilityRandomIfAny(emailSettings.SystemAccounts).Email;
                    resultEmails.Add(email);

                    var outEmail = emailSettings.EmailRandomizerSettings.GetStatefullDataGenerator().Generate();
                    outEmail.Incoming = false;
                    outEmail.From = Randomizer.ProbabilityRandomIfAny(emailSettings.SystemAccounts).Email;
                    outEmail.To = lead.Email;
                    resultEmails.Add(outEmail);
                }
                yield return new KeyValuePair<Lead, IEnumerable<Email>>(lead, resultEmails);
            }
        }
    }
}
