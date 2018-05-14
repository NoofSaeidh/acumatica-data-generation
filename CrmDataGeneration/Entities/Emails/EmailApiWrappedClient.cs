using CrmDataGeneration.Common;
using CrmDataGeneration.OpenApi;
using CrmDataGeneration.OpenApi.Reference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VoidTask = System.Threading.Tasks.Task;

namespace CrmDataGeneration.Entities.Emails
{
    public class EmailApiWrappedClient : ApiWrappedClient<Email>
    {
        private readonly EmailClient _innerClient;
        private readonly InvokeActionClient _actionClient;

        public EmailApiWrappedClient(OpenApiState openApiState): base(openApiState)
        {
            _innerClient = new EmailClient(openApiState);
            _actionClient = new InvokeActionClient(openApiState);
        }

        protected override async Task<Email> CreateRaw(Email entity, CancellationToken cancellationToken = default)
        {
            return await _innerClient.PutEntityAsync(entity, cancellationToken: cancellationToken);
        }

        public async Task<IEnumerable<Email>> CreateAllAndLinkEntities(IEnumerable<SelectRelatedEntityEmail> emailsAndRelations, ExecutionTypeSettings executionTypeSettings, CancellationToken cancellationToken = default)
        {
            return await ProcessAll("Create emails and link entities", emailsAndRelations, executionTypeSettings, async (e, t) =>
            {
                var entity = await CreateRaw(e.Entity, cancellationToken);
                e.Entity = entity;
                await LinkEntityRaw(e, cancellationToken);
                return entity;
            }, cancellationToken);
        }

        public async VoidTask LinkEntity(SelectRelatedEntityEmail relation, CancellationToken cancellationToken = default)
        {
            await ProcessSingle("Link entity", relation, LinkEntityRaw, cancellationToken);
        }

        public async VoidTask LinkEntities(IEnumerable<SelectRelatedEntityEmail> relations, ExecutionTypeSettings executionTypeSettings, CancellationToken cancellationToken = default)
        {
            await ProcessAll("Link entities", relations, executionTypeSettings, LinkEntityRaw, cancellationToken);
        }

        protected async VoidTask LinkEntityRaw(SelectRelatedEntityEmail relation, CancellationToken cancellationToken = default)
        {
            await _actionClient.SelectRelatedEntityEmailAsync(relation, cancellationToken);
        }
    }
}
