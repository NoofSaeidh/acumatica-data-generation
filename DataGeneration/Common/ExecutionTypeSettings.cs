using Newtonsoft.Json;

namespace DataGeneration.Common
{
    public struct ExecutionTypeSettings
    {
        [JsonConstructor]
        public ExecutionTypeSettings(
            ExecutionType executionType,
            bool ignoreProcessingErrors = false,
            int parallelThreads = 1)
        {
            ExecutionType = executionType;
            ParallelThreads = parallelThreads;
            IgnoreProcessingErrors = ignoreProcessingErrors;
        }

        public ExecutionType ExecutionType { get; }

        public int ParallelThreads { get; }

        // ignore error for single entity.
        // process all entities even if one failed.
        public bool IgnoreProcessingErrors { get; }

        public static ExecutionTypeSettings Sequent(bool ignoreErrorsForEntities = false)
        {
            return new ExecutionTypeSettings(ExecutionType.Sequent, ignoreErrorsForEntities);
        }

        public static ExecutionTypeSettings Parallel(int parallelThreads, bool ignoreErrorsForEntities = false)
        {
            return new ExecutionTypeSettings(ExecutionType.Parallel, ignoreErrorsForEntities, parallelThreads);
        }
    }
}