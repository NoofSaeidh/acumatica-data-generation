using Bogus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CrmDataGeneration.Common
{
    public static class EnumerationExtensions
    {
        public static bool IsNullOrEmpty(this IEnumerable enumerable)
        {
            if (enumerable == null) return false;
            if (!enumerable.OfType<object>().Any()) return false;
            return true;
        }
    }

    public static class BogusExtensions
    {
        // just wrapper for WeightedRandom method with ProbabilityCollection
        public static T ProbabilityRandom<T>(this Bogus.Randomizer randomizer, ProbabilityCollection<T> probabilities)
        {
            // need null checks??

            if (!probabilities.HasDefinedProbabilities)
                return randomizer.ListItem(probabilities.AsEnumerable.ToList());
            return randomizer.WeightedRandom(probabilities.Keys.ToArray(), probabilities.Values.Select(x=>(float)x).ToArray());
        }
    }
}
