using DataGeneration.Core;
using DataGeneration.Core.Common;
using System.Collections.Generic;
using System.Linq;

namespace Bogus
{
    public static class BogusExtensions
    {
        // just wrapper for WeightedRandom method with ProbabilityCollection
        public static T ProbabilityRandom<T>(this Randomizer randomizer, ProbabilityCollection<T> probabilities)
        {
            // need null checks??

            if (!probabilities.HasDefinedProbabilities)
                return randomizer.ListItem(probabilities.AsList.ToList());

            var itemsWithProbs = probabilities.GetItemsWithProbabilities(true).ToArray();
            var items = itemsWithProbs.Select(i => i.Key).ToArray();
            var weights = itemsWithProbs.Select(i => (float)i.Value).ToArray();

            return randomizer.WeightedRandom(items, weights);
        }

        public static T ProbabilityRandomIfAny<T>(this Randomizer randomizer, ProbabilityCollection<T> probabilities)
        {
            if (probabilities.IsNullOrEmpty())
                return default;
            return randomizer.ProbabilityRandom(probabilities);
        }

        public static int Int(this Randomizer randomizer, (int, int) value) => randomizer.Int(value.Item1, value.Item2);

        public static T WeightedRandom<T>(this Randomizer randomizer, params (T element, float weight)[] items)
        {
            var elements = new T[items.Length];
            var weights = new float[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                elements[i] = items[i].element;
                weights[i] = items[i].weight;
            }
            return randomizer.WeightedRandom(elements, weights);
        }

        public static IEnumerator<T> GuaranteedRandomEnumerator<T>(this Randomizer randomizer, params (T element, int count)[] items)
        {
            var picked = new int[items.Length];

            var options = Enumerable.Range(0, items.Length).ToList();

            while(options.Count > 0)
            {
                var i = randomizer.CollectionItem(options);
                var (element, count) = items[i];

                yield return element;

                // zero or below traited like endless options
                if(count > 0 && count == ++picked[i]) 
                {
                    options.Remove(i);
                }
            }
        }
    }
}