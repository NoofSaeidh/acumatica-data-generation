using DataGeneration.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataGeneration.GenerationInfo
{
    public class LaunchSettings : IValidatable
    {
        private static int _id;
        public virtual int Id { get; } = Interlocked.Increment(ref _id);

        [RequiredCollection(AllowEmpty = false)]
        public ICollection<IGenerationSettings> GenerationSettings { get; set; }
        public ICollection<JsonInjection> Injections { get; set; }

        // if true processing will be stopped if any generation option will fail
        // ignored for validation exceptions
        public bool StopProccesingAtExeception { get; set; }

        public IEnumerable<IGenerationSettings> GetPreparedGenerationSettings()
        {
            Validate();
            var settings = GenerationSettings.Select(g => g.Copy());
            if (Injections != null)
            {
                foreach (var setting in settings)
                {
                    JsonInjection.Inject(setting, Injections);
                }
            }
            return settings;
        }

        public void Validate()
        {
            ValidateHelper.ValidateObject(this);
        }
    }
}
