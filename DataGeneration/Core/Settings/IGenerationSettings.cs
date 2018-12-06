using DataGeneration.Core.Api;
using DataGeneration.Core.Common;

namespace DataGeneration.Core.Settings
{
    public interface IGenerationSettings
    {
        int Count { get; set; }
        string GenerationType { get; }

        // get, set seed for randomizer settings
        int? Seed { get; set; }
        int Id { get; }

        bool CollectGarbageBeforeGeneration { get; }

        ExecutionTypeSettings ExecutionTypeSettings { get; set; }

        GenerationRunner GetGenerationRunner(ApiConnectionConfig apiConnectionConfig);
    }

    public interface IGenerationSettings<T> : IGenerationSettings, IValidatable
    {
        IRandomizerSettings<T> RandomizerSettings { get; }
    }
}