using Bogus;
using CrmDataGeneration.Common;
using CrmDataGeneration.Entities.Emails;
using CrmDataGeneration.OpenApi.Reference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VoidTask = System.Threading.Tasks.Task;

namespace CrmDataGeneration.Entities.Leads
{
    public class LeadGenerationOption : GenerationOption<Lead>
    {
        public IDictionary<string, ProbabilityCollection<ConvertLead>> ConvertByStatuses { get; set; }
        public ProbabilityCollection<(string Email, string DisplayName)> SystemAccounts { get; set; }
        public EmailGenerationOption IncomingEmails { get; set; }
        public EmailGenerationOption OutgoingEmails { get; set; }

        #region Methods

        public override async VoidTask RunGeneration(GeneratorClient client, CancellationToken cancellationToken = default)
        {
            CheckSettings();
            var leads = (await client.GenerateAll(this, cancellationToken)).ToArray();

            var wrappedClient = client.GetApiWrappedClient<LeadApiWrappedClient>();
            var emailsWrappedClient = client.GetApiWrappedClient<EmailApiWrappedClient>();
            var seed = RandomizerSettings.Seed ?? client.Config.GlobalSeed;

            if (!ConvertByStatuses.IsNullOrEmpty())
            {
                var toConvertLeads = PrepareLeadsForConvertionByStatuses(seed, leads).ToArray();

                if (toConvertLeads.Any())
                {
                    // convert to opportunities
                    await wrappedClient.ConvertLeadsToOpportunities(
                            GetLeadsByConvertFlag(toConvertLeads, ConvertLead.ToOpportunity)
                            .Select(x=>new ConvertLeadToOpportunity { Entity = x }),
                        ExecutionTypeSettings,
                        cancellationToken);
                }
            }

            var emails = PrepareEmailsForCreation(seed, leads).ToArray();
            if(emails.Any())
            {
                await emailsWrappedClient.CreateAllAndLinkEntities(emails, ExecutionTypeSettings, cancellationToken);
            }
        }

        protected IEnumerable<Lead> GetLeadsByConvertFlag(IEnumerable<KeyValuePair<ConvertLead, IEnumerable<Lead>>> leads, ConvertLead flag)
        {
            return leads
                .Where(x => x.Key.HasFlag(flag))
                .SelectMany(x => x.Value);
        }

        protected IEnumerable<KeyValuePair<ConvertLead, IEnumerable<Lead>>> PrepareLeadsForConvertionByStatuses(int seed, IEnumerable<Lead> leads)
        {
            // convert depending on Probability defined in ConvertToOpportunityByStatus

            var leadsList = leads.ToArray();

            var rand = new Randomizer(seed);

            var convertToOpportunity = new List<Lead>(leadsList.Length);

            var byConversion = ConvertByStatuses
                .SelectMany(x => x.Value.AsDictionary,
                    (x, y) => new { conversion = y.Key, status = x.Key, probability = y.Value })
                .GroupBy(x => x.conversion);
            //.ToDictionary(x => x.conversion, x => new { x.status, x.probability });

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
                            if (rand.Bool((float)conv.probability))
                                return true;
                            return false;
                        })
                );
            }
        }

        protected IEnumerable<SelectRelatedEntityEmail> PrepareEmailsForCreation(int seed, IEnumerable<Lead> leads)
        {
            var rand = new Randomizer(seed);
            if (IncomingEmails != null)
            {
                foreach (var lead in leads)
                {
                    var emailsCount = rand.ProbabilityRandomIfAny(IncomingEmails.EmailsCount);
                    if (emailsCount == 0)
                        continue;
                    var emails = IncomingEmails.RandomizerSettings.GetRandomizer().GenerateList(emailsCount);
                    foreach (var email in emails)
                    {
                        email.Incoming = true;
                        email.From = lead.Email;
                        email.To = rand.ProbabilityRandomIfAny(SystemAccounts).Email;
                        yield return new SelectRelatedEntityEmail
                        {
                            Entity = email,
                            Parameters = new Parameters4
                            {
                                RelatedEntity = lead.GetKeyFieldValue(),
                                Type = PxTypeName
                            }
                        };
                    }
                }
            }
            if (OutgoingEmails != null)
            {
                //todo: !!!
                //foreach (var lead in leads)
                //{
                //    var emailsCount = rand.ProbabilityRandomIfAny(OutgoingActivities.EmailsCount);
                //    if (emailsCount == 0)
                //        continue;
                //    var emails = OutgoingActivities.RandomizerSettings.GetRandomizer().GenerateList(emailsCount);
                //    foreach (var item in emails)
                //    {
                //        item.Incoming = false;
                //        item.To = lead.Email;
                //        //todo: from system account
                //        item.FromEmailAccountDisplayName = rand.ProbabilityRandomIfAny(SystemAccounts).DisplayName;
                //        item.From = rand.ProbabilityRandomIfAny(SystemAccounts).Email;
                //    }
                //}
            }
        }

        #endregion
    }
}
