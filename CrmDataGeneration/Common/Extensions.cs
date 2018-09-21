using Bogus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrmDataGeneration.Common
{
    public static class EnumerationExtensions
    {
        public static bool IsNullOrEmpty(this IEnumerable enumerable)
        {
            if (enumerable == null) return true;
            if (!enumerable.OfType<object>().Any()) return true;
            return false;
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> enumerable, T element)
        {
            return enumerable.Concat(Enumerable.Repeat(element, 1));
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> enumerable, params T[] elements)
        {
            return enumerable.Concat(elements);
        }

        public static ICollection<T> AddMany<T>(this ICollection<T> collection, params T[] elements)
        {
            if(collection is List<T> list)
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
    }

    public static class BogusExtensions
    {
        // just wrapper for WeightedRandom method with ProbabilityCollection
        public static T ProbabilityRandom<T>(this Bogus.Randomizer randomizer, ProbabilityCollection<T> probabilities)
        {
            // need null checks??

            if (!probabilities.HasDefinedProbabilities)
                return randomizer.ListItem(probabilities.AsList.ToList());
            return randomizer.WeightedRandom(probabilities.AsList.ToArray(), probabilities.Probabilities.Select(x => (float)x).ToArray());
        }

        public static T ProbabilityRandomIfAny<T>(this Bogus.Randomizer randomizer, ProbabilityCollection<T> probabilities)
        {
            if (probabilities.IsNullOrEmpty())
                return default;
            return randomizer.ProbabilityRandom(probabilities);
        }
    }

    public static class TaskExtensions
    {
        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            // Create a self-cancelling TaskCompletionSource 
            var tcs = new TaskCompletionSourceWithCancellation<T>(cancellationToken);

            // Wait for completion or cancellation
            Task<T> completedTask = await Task.WhenAny(task, tcs.Task);
            return await completedTask;
        }

        public static async Task WithCancellation(this Task task, CancellationToken cancellationToken)
        {
            // Create a self-cancelling TaskCompletionSource 
            var tcs = new TaskCompletionSourceWithCancellation<object>(cancellationToken);

            // Wait for completion or cancellation
            Task completedTask = await Task.WhenAny(task, tcs.Task);
            await completedTask;
        }

        class TaskCompletionSourceWithCancellation<TResult> : TaskCompletionSource<TResult>
        {
            public TaskCompletionSourceWithCancellation(CancellationToken cancellationToken)
            {
                CancellationTokenRegistration registration =
                    cancellationToken.Register(() => TrySetResult(default));

                // Remove the registration after the task completes
                Task.ContinueWith(_ => registration.Dispose());
            }
        }
    }
}
