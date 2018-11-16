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
    public class GenerationLaunchSettings : IValidatable
    {
        private static int _id;
        public virtual int Id { get; } = Interlocked.Increment(ref _id);

        [RequiredCollection(AllowEmpty = false)]
        public ICollection<IGenerationSettings> GenerationSettings { get; set; }
        public GenerationSettingsInjection Injection { get; set; }

        // if true processing will be stopped if any generation option will fail
        // ignored for validation exceptions
        public bool StopProccesingAtExeception { get; set; }

        public IEnumerable<IGenerationSettings> GetPreparedGenerationSettings()
        {
            Validate();
            if (Injection != null)
                return Injection.Inject(GenerationSettings);

            return GenerationSettings.Select(g => g);
        }

        public void Validate()
        {
            ValidateHelper.ValidateObject(this);
        }
    }
}
