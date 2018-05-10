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
            var leads = (await client.GenerateAll(this, cancellationToken)).ToList();

            if (ConvertByStatuses.IsNullOrEmpty())
                return;

            // convert depending on Probability defined in ConvertToOpportunityByStatus

            var rand = new Randomizer(RandomizerSettings.Seed ?? client.Config.GlobalSeed);

            var convertToOpportunity = new List<Lead>(leads.Count);

            foreach (var lead in leads)
            {
                // define should convert (to anything) current lead by status
                if(!ConvertByStatuses.TryGetValue(lead.Status, out var probabilities))
                    continue;

                // convert to Opportunities
                var indexOfStatus = probabilities.IndexOf(ConvertLead.ToOpportunity);
                if (indexOfStatus >= 0)
                {
                    var shouldConvert = rand.Bool((float)probabilities[indexOfStatus].Value);
                    if (shouldConvert)
                        convertToOpportunity.Add(lead);
                }

            }

            if(convertToOpportunity.Any())
                await ConvertLeadsToOpportunities(client, convertToOpportunity, cancellationToken);
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
