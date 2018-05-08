using CrmDataGeneration.Common;
using CrmDataGeneration.OpenApi;
using CrmDataGeneration.OpenApi.Reference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrmDataGeneration.Generation.Leads
{
    public class LeadApiWrappedClient : ApiWrappedClient<Lead>
    {
        private readonly LeadClient _innerClient;
        public LeadApiWrappedClient(OpenApiState openApiState) : base(openApiState)
        {
            _innerClient = new LeadClient(openApiState);
        }

        protected override async Task<Lead> CreateRaw(Lead entity, CancellationToken cancellationToken = default)
        {
            return await _innerClient.PutEntityAsync(entity, cancellationToken: cancellationToken);
        }
    }
}
