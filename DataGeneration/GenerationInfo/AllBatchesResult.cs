using System.Collections.Generic;
using System.Linq;

namespace DataGeneration.GenerationInfo
{
    public class AllBatchesResult
    {
        internal AllBatchesResult(IEnumerable<AllGenerationsResult> generationResults)
        {
            GenerationResults = generationResults.ToArray();
        }

        public bool AllSucceeded => GenerationResults.All(g => g.AllSucceeded);
        public bool AllFailed => GenerationResults.All(g => g.AllFailed);
        public IReadOnlyCollection<AllGenerationsResult> GenerationResults { get; }
    }
}
