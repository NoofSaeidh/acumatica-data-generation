using DataGeneration.Core;
using DataGeneration.Core.Common;
using DataGeneration.Core.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataGeneration.GenerationInfo
{
    public class BatchSettings : IValidatable
    {
        private static int _generationId;
        private static int GetGenerationId() => Interlocked.Increment(ref _generationId);

        private static int _id;
        // copy method ignores this
        // so setter used in injections
        public virtual int Id { get; internal set; } = Interlocked.Increment(ref _id);

        [RequiredCollection(AllowEmpty = false)]
        public ICollection<IGenerationSettings> GenerationSettings { get; set; }
        public ICollection<JsonInjection<IGenerationSettings>> Injections { get; set; }

        // if true processing will be stopped if any generation option will fail
        // ignored for validation exceptions
        public bool StopProcessingAtException { get; set; }
        public bool RestartIisBeforeBatch { get; set; }
        public bool CollectGarbageBeforeBatch { get; set; }

        public IEnumerable<IGenerationSettings> GetPreparedGenerationSettings()
        {
            Validate();

            foreach (var settings in GenerationSettings)
            {
                var s = settings.Copy();
                JsonInjection.Inject(s, Injections);
                if (s is GenerationSettingsBase gs)
                    gs.Id = GetGenerationId();
                yield return s;
            }
        }

        public Batch CompileBatch() => new Batch(this);

        public void Validate()
        {
            ValidateHelper.ValidateObject(this);
        }
    }
}
