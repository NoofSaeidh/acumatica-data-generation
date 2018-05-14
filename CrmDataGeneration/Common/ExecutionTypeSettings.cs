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
            bool ignoreErrorsForEntities,
            bool ignoreErrorsForExecution)
        {
            ExecutionType = executionType;
            ParallelThreads = parallelThreads;
            IgnoreErrorsForEntities = ignoreErrorsForEntities;
            IgnoreErrorsForExecution = ignoreErrorsForExecution;
        }

        public ExecutionType ExecutionType { get; }


        public int ParallelThreads { get; }

        // ignore error for single entity.
        // process all entities even if one failed.
        public bool IgnoreErrorsForEntities { get; }

        // don't throw never
        public bool IgnoreErrorsForExecution { get; }
    }
}
