using CrmDataGeneration.Common;
using CrmDataGeneration.Core;
using CrmDataGeneration.OpenApi;
using CrmDataGeneration.OpenApi.Reference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmDataGeneration.Generation.Leads
{
    public class LeadApiWrappedClient : ApiWrappedClient<Lead>
    {
        public LeadApiWrappedClient(OpenApiState openApiState) : base(openApiState)
        {
        }

        protected override Task<IEnumerable<Lead>> CreateAllRaw(IEnumerable<Lead> entity)
        {
            throw new NotImplementedException();
        }

        protected override Task<Lead> CreateRaw(Lead entity)
        {
            throw new NotImplementedException();
        }
    }
}
