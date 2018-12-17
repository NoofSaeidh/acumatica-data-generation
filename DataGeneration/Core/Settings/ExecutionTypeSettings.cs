using Newtonsoft.Json;

namespace DataGeneration.Core.Settings
{
    public struct ExecutionTypeSettings
    {
        [JsonConstructor]
        public ExecutionTypeSettings(
            ExecutionType executionType,
            bool ignoreProcessingErrors = false,
            int parallelThreads = 1,
            int retryCount = 0)
        {
            ExecutionType = executionType;
            ParallelThreads = parallelThreads;
            RetryCount = retryCount;
            IgnoreProcessingErrors = ignoreProcessingErrors;
        }

        public ExecutionType ExecutionType { get; }

        public int ParallelThreads { get; }
        public int RetryCount { get; }

        // ignore error for single entity.
        // process all entities even if one failed.
        public bool IgnoreProcessingErrors { get; }

        public override string ToString() => ExecutionType.ToString()
            + (ExecutionType == ExecutionType.Parallel ? " Threads = " + ParallelThreads : null);

        public static ExecutionTypeSettings Sequent(bool ignoreErrorsForEntities = false)
        {
            return new ExecutionTypeSettings(ExecutionType.Sequent, ignoreErrorsForEntities);
        }

        public static ExecutionTypeSettings Parallel(int parallelThreads, bool ignoreErrorsForEntities = false)
        {
            return new ExecutionTypeSettings(ExecutionType.Parallel, ignoreErrorsForEntities, parallelThreads);
        }
    }

    public enum ExecutionType : byte
    {
        Sequent,
        Parallel
    }
}