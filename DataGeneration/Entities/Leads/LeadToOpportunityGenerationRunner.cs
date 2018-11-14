using DataGeneration.Common;
using DataGeneration.Soap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataGeneration.Entities.Leads
{
    public class LeadToOpportunityGenerationRunner : EntitiesSearchGenerationRunner<Lead, LeadToOpportunityGenerationSettings>
    {
        public LeadToOpportunityGenerationRunner(ApiConnectionConfig apiConnectionConfig, LeadToOpportunityGenerationSettings generationSettings) : base(apiConnectionConfig, generationSettings)
        {
        }

        protected override async System.Threading.Tasks.Task GenerateSingle(IApiClient client, Lead entity, CancellationToken cancellationToken)
        {
            await client.InvokeAsync(entity, new ConvertLeadToOpportunity(), cancellationToken);
        }

        protected override void UtilizeFoundEntities(IList<Entity> entities)
        {
            GenerationSettings.RandomizerSettings.Leads = entities.Cast<Lead>();

            ChangeGenerationCount(GenerationSettings.RandomizerSettings.NewLeads.Count, "To be equal to found entities count.");
        }
    }
}
