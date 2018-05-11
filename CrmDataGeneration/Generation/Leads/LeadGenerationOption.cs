using Bogus;
using CrmDataGeneration.Common;
using CrmDataGeneration.OpenApi.Reference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrmDataGeneration.Generation.Leads
{
    public class LeadGenerationOption : GenerationOption<Lead>
    {
        public IDictionary<string, ProbabilityCollection<ConvertLead>> ConvertByStatuses { get; set; }

        public override async System.Threading.Tasks.Task RunGeneration(GeneratorClient client, CancellationToken cancellationToken = default)
        {
            CheckSettings();
            var leads = (await client.GenerateAll(this, cancellationToken));

            if (ConvertByStatuses.IsNullOrEmpty())
                return;

            var seed = RandomizerSettings.Seed ?? client.Config.GlobalSeed;

            var toConvertLeads = PrepareLeadsForConvertionByStatuses(seed, leads);

            if (toConvertLeads.Any())
            {
                // convert to opportunities
                await ConvertLeadsToOpportunities(client, 
                    GetLeadsByConvertFlag(toConvertLeads, ConvertLead.ToOpportunity),
                    cancellationToken);
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
                .ToDictionary(x => x.conversion, x => new { x.status, x.probability });

            foreach (var conversion in byConversion)
            {
                yield return new KeyValuePair<ConvertLead, IEnumerable<Lead>>(
                    conversion.Key,
                    leadsList
                        .Where(l => l.Status == conversion.Value.status
                                 && rand.Bool((float)conversion.Value.probability))
                );
            }
        }

        protected async System.Threading.Tasks.Task ConvertLeadsToOpportunities(GeneratorClient client, IEnumerable<Lead> leads, CancellationToken cancellationToken = default)
        {
            var wrappedClient = client.GetApiWrappedClient<Lead>();
            var actionClient = client.GetRawApiClient<InvokeActionClient>();
            foreach (var lead in leads)
            {
                var invocation = new ConvertLeadToOpportunity { Entity = lead };

                await wrappedClient.WrapAction(actionClient.ConvertLeadToOpportunityAsync(invocation, cancellationToken));
            }
        }
    }
}
