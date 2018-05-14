using CrmDataGeneration.Common;
using CrmDataGeneration.OpenApi;
using CrmDataGeneration.OpenApi.Reference;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VoidTask = System.Threading.Tasks.Task;

namespace CrmDataGeneration.Entities.Leads
{
    public class LeadApiWrappedClient : ApiWrappedClient<Lead>
    {
        private readonly LeadClient _innerClient;
        private readonly InvokeActionClient _actionClient;
        public LeadApiWrappedClient(OpenApiState openApiState) : base(openApiState)
        {
            _innerClient = new LeadClient(openApiState);
            _actionClient = new InvokeActionClient(openApiState);
        }

        protected override async Task<Lead> CreateRaw(Lead entity, CancellationToken cancellationToken = default)
        {
            return await _innerClient.PutEntityAsync(entity, cancellationToken: cancellationToken);
        }

        protected async VoidTask ConvertLeadToOpportunityRaw(ConvertLeadToOpportunity lead, CancellationToken cancellationToken = default)
        {
            await _actionClient.ConvertLeadToOpportunityAsync(lead, cancellationToken);
        }

        public async VoidTask ConvertLeadToOpportunity(ConvertLeadToOpportunity lead, CancellationToken cancellationToken = default)
        {
            await ProcessSingle("Convert lead to opportunity", lead, ConvertLeadToOpportunityRaw, cancellationToken);
        }

        public async VoidTask ConvertLeadsToOpportunities(IEnumerable<ConvertLeadToOpportunity> leads, ExecutionTypeSettings executionTypeSettings, CancellationToken cancellationToken = default)
        {
            await ProcessAll("Convert leads to opportunities", leads, executionTypeSettings, ConvertLeadToOpportunityRaw, cancellationToken);
        }
    }
}
