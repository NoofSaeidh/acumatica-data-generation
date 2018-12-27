using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataGeneration.Core;
using DataGeneration.Core.Api;
using DataGeneration.Core.Queueing;
using DataGeneration.Soap;
using VoidTask = System.Threading.Tasks.Task;

namespace DataGeneration.Entities.Leads
{
    public class LeadConvertGenerationRunner : EntitiesSearchGenerationRunner<EntityWrapper<int>, LeadConvertGenerationSettings>
    {
        public LeadConvertGenerationRunner(ApiConnectionConfig apiConnectionConfig, LeadConvertGenerationSettings generationSettings)
            : base(apiConnectionConfig, generationSettings)
        {
        }

        protected override bool IgnoreComplexQueryEntity => true;

        protected override bool IgnoreAdjustReturnBehavior => true;

        protected override void AdjustEntitySearcher(EntitySearcher searcher)
        {
            base.AdjustEntitySearcher(searcher);
            searcher
                .AdjustInput(adj =>
                    adj.AdjustIfIsOrThrow<Lead>(lead =>
                    {
                        lead.LeadID = new IntReturn();
                        lead.Status = new StringReturn();
                    }));
        }

        protected override async VoidTask GenerateSingle(IApiClient client, EntityWrapper<int> entity, CancellationToken ct)
        {
            await client.InvokeAsync(
                new Lead
                {
                    LeadID = new IntSearch(entity.Key),
                    ReturnBehavior = ReturnBehavior.None
                },
                new ConvertLeadToOpportunity(),
                ct
            );
        }

        protected override void UtilizeFoundEntities(IList<Entity> entities)
        {
            GenerationSettings.RandomizerSettings.Leads =
                entities
                    .Cast<Lead>()
                    .Select(l => (l.LeadID.Value.Value, l.Status.Value))
                    .ToArray();
        }

        protected override void LogResultsArgs(out string entity, out string parentEntity, out string action)
        {
            entity = "Lead";
            parentEntity = "Lead";
            action = "Convert";
        }
    }
}
