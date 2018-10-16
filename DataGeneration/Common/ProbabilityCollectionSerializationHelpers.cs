using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.Common
{
    /// <summary>
    ///     Wrapper for probability collection of <see cref="IProbabilityObject"/>.
    /// Provided for right parsing complex objects with probabilities.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ProbabilityObjectCollection<T>
    {
        public ProbabilityObjectCollection(IEnumerable items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            if (typeof(IProbabilityObject).IsAssignableFrom(typeof(T)))
            {
                ProbabilityCollection = new ProbabilityCollection<T>(
                    items
                    .Cast<IProbabilityObject>()
                    .Select(i => new KeyValuePair<T, decimal?>((T)i, i?.Probability))
                );
            }
            else if(ValueTupleReflectionHelper.IsValueTupleOrNullableType(typeof(T)))
            {
                ProbabilityCollection = new ProbabilityCollection<T>(
                    items
                    .Cast<ValueTupleProbabilityWrapper<T>>()
                    .Select(i => new KeyValuePair<T, decimal?>(i.Value, i?.Probability))
                );
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public ProbabilityCollection<T> ProbabilityCollection { get; }
    }

    internal class ValueTupleProbabilityWrapper<T> : IProbabilityObject
    {
        public decimal? Probability { get; set; }
        public T Value { get; set; }
    }
}
