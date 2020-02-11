﻿using DataGeneration.Core;
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
    }
}