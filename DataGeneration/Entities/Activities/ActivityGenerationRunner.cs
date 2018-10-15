using DataGeneration.Common;
using DataGeneration.Soap;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VoidTask = System.Threading.Tasks.Task;

namespace DataGeneration.Entities.Activities
{
    public class ActivityGenerationRunner : GenerationRunner<Activity, ActivityGenerationSettings>
    {
        private IProducerConsumerCollection<(string noteId, Entity entity)> _linkEntities;

        public ActivityGenerationRunner(ApiConnectionConfig apiConnectionConfig, ActivityGenerationSettings generationSettings)
            : base(apiConnectionConfig, generationSettings)
        {
        }

        // everything will crash if run two generation simultaneously
        protected override async VoidTask RunBeforeGeneration(CancellationToken cancellationToken = default)
        {
            using (var client = await GetLoginLogoutClient(cancellationToken))
            {
                _linkEntities = await CrossEntityGenerationHelper.GetLinkEntitiesCollectionForActivityGeneration(GenerationSettings, client, cancellationToken);
            }
        }

        protected override async VoidTask GenerateSingle(IApiClient client, Activity entity, CancellationToken cancellationToken)
        {
            if (_linkEntities.TryTake(out var item))
            {
                entity.ReturnBehavior = ReturnBehavior.OnlySystem;
                var activity = await client.PutAsync(entity);

                var link = new LinkEntityToActivity
                {
                    RelatedEntity = item.noteId,
                    Type = GenerationSettings.PxTypeForLinkedEntity
                };

                await client.InvokeAsync(activity, link);
            }
            else
                Logger.Warn("No entities remain for generation. Possible counts mismatch.");
        }
    }
}