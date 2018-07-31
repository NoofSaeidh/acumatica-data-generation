using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmDataGeneration.Common
{
    public struct ExecutionTypeSettings
    {
        [JsonConstructor]
        public ExecutionTypeSettings(
            ExecutionType executionType,
            int parallelThreads,
            bool ignoreProcessingErrors)
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
            return new ExecutionTypeSettings(ExecutionType.Sequent, 0, ignoreErrorsForEntities);
        }

        public static ExecutionTypeSettings Parallel(int parallelThreads, bool ignoreErrorsForEntities = false)
        {
            return new ExecutionTypeSettings(ExecutionType.Parallel, parallelThreads, ignoreErrorsForEntities);
        }
    }
}
