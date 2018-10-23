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

namespace DataGeneration.Entities.Activities
{
    public class LinkActivitiesGenerationRunner : EntitiesSearchGenerationRunner<OneToManyRelation<Entity, Activity>, LinkActivitiesGenerationSettings>
    {
        public LinkActivitiesGenerationRunner(ApiConnectionConfig apiConnectionConfig, LinkActivitiesGenerationSettings generationSettings) : base(apiConnectionConfig, generationSettings)
        {
        }

        protected override void UtilizeFoundEntities(IList<Entity> entities)
        {
            ChangeGenerationCount(entities.Count, "To be equal to found entities count.");

            GenerationSettings.RandomizerSettings.LinkEntities = new ConcurrentQueue<Entity>(entities);
        }

        protected override async VoidTask GenerateSingle(IApiClient client, OneToManyRelation<Entity, Activity> entity, CancellationToken cancellationToken)
        {
            var noteId = entity.Left.GetNoteId().ToString();
            if (noteId.IsNullOrEmpty())
                throw new InvalidOperationException("NoteId must be not empty for linked entity.");

            foreach (var activity in entity.Right)
            {
                activity.ReturnBehavior = ReturnBehavior.OnlySystem;
                var resActivity = await client.PutAsync(activity, cancellationToken);

                var link = new LinkEntityToActivity
                {
                    RelatedEntity = noteId,
                    Type = GenerationSettings.PxTypeForLinkedEntity
                };

                await client.InvokeAsync(resActivity, link, cancellationToken);
            }
        }
    }
}
