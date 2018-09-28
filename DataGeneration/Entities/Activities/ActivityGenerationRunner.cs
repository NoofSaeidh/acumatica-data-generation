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

                var entities = await client.GetListAsync(entity, cancellationToken);

                _linkEntitiesKeys = new ConcurrentQueue<string>(entities.Select(e => e.GetNoteId().ToString()));
            }
        }

        protected override async VoidTask GenerateSingle(IApiClient client, Activity entity, CancellationToken cancellationToken)
        {
            entity.ReturnBehavior = ReturnBehavior.OnlySystem;
            var activity = await client.PutAsync(entity);
            if (_linkEntitiesKeys.TryTake(out var id))
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