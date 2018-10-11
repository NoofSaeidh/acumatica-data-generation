using System;
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
    public class ProbabilityObjectCollection<T> where T : IProbabilityObject
    {
        public ProbabilityObjectCollection(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            ProbabilityCollection = new ProbabilityCollection<T>(items.Select(i => new KeyValuePair<T, decimal?>(i, i?.Probability)));
        }

        public ProbabilityCollection<T> ProbabilityCollection { get; }
    }
}
