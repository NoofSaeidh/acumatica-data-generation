using DataGeneration.Common;
using DataGeneration.Soap;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using VoidTask = System.Threading.Tasks.Task;

namespace DataGeneration.Entities.Activities
{
    public class ActivityGenerationRunner : GenerationRunner<Activity, ActivityGenerationSettings>
    {
        private IProducerConsumerCollection<string> _linkEntitiesKeys;
        private string _pxTypeName;

        public ActivityGenerationRunner(ApiConnectionConfig apiConnectionConfig, ActivityGenerationSettings generationSettings)
            : base(apiConnectionConfig, generationSettings)
        {
        }

        // everything will crash if run two generation simultaneously

        protected override async VoidTask RunBeforeGeneration(CancellationToken cancellationToken = default)
        {
            using (var client = await GetLoginLogoutClient(cancellationToken))
            {
                _pxTypeName = GenerationSettings.PxTypeForLinkedEntity;
                var entity = EntityHelper.InitializeFromType(GenerationSettings.EntityTypeForLinkedEntity);
                entity.ReturnBehavior = ReturnBehavior.OnlySpecified;
                EntityHelper.SetPropertyValue(entity, "NoteID", new GuidReturn());
                if (GenerationSettings.CreatedAtSearchRange != null)
                {
                    var (start, end) = GenerationSettings.CreatedAtSearchRange.Value;
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

                var entities = await client.GetListAsync(entity, cancellationToken);

                _linkEntitiesKeys = new ConcurrentQueue<string>(entities.Select(e => e.GetNoteId().ToString()));

                // adjust count
                if (GenerationSettings.EntitiesCountProbability != null)
                {
                    GenerationSettings.Count = (int)(RandomizerSettingsBase
                        .GetRandomizer(GenerationSettings.RandomizerSettings)
                        .Double(0, GenerationSettings.EntitiesCountProbability.Value) * _linkEntitiesKeys.Count);
                }
            }
        }

        protected override async VoidTask GenerateSingle(IApiClient client, Activity entity, CancellationToken cancellationToken)
        {
            if (_linkEntitiesKeys.TryTake(out var id))
            {
                entity.ReturnBehavior = ReturnBehavior.OnlySystem;
                if (entity.TimeActivity != null)
                    entity.TimeActivity.ReturnBehavior = ReturnBehavior.None;
                var activity = await client.PutAsync(entity);

                var link = new LinkEntityToActivity
                {
                    RelatedEntity = id,
                    Type = _pxTypeName
                };

                await client.InvokeAsync(activity, link);
            }
        }
    }
}