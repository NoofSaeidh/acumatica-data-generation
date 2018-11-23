using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataGeneration.Common
{
    public static class EnumerationExtensions
    {
        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static string FormatWith(this string value, params object[] args) => string.Format(value, args);

        public static bool IsNullOrEmpty(this IEnumerable enumerable)
        {
            if (enumerable == null) return true;
            if (!enumerable.OfType<object>().Any()) return true;
            return false;
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null) return true;
            if (enumerable is ICollection col) return col.Count == 0;
            if (!enumerable.Any()) return true;
            return false;
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> enumerable, T element)
        {
            return enumerable.Concat(Enumerable.Repeat(element, 1));
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> enumerable, params T[] elements)
        {
            return enumerable.Concat((IEnumerable<T>)elements);
        }

        public static ICollection<T> AddMany<T>(this ICollection<T> collection, params T[] elements)
        {
            if (collection is List<T> list)
            {
                list.AddRange(elements);
                return list;
            }
            foreach (var item in elements)
            {
                collection.Add(item);
            }
            return collection;
        }

        public static bool ContainsOnly<T>(this IEnumerable<T> enumerable, T element)
        {
            if(enumerable is ICollection<T> collection)
            {
                return collection.Count == 1 && collection.Contains(element);
            }
            return enumerable.Count() == 1 && enumerable.Contains(element);
        }

        public static bool ContainsOnlyAnyOf<T>(this IEnumerable<T> enumerable, params T[] elements)
        {
            foreach (var item in enumerable)
            {
                if (!elements.Contains(item))
                    return false;
            }
            return true;
        }

        public static T[] AsArray<T>(this T value) => new T[] { value };

        public static IEnumerable<T> AsEnumerable<T>(this T value) => Enumerable.Repeat(value, 1);

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if(source is List<T> list)
            {
                list.ForEach(action);
                return list;
            }
            if(source is T[] array)
            {
                Array.ForEach(array, action);
                return array;
            }
            return source.Select(i =>
            {
                action(i);
                return i;
            });
        }

        public static bool HasValue<T>(this T? nullable, out T value) where T : struct
        {
            value = nullable.GetValueOrDefault();
            return nullable.HasValue;
        }

        public static bool SetIfHasValue<T>(this T? nullable, ref T setter) where T : struct
        {
            if (!nullable.HasValue(out var v))
                return false;
            setter = v;
            return true;
        }

        public static bool HasValue<T>(this T instance, out T value) where T : class
        {
            value = instance;
            return instance != null;
        }
    }

    public static class BogusExtensions
    {
        // just wrapper for WeightedRandom method with ProbabilityCollection
        public static T ProbabilityRandom<T>(this Bogus.Randomizer randomizer, ProbabilityCollection<T> probabilities)
        {
            // need null checks??

            if (!probabilities.HasDefinedProbabilities)
                return randomizer.ListItem(probabilities.AsList.ToList());

            var itemsWithProbs = probabilities.GetItemsWithProbabilities(true).ToArray();
            var items = itemsWithProbs.Select(i => i.Key).ToArray();
            var weights = itemsWithProbs.Select(i => (float)i.Value).ToArray();

            return randomizer.WeightedRandom(items, weights);
        }

        public static T ProbabilityRandomIfAny<T>(this Bogus.Randomizer randomizer, ProbabilityCollection<T> probabilities)
        {
            if (probabilities.IsNullOrEmpty())
                return default;
            return randomizer.ProbabilityRandom(probabilities);
        }

        public static int Int(this Bogus.Randomizer randomizer, (int, int) value) => randomizer.Int(value.Item1, value.Item2);
    }

    public static class TaskExtensions
    {
        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            // Create a self-cancelling TaskCompletionSource
            var tcs = new TaskCompletionSourceWithCancellation<T>(cancellationToken);

            // Wait for completion or cancellation
            return await await Task.WhenAny(task, tcs.Task);
        }

        public static async Task WithCancellation(this Task task, CancellationToken cancellationToken)
        {
            // Create a self-cancelling TaskCompletionSource
            var tcs = new TaskCompletionSourceWithCancellation<object>(cancellationToken);

            // Wait for completion or cancellation
            await await Task.WhenAny(task, tcs.Task);
        }

        private class TaskCompletionSourceWithCancellation<TResult> : TaskCompletionSource<TResult>
        {
            public TaskCompletionSourceWithCancellation(CancellationToken cancellationToken)
            {
                CancellationTokenRegistration registration =
                    cancellationToken.Register(() => TrySetCanceled());

                // Remove the registration after the task completes
                Task.ContinueWith(_ => registration.Dispose());
            }
        }
    }
}