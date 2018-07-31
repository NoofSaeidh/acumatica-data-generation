using CrmDataGeneration.Entities;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrmDataGeneration.Common
{
    public abstract class BaseGenerationSettings
    {
        public int Count { get; set; }
        public ExecutionTypeSettings ExecutionTypeSettings { get; set; }
        public abstract string GenerationEntity { get; }

        public abstract GenerationRunner GetGenerationRunner(ApiConnectionConfig apiConnectionConfig);

        internal virtual BaseRandomizerSettings BaseRandomizerSettings { get; }
    }

    public abstract class GenerationSettings<T> : BaseGenerationSettings, IGenerationSettings<T> where T : Soap.Entity
    {
        private string _pxTypeName;
        [JsonIgnore]
        public string PxTypeName => _pxTypeName ?? (_pxTypeName = PxObjectsTypes.GetEntityPxTypeName<T>());

        public override string GenerationEntity => typeof(T).Name;

        [Required]
        public IRandomizerSettings<T> RandomizerSettings { get; set; }
        internal override BaseRandomizerSettings BaseRandomizerSettings => RandomizerSettings as BaseRandomizerSettings;

        public virtual void Validate()
        {
            ValidateHelper.ValidateObject(this);
            RandomizerSettings.Validate();
        }

    }
}
