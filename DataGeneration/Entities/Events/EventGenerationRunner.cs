using DataGeneration.Common;
using DataGeneration.Entities.Activities;
using DataGeneration.Soap;
using System.Collections.Concurrent;
using System.Threading;
using VoidTask = System.Threading.Tasks.Task;

namespace DataGeneration.Entities.Events
{
    public class EventGenerationRunner : GenerationRunner<Event, EventGenerationSettings>
    {
        private IProducerConsumerCollection<(string noteId, Entity entity)> _linkEntities;

        public EventGenerationRunner(ApiConnectionConfig apiConnectionConfig, EventGenerationSettings generationSettings) : base(apiConnectionConfig, generationSettings)
        {
        }

        protected override async VoidTask RunBeforeGeneration(CancellationToken cancellationToken = default)
        {
            using (var client = await GetLoginLogoutClient(cancellationToken))
            {
                _linkEntities = await CrossEntityGenerationHelper.GetLinkEntitiesCollectionForActivityGeneration(
                    GenerationSettings,
                    client,
                    cancellationToken,
                    e =>
                    {
                        if (e is Opportunity o)
                        {
                            o.Address = new Address
                            {
                                AddressLine1 = new StringReturn(),
                                ReturnBehavior = ReturnBehavior.OnlySpecified
                            };
                        }
                    },
                    returnEntities: true);
            }
        }

        protected override async VoidTask GenerateSingle(IApiClient client, Event entity, CancellationToken cancellationToken)
        {
            if (_linkEntities.TryTake(out var item))
            {
                entity.ReturnBehavior = ReturnBehavior.OnlySystem;
                if (item.entity is Opportunity o)
                {
                    entity.Location = o?.Address?.AddressLine1?.Value;
                }
                var activity = await client.PutAsync(entity);

                var link = new LinkEntityToEvent
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