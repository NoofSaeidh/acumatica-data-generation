using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmDataGeneration.Common
{
    public class ProbabilityCollection<T> : Collection<T>, ICollection<T>, IDictionary<T, double>
    {
        private double _freeProbability;

        public ProbabilityCollection()
        {
            Probabilities = new List<double>();
        }

        public ProbabilityCollection(IList<T> list) : base(list)
        {
            Probabilities = Enumerable.Repeat(-1.0, list.Count).ToList();
        }

        public ProbabilityCollection(IDictionary<T, double> list)
        {
            Probabilities = new List<double>();
            foreach (var item in list)
            {
                Add(item);
            }
        }

        // if probability == -1 - it means need to take free probability
        protected IList<double> Probabilities { get; }

        // if set <0 will be used default probability per item
        public double this[T key]
        {
            get
            {
                var index = IndexOf(key);
                if (index < 0)
                {
                    throw new KeyNotFoundException();
                }
                var prob = Probabilities[index];
                if (prob < 0)
                {
                    return ProbabilityPerItem;
                }
                return prob;
            }
            set
            {
                ThrowIfProbilityExceeded(value);
                var index = IndexOf(key);
                if (index < 0)
                {
                    Add(key);
                    Probabilities.Add(value);
                }
                else
                {
                    Probabilities[index] = value;
                }
                DecreaseFreeProbability(value);
            }
        }

        public ICollection<T> Keys => new ReadOnlyCollection<T>(Items);

        public ICollection<double> Values => new ReadOnlyCollection<double>(Probabilities);

        public bool IsReadOnly => false;

        protected double ProbabilityPerItem
        {
            get
            {
                if (_freeProbability <= 0) return 0;
                return _freeProbability / Items.Count;
            }
        }

        public void Add(T key, double value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            ThrowIfProbilityExceeded(value);
            var index = IndexOf(key);
            if (index >= 0)
            {
                throw new ArgumentException("An item with the same key has already been added.", nameof(key));
            }

            Add(key);
            Probabilities.Add(value);
            DecreaseFreeProbability(value);
        }

        public void Add(KeyValuePair<T, double> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<T, double> item)
        {
            return ContainsKey(item.Key);
        }

        public bool ContainsKey(T key)
        {
            return Contains(key);
        }

        public void CopyTo(KeyValuePair<T, double>[] array, int arrayIndex)
        {
            for (int i = 0; i < Count; i++)
            {
                array[i + arrayIndex] = new KeyValuePair<T, double>(Items[i], Probabilities[i]);
            }
        }

        public bool Remove(KeyValuePair<T, double> item)
        {
            return Remove(item.Key);
        }

        public bool TryGetValue(T key, out double value)
        {
            var index = Items.IndexOf(key);
            if(index < 0)
            {
                value = default;
                return false;
            }
            value = Probabilities[index];
            return true;
        }

        protected void ThrowIfProbilityExceeded(double value)
        {
            if (value > _freeProbability)
                throw new InvalidOperationException("Cannot add probability. Total probability cannot exceed 100%.");
        }

        private void DecreaseFreeProbability(double value)
        {
            if (value <= 0) return;
            _freeProbability -= value;
        }

        private IEnumerable<KeyValuePair<T, double>> Enumerate()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return new KeyValuePair<T, double>(Items[i], Probabilities[i]);
            }
        }

        IEnumerator<KeyValuePair<T, double>> IEnumerable<KeyValuePair<T, double>>.GetEnumerator() => Enumerate().GetEnumerator();
    }
}
