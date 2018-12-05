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
    public class LeadGenerationRunner : GenerationRunner<LeadWrapper, LeadGenerationSettings>
    {
        public LeadGenerationRunner(ApiConnectionConfig apiConnectionConfig, LeadGenerationSettings generationSettings)
            : base(apiConnectionConfig, generationSettings)
        {
        }

        protected override async VoidTask GenerateSingle(IApiClient client, LeadWrapper entity, CancellationToken cancellationToken)
        {
            if(entity.ConvertToOpportunity)
            {
                // Invoke put entity, so it should work without Put Lead before it... but it doesn't.
                await client.InvokeAsync(
                    await client.PutAsync(entity.Lead, cancellationToken), 
                    new ConvertLeadToOpportunity(),
                    cancellationToken
                );
            }
            else
            {
                await client.PutAsync(entity.Lead, cancellationToken);
            }
        }
    }
}