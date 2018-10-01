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


        internal static async Task<IProducerConsumerCollection<(string noteId, Entity entity)>> BeforeActivityGeneration(
            IActivityGenerationSettings generationSettings,
            IApiClient apiClient,
            CancellationToken cancellationToken,
            Action<Entity> searchEntityAdjustmet = null,
            bool returnEntities = false)
        {
            var entity = EntityHelper.InitializeFromType(generationSettings.EntityTypeForLinkedEntity);
            entity.ReturnBehavior = ReturnBehavior.OnlySpecified;
            searchEntityAdjustmet?.Invoke(entity);
            EntityHelper.SetPropertyValue(entity, "NoteID", new GuidReturn());
            if (generationSettings.CreatedAtSearchRange != null)
            {
                var (start, end) = generationSettings.CreatedAtSearchRange.Value;
                if (start != null || end != null)
                {
                    var date = new DateTimeSearch();
                    if (start != null && end != null)
                    {
                        date.Value = start;
                        date.Value2 = end;
                        date.Condition = DateTimeCondition.IsBetween;
                    }
                    else if (start != null)
                    {
                        date.Value = start;
                        date.Condition = DateTimeCondition.IsGreaterThanOrEqualsTo;
                    }
                    else if (end != null)
                    {
                        date.Value = end;
                        date.Condition = DateTimeCondition.IsLessThanOrEqualsTo;
                    }

                    EntityHelper.SetPropertyValue(entity, "CreatedAt", date);
                }
            }

            var entities = await apiClient.GetListAsync(entity, cancellationToken);
            var search = (returnEntities
                ? entities.Select(e => (e.GetNoteId().ToString(), e))
                : entities.Select(e => (e.GetNoteId().ToString(), (Entity)null))
                ).ToArray();

            // adjust count
            if (generationSettings.EntitiesCountProbability != null)
            {
                var randomizer = RandomizerSettingsBase
                    .GetRandomizer(generationSettings.Seed ?? throw new InvalidOperationException()); //it should not fail

                search = randomizer.ArrayElements(search, generationSettings.Count).ToArray();

                generationSettings.Count = search.Length;
            }

            return new ConcurrentQueue<(string, Entity)>(search);
        }


        // everything will crash if run two generation simultaneously
        protected override async VoidTask RunBeforeGeneration(CancellationToken cancellationToken = default)
        {
            using (var client = await GetLoginLogoutClient(cancellationToken))
            {
                _linkEntities = await BeforeActivityGeneration(GenerationSettings, client, cancellationToken);
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