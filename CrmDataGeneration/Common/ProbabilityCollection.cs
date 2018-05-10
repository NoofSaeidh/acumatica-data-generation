using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmDataGeneration.Common
{
    [JsonConverter(typeof(ProbabilityCollectionJsonConverter))]
    [DebuggerTypeProxy(typeof(ProbabilityCollection<>.DebuggerProxyView))]
    public class ProbabilityCollection<T> : Collection<T>, ICollection<T>, IDictionary<T, double>
    {
        public ProbabilityCollection()
        {
            Probabilities = new List<double>();
            FreeProbability = 100;
        }

        public ProbabilityCollection(IList<T> list) : base(list)
        {
            Probabilities = Enumerable.Repeat(-1.0, list.Count).ToList();
            FreeProbability = 100;
        }

        public ProbabilityCollection(IDictionary<T, double> list) : this()
        {
            foreach (var item in list)
            {
                Add(item);
            }
        }
        public double FreeProbability { get; private set; }
        public bool HasDefinedProbabilities => FreeProbability < 100;
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
                if (FreeProbability <= 0) return 0;
                return FreeProbability / Items.Count;
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

            Items.Add(key);
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
                array[i + arrayIndex] = new KeyValuePair<T, double>(Items[i], this[Items[i]]);
            }
        }

        public bool Remove(KeyValuePair<T, double> item)
        {
            return Remove(item.Key);
        }

        public bool TryGetValue(T key, out double value)
        {
            var index = Items.IndexOf(key);
            if (index < 0)
            {
                value = default;
                return false;
            }
            value = Probabilities[index];
            return true;
        }

        protected void ThrowIfProbilityExceeded(double value)
        {
            if (value > FreeProbability)
                throw new InvalidOperationException("Cannot add probability. Total probability cannot exceed 100%.");
        }

        protected void DecreaseFreeProbability(double value)
        {
            if (value <= 0) return;
            FreeProbability -= value;
            if (FreeProbability < 0) FreeProbability = 0;
        }

        protected void IncreaseFreeProbability(double value)
        {
            if (value <= 0) return;
            FreeProbability += value;
            if (FreeProbability > 100) FreeProbability = 100;
        }

        private IEnumerable<KeyValuePair<T, double>> EnumerateProbabilities()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return new KeyValuePair<T, double>(Items[i], this[Items[i]]);
            }
        }

        public new IEnumerator<KeyValuePair<T, double>> GetEnumerator() 
            => EnumerateProbabilities().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected override void ClearItems()
        {
            base.ClearItems();
            Probabilities.Clear();
            FreeProbability = 100;
        }

        protected override void InsertItem(int index, T item)
        {
            if (Items.Contains(item))
                throw new ArgumentException("An item with the same key has already been added.");

            base.InsertItem(index, item);
            Probabilities.Insert(index, -1);
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            var prob = Probabilities[index];
            IncreaseFreeProbability(prob);
            Probabilities.RemoveAt(index);
        }

        protected override void SetItem(int index, T item)
        {
            if (Items.Contains(item))
                throw new ArgumentException("An item with the same key has already been added.");

            base.SetItem(index, item);
        }

        #region Debugger display
        private class DebuggerProxyView
        {
            private readonly ProbabilityCollection<T> _collection;
            public DebuggerProxyView(ProbabilityCollection<T> collection)
            {
                _collection = collection;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public DisplayPair[] Items => ((IDictionary<T, double>)_collection)
                        .Select(p => new DisplayPair(p.Key, p.Value))
                        .ToArray();
        }
        private class DisplayPair
        {  
            public DisplayPair(object key, double probability)
            {
                Key = key;
                Probability = probability;
            }
            public object Key { get; }
            public double Probability { get; }

            public override string ToString() => $"\"{Key}\" ({Probability}%)";
        }
        #endregion

    }
}
