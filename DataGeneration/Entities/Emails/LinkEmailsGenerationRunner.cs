using DataGeneration.Common;
using DataGeneration.Soap;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VoidTask = System.Threading.Tasks.Task;

namespace DataGeneration.Entities.Emails
{
    public class LinkEmailsGenerationRunner : EntitiesSearchGenerationRunner<OneToManyRelation<LinkEntityToEmail, Email>, LinkEmailsGenerationSettings>
    {
        public LinkEmailsGenerationRunner(ApiConnectionConfig apiConnectionConfig, LinkEmailsGenerationSettings generationSettings) : base(apiConnectionConfig, generationSettings)
        {
        }

        protected override void UtilizeFoundEntities(IList<Entity> entities)
        {
            // not in AdjustEntitySearcher because email may be taken (as in case) from other entities 
            // and it is processed only after search
            entities = entities
                .GetEnumerableAdjuster()
                .AdjustCast<IEmailEntity>(en => en.Where(e => e.Email != null))
                .Value
                .ToList();

            ChangeGenerationCount(entities.Count, "To be equal to found entities count.");

            GenerationSettings.RandomizerSettings.LinkEntities = new ConcurrentQueue<Entity>(entities);
        }

        protected override async VoidTask GenerateSingle(IApiClient client, OneToManyRelation<LinkEntityToEmail, Email> entity, CancellationToken cancellationToken)
        {
            foreach (var email in entity.Right)
            {
                await client.InvokeAsync(await client.PutAsync(email, cancellationToken), entity.Left, cancellationToken);
            }
        }
    }
}
