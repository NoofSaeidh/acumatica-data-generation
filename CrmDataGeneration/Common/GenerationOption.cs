using CrmDataGeneration.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrmDataGeneration.Common
{
    /// <summary>
    ///     Options for generation. 
    /// Contains all required info for start generation 
    /// and method for start generation that can contain customized logic.
    /// </summary>
    public abstract class GenerationOption
    {
        public abstract string GenerateEntity { get; }
        public int Count { get; set; }
        public ExecutionTypeSettings ExecutionTypeSettings { get; set; }
        public abstract Task RunGeneration(GeneratorClient client, CancellationToken cancellationToken = default);
    }
    // it may be helpful further (now just mark entity)
    public abstract class GenerationOption<T> : GenerationOption
        where T : OpenApi.Reference.Entity
    {
        private string _pxTypeName;

        [JsonIgnore]
        protected string PxTypeName
        {
            get
            {
                if (_pxTypeName != null)
                    return _pxTypeName;
                return _pxTypeName = PxObjectsTypes.GetEntityPxTypeName<T>();
            }
        }

        public virtual IRandomizerSettings<T> RandomizerSettings { get; set; }
        public override string GenerateEntity => typeof(T).Name;
        public override async Task RunGeneration(GeneratorClient client, CancellationToken cancellationToken = default)
        {
            CheckSettings();
            await client.GenerateAll(this, cancellationToken);
        }

        protected virtual void CheckSettings()
        {
            if (RandomizerSettings == null)
                throw new InvalidOperationException($"{nameof(RandomizerSettings)} cannot be null.");
        }
    }
}
