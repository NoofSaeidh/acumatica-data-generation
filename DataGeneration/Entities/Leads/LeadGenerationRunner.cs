using Bogus;
using DataGeneration.Core;
using DataGeneration.Core.Api;
using DataGeneration.Soap;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using VoidTask = System.Threading.Tasks.Task;

namespace DataGeneration.Entities.Leads
{
    public class LeadGenerationRunner : GenerationRunner<Lead, LeadGenerationSettings>
    {
        public LeadGenerationRunner(ApiConnectionConfig apiConnectionConfig, LeadGenerationSettings generationSettings)
            : base(apiConnectionConfig, generationSettings)
        {
        }

        protected override async VoidTask GenerateSingle(IApiClient client, Lead entity, CancellationToken ct)
        {
            await client.PutAsync(entity, ct);
        }

        protected override void LogResultsArgs(out string entity, out string parentEntity, out string action)
        {
            entity = "Lead";
            parentEntity = "Lead";
            action = "Create";
        }
    }
}