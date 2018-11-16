using System.Collections.Generic;
using System.Linq;

namespace DataGeneration.GenerationInfo
{
    public class AllGenerationsResult
    {
        internal AllGenerationsResult(IEnumerable<GenerationResult> generationResults, bool processingStopped = false)
        {
            GenerationResults = generationResults.ToArray();
            ProcessingStopped = processingStopped;
        }

        public bool ProcessingStopped { get; }
        public bool AllSucceeded => GenerationResults.All(g => g.Success);
        public bool AllFailed => GenerationResults.All(g => !g.Success);
        public IReadOnlyCollection<GenerationResult> GenerationResults { get; }
    }
}