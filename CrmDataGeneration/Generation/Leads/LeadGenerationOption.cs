using Bogus;
using CrmDataGeneration.Common;
using CrmDataGeneration.Generation.Activities;
using CrmDataGeneration.OpenApi.Reference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VoidTask = System.Threading.Tasks.Task;

namespace CrmDataGeneration.Generation.Leads
{
    public class LeadGenerationOption : GenerationOption<Lead>
    {
        public IDictionary<string, ProbabilityCollection<ConvertLead>> ConvertByStatuses { get; set; }
        public ProbabilityCollection<(string Email, string DisplayName)> SystemAccounts { get; set; }
        public ActivityGenerationOption IncomingActivities { get; set; }
        public ActivityGenerationOption OutgoingActivities { get; set; }

        #region Methods
        public override async VoidTask RunGeneration(GeneratorClient client, CancellationToken cancellationToken = default)
        {
            CheckSettings();
            var leads = await client.GenerateAll(this, cancellationToken);

            if (ConvertByStatuses.IsNullOrEmpty())
                return;

            var seed = RandomizerSettings.Seed ?? client.Config.GlobalSeed;

            var toConvertLeads = PrepareLeadsForConvertionByStatuses(seed, leads).ToArray();

            if (toConvertLeads.Any())
            {
                // convert to opportunities
                await ConvertLeadsToOpportunities(client,
                    GetLeadsByConvertFlag(toConvertLeads, ConvertLead.ToOpportunity),
                    cancellationToken);
            }

            await GenerateEmails(client, leads, cancellationToken);
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

        protected async VoidTask ConvertLeadsToOpportunities(GeneratorClient client, IEnumerable<Lead> leads, CancellationToken cancellationToken = default)
        {
            var wrappedClient = client.GetApiWrappedClient<Lead>();
            var actionClient = client.GetRawApiClient<InvokeActionClient>();
            await wrappedClient.WrapAction("Convert leads to opportunity", System.Threading.Tasks.Task.Run(async () =>
            {
                foreach (var lead in leads)
                {
                    var invocation = new ConvertLeadToOpportunity { Entity = lead };

                    await actionClient.ConvertLeadToOpportunityAsync(invocation, cancellationToken);
                }
                return leads;
            }));
        }

        protected async Task<IEnumerable<Email>> GenerateEmails(GeneratorClient client, IEnumerable<Lead> leads, CancellationToken cancellationToken = default)
        {
            var result = Enumerable.Empty<Email>();
            var rand = new Randomizer(RandomizerSettings.Seed ?? client.Config.GlobalSeed);
            if (IncomingActivities != null)
            {
                foreach (var lead in leads)
                {
                    var emailsCount = rand.ProbabilityRandomIfAny(IncomingActivities.EmailsCount);
                    if (emailsCount == 0)
                        continue;
                    var emails = IncomingActivities.RandomizerSettings.GetRandomizer().GenerateList(emailsCount);
                    foreach (var item in emails)
                    {
                        item.Incoming = true;
                        item.From = lead.Email;
                        item.To = rand.ProbabilityRandomIfAny(SystemAccounts).Email;
                    }
                    // todo: thread maintain (and log) here
                    result.Union(await IncomingActivities.GenerateEmails(client, emails, cancellationToken));
                }
            }
            if (OutgoingActivities != null)
            {
                foreach (var lead in leads)
                {
                    var emailsCount = rand.ProbabilityRandomIfAny(OutgoingActivities.EmailsCount);
                    if (emailsCount == 0)
                        continue;
                    var emails = OutgoingActivities.RandomizerSettings.GetRandomizer().GenerateList(emailsCount);
                    foreach (var item in emails)
                    {
                        item.Incoming = false;
                        item.To = lead.Email;
                        //todo: from system account
                        item.FromEmailAccountDisplayName = rand.ProbabilityRandomIfAny(SystemAccounts).DisplayName;
                        item.From = rand.ProbabilityRandomIfAny(SystemAccounts).Email;
                    }
                    result.Union(await OutgoingActivities.GenerateEmails(client, emails, cancellationToken));
                }
            }
            return result;
        }

        #endregion
    }
}
