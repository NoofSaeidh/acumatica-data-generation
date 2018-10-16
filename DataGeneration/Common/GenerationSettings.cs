using DataGeneration.Entities;
using DataGeneration.Soap;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace DataGeneration.Common
{
    public abstract class GenerationSettingsBase : IGenerationSettings
    {
        public int Count { get; set; }
        public ExecutionTypeSettings ExecutionTypeSettings { get; set; }
        public abstract int? Seed { get; set; }
        public abstract string GenerationEntity { get; }

        public abstract GenerationRunner GetGenerationRunner(ApiConnectionConfig apiConnectionConfig);
    }

    public abstract class GenerationSettings<T, TRandomizerSettings> : GenerationSettingsBase, IGenerationSettings<T> 
        where T : class
        where TRandomizerSettings : IRandomizerSettings<T>
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
        public TRandomizerSettings RandomizerSettings { get; set; }

        IRandomizerSettings<T> IGenerationSettings<T>.RandomizerSettings => RandomizerSettings;

        public virtual void Validate()
        {
            ValidateHelper.ValidateObject(this);
            RandomizerSettings.Validate();
        }
    }
}