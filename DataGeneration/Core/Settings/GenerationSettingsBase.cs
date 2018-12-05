using DataGeneration.Core.Api;
using System.Threading;

namespace DataGeneration.Core.Settings
{
    public abstract class GenerationSettingsBase : IGenerationSettings
    {
        private static int _id;

        public int Count { get; set; }
        public ExecutionTypeSettings ExecutionTypeSettings { get; set; }
        public abstract int? Seed { get; set; }
        public virtual string GenerationType { get; set; }
        // copy method ignores this
        // so setter used in injections
        public virtual int Id { get; internal set; } = Interlocked.Increment(ref _id);

        public abstract GenerationRunner GetGenerationRunner(ApiConnectionConfig apiConnectionConfig);
    }
}