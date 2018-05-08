using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrmDataGeneration.Common
{
    public abstract class GenerationOption
    {
        public int Count { get; set; }
        public abstract string GenerateEntity { get; }
        public bool GenerateInParallel { get; set; }
        public int MaxExecutionThreadsParallel { get; set; } // For parallel
        public bool SkipErrorsSequent { get; set; } // For sequent
        public abstract Task RunGeneration(GeneratorClient client, CancellationToken cancellationToken = default);
    }
    // it may be helpful further (now just mark entity)
    public abstract class GenerationOption<T> : GenerationOption
        where T : OpenApi.Reference.Entity
    {
        public virtual IRandomizerSettings<T> RandomizerSettings { get; set; }
        public override string GenerateEntity => typeof(T).Name;
        public override async Task RunGeneration(GeneratorClient client, CancellationToken cancellationToken = default)
        {
            if (RandomizerSettings == null)
                throw new InvalidOperationException($"{nameof(RandomizerSettings)} cannot be null.");

            await client.GenerateAll(this, cancellationToken);
        }
    }
}
