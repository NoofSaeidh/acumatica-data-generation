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
        public override async VoidTask RunGeneration(CancellationToken cancellationToken = default)
        {
            using (var client = await GetLoginLogoutClient())
            {
                _pxTypeName = GenerationSettings.PxTypeNameForLinkedEntity;
                var entity = EntityHelper.InitializeFromType(GenerationSettings.EntityTypeName);

                entity.ReturnBehavior = ReturnBehavior.OnlySpecified;
                EntityHelper.SetPropertyValue(entity, "NoteID", new StringReturn());
                var list = await client.GetListAsync(entity);

                _linkEntitiesKeys = new ConcurrentQueue<string>(list.Select(e => e.GetNoteId().ToString()));
            }

            await base.RunGeneration(cancellationToken);
        }

        protected override async VoidTask GenerateSingle(IApiClient client, Activity entity, CancellationToken cancellationToken)
        {
            Activity activity;
            using (StopwatchLoggerFactory.Log("Put Activity"))
            {
                entity.ReturnBehavior = ReturnBehavior.OnlySystem;
                activity = await client.PutAsync(entity);
            }
            if (_linkEntitiesKeys.TryTake(out var id))
            {
                using (StopwatchLoggerFactory.Log("Link Activity"))
                {
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
}