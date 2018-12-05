using DataGeneration.Entities;
using DataGeneration.Soap;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace DataGeneration.Core.Settings
{
    public abstract class GenerationSettings<T, TRandomizerSettings> : 
        GenerationSettingsBase, 
        IGenerationSettings<T>
            where T : class
            where TRandomizerSettings : IRandomizerSettings<T>
    {
        public string PxType { get; set; }

        public override string GenerationType { get => base.GenerationType ?? typeof(T).Name; set => base.GenerationType = value; }
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

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IRandomizerSettings<T> IGenerationSettings<T>.RandomizerSettings => RandomizerSettings;

        public virtual void Validate()
        {
            ValidateHelper.ValidateObject(this);
            RandomizerSettings.Validate();
        }
    }
}