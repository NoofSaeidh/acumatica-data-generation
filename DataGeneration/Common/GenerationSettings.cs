using DataGeneration.Entities;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace DataGeneration.Common
{
    public abstract class GenerationSettingsBase : IGenerationSettings
    {
        public int Count { get; set; }
        public ExecutionTypeSettings ExecutionTypeSettings { get; set; }
        public abstract int? Seed { get; set; }
        public abstract string GenerationEntity { get; }

        public abstract GenerationRunner GetGenerationRunner(ApiConnectionConfig apiConnectionConfig);

        internal virtual RandomizerSettingsBase RandomizerSettingsBase { get; }
    }

    public abstract class GenerationSettings<T> : GenerationSettingsBase, IGenerationSettings<T> where T : Soap.Entity
    {
        public string PxType { get; set; }

        public override string GenerationEntity => typeof(T).Name;

        public override int? Seed
        {
            get => RandomizerSettings?.Seed;
            set
            {
                if (RandomizerSettings != null && value != null)
                {
                    RandomizerSettings.Seed = (int)value;
                }
            }
        }

        [Required]
        public IRandomizerSettings<T> RandomizerSettings { get; set; }

        internal override RandomizerSettingsBase RandomizerSettingsBase => RandomizerSettings as RandomizerSettingsBase;

        public virtual void Validate()
        {
            ValidateHelper.ValidateObject(this);
            RandomizerSettings.Validate();
        }
    }
}