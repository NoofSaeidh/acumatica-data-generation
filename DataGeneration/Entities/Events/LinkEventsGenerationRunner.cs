using DataGeneration.Common;
using DataGeneration.Entities.Activities;
using DataGeneration.Soap;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using VoidTask = System.Threading.Tasks.Task;

namespace DataGeneration.Entities.Events
{
    public class LinkEventsGenerationRunner : EntitiesSearchGenerationRunner<OneToManyRelation<Entity, Event>, LinkEventsGenerationSettings>
    {
        public LinkEventsGenerationRunner(ApiConnectionConfig apiConnectionConfig, LinkEventsGenerationSettings generationSettings) : base(apiConnectionConfig, generationSettings)
        {
        }

        protected override void UtilizeFoundEntities(IList<Entity> entities)
        {
            ChangeGenerationCount(entities.Count, "To be equal to found entities count.");

            GenerationSettings.RandomizerSettings.LinkEntities = new ConcurrentQueue<Entity>(entities);
        }

        protected override async VoidTask GenerateSingle(IApiClient client, OneToManyRelation<Entity, Event> entity, CancellationToken cancellationToken)
        {
            var noteId = entity.Left.GetNoteId().ToString();
            if (noteId.IsNullOrEmpty())
                throw new InvalidOperationException("NoteId must be not empty for linked entity.");

            foreach (var @event in entity.Right)
            {
                @event.ReturnBehavior = ReturnBehavior.OnlySystem;
                var resEvent = await client.PutAsync(@event, cancellationToken);

                var link = new LinkEntityToEvent
                {
                    RelatedEntity = noteId,
                    Type = GenerationSettings.PxTypeForLinkedEntity
                };

                await client.InvokeAsync(resEvent, link, cancellationToken);
            }
        }
    }
}