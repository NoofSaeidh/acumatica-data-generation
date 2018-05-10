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
    public class ProbabilityCollection<T> : Collection<T>, IEnumerable<KeyValuePair<T, double>>, IEnumerable<T>, ICollection<T>, IDictionary<T, double>
    {
        private readonly List<int> _definedProbabilitiesIndexes;
        public ProbabilityCollection()
        {
            Probabilities = new List<double>();
            _definedProbabilitiesIndexes = new List<int>();
            FreeProbability = 1;
        }

        public ProbabilityCollection(IList<T> list) : base(list)
        {
            Probabilities = Enumerable.Repeat(-1.0, list.Count).ToList();
            _definedProbabilitiesIndexes = new List<int>();
            FreeProbability = 1;
        }

        public ProbabilityCollection(IDictionary<T, double> list) : this()
        {
            foreach (var item in list)
            {
                Add(item);
            }
        }
        public double FreeProbability { get; private set; }
        public bool HasDefinedProbabilities => FreeProbability < 1;
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
                ThrowIfProbilityWillExceed(value);
                var index = IndexOf(key);
                if (index < 0)
                {
                    Add(key);
                    if (value < 0)
                        Probabilities.Add(-1);
                    else
                    {
                        Probabilities.Add(value);
                        _definedProbabilitiesIndexes.Add(Items.Count - 1);
                    }
                }
                else
                {
                    Probabilities[index] = value;
                }
                DecreaseFreeProbability(value);
            }
        }

        public new KeyValuePair<T, double> this[int index]
        {
            get
            {
                var item = base[index];
                return new KeyValuePair<T, double>(item, this[item]);
            }
        }

        ICollection<T> IDictionary<T, double>.Keys => AsList.ToList();

        ICollection<double> IDictionary<T, double>.Values => RawProbabilities.ToList();

        public IEnumerable<double> RawProbabilities => new ReadOnlyCollection<double>(Probabilities);

        public IEnumerable<double> CalculatedProbabilities
        {
            get
            {
                var probPerItem = ProbabilityPerItem;
                return Probabilities.Select(x => x >= 0 ? x : probPerItem).ToList();
            }
        }

        public bool IsReadOnly => false;

        // because linq doesn't when both interfaces defined
        public IList<T> AsList => this;
        public IDictionary<T, double> AsDictionary => this;

        protected double ProbabilityPerItem
        {
            get
            {
                if (FreeProbability <= 0) return 0;
                return FreeProbability / (Items.Count - _definedProbabilitiesIndexes.Count);
            }
        }

        public void Add(T key, double value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var index = IndexOf(key);
            if (index >= 0)
            {
                throw new ArgumentException("An item with the same key has already been added.", nameof(key));
            }

            Items.Add(key);
            AddProbability(value);
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
            return base.Contains(key);
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
            return base.Remove(item.Key);
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
            ClearProbabilities();
        }

        protected override void InsertItem(int index, T item)
        {
            if (Items.Contains(item))
                throw new ArgumentException("An item with the same key has already been added.");
            base.InsertItem(index, item);
            InsertProbability(index, -1);
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            RemoveProbability(index);
        }

        protected override void SetItem(int index, T item)
        {
            if (Items.Contains(item))
                throw new ArgumentException("An item with the same key has already been added.");

            base.SetItem(index, item);
        }

        protected void ThrowIfProbilityWillExceed(double toAddValue)
        {
            if (toAddValue > FreeProbability)
                throw new InvalidOperationException("Cannot add probability. Total probability cannot exceed 1.");
        }

        protected void DecreaseFreeProbability(double value)
        {
            if (value <= 0)
                return;
            ThrowIfProbilityWillExceed(value);
            FreeProbability -= value;
        }

        protected void IncreaseFreeProbability(double value)
        {
            if (value <= 0)
                return;
            FreeProbability += value;
            if (FreeProbability > 1)
                FreeProbability = 1;
        }

        protected void ClearProbabilities()
        {
            FreeProbability = 1;
            _definedProbabilitiesIndexes.Clear();
            Probabilities.Clear();
        }

        protected void AddProbability(double value)
        {
            if (value < 0)
            {
                Probabilities.Add(-1);
            }
            else
            {
                DecreaseFreeProbability(value);
                Probabilities.Add(value);
                _definedProbabilitiesIndexes.Add(Probabilities.Count - 1);
            }
        }

        protected void InsertProbability(int index, double value)
        {
            if (index == Probabilities.Count)
            {
                AddProbability(value);
                return;
            }

            if (index < 0 || index >= Probabilities.Count)
                throw new ArgumentOutOfRangeException(nameof(index), index, "An index was out of range.");

            for (int i = 0; i < _definedProbabilitiesIndexes.Count; i++)
            {
                if (_definedProbabilitiesIndexes[i] >= index)
                    _definedProbabilitiesIndexes[i]++;
            }

            if (value < 0)
            {
                IncreaseFreeProbability(Probabilities[index]);
                Probabilities.Insert(index, value);
            }
            else
            {
                DecreaseFreeProbability(value - Probabilities[index]);
                Probabilities.Insert(index, value);
                _definedProbabilitiesIndexes.Add(Probabilities.Count - 1);
            }
        }

        protected void SetProbability(int index, double value)
        {
            if (index < 0 || index >= Probabilities.Count)
                throw new ArgumentOutOfRangeException(nameof(index), index, "An index was out of range.");

            var defIndex = _definedProbabilitiesIndexes.IndexOf(index);
            if (defIndex >= 0)
                _definedProbabilitiesIndexes.RemoveAt(defIndex);


            if (value < 0)
            {
                IncreaseFreeProbability(Probabilities[index]);
                Probabilities[index] = -1;
            }
            else
            {
                DecreaseFreeProbability(value - Probabilities[index]);
                Probabilities[index] = value;
                _definedProbabilitiesIndexes.Add(Probabilities.Count - 1);
            }
        }

        protected void RemoveProbability(int index)
        {
            if (index < 0 || index >= Probabilities.Count)
                throw new ArgumentOutOfRangeException(nameof(index), index, "An index was out of range.");

            var defIndex = _definedProbabilitiesIndexes.IndexOf(index);
            if (defIndex >= 0)
                _definedProbabilitiesIndexes.RemoveAt(defIndex);

            IncreaseFreeProbability(Probabilities[index]);
            Probabilities.RemoveAt(index);
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

            public override string ToString() => $"\"{Key}\" ({Probability * 100}%)";
        }
        #endregion

    }
}
