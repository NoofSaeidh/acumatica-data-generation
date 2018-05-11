using CrmDataGeneration.Common;
using CrmDataGeneration.OpenApi;
using CrmDataGeneration.OpenApi.Reference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrmDataGeneration.Generation.Activities
{
    public class EmailApiWrappedClient : ApiWrappedClient<Email>
    {
        private readonly EmailClient _innerClient;

        public EmailApiWrappedClient(OpenApiState openApiState): base(openApiState)
        {
            _innerClient = new EmailClient(openApiState);
        }

        protected override async Task<Email> CreateRaw(Email entity, CancellationToken cancellationToken = default)
        {
            return await _innerClient.PutEntityAsync(entity, cancellationToken: cancellationToken);
        }
    }
}
