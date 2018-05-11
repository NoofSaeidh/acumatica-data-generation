using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace CrmDataGeneration.Common
{
    /// <summary>
    ///     Collection taht provided to define items and theirs probabilities.
    /// It wraps common <see cref="IList(T)"/> and <see cref="IDictionary(T, decimal)"/> at ones.
    /// If you add items with negative probabilities (or without probabilities)
    /// will be used default probability that depends on <see cref="FreeProbability"/>.
    /// If you want use this collection as common list call <see cref="AsList"/>,
    /// this will return collection itself but you could use LINQ.
    /// The same for dictionary can be used by <see cref="AsDictionary"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [JsonConverter(typeof(ProbabilityCollectionJsonConverter))]
    [DebuggerTypeProxy(typeof(ProbabilityCollection<>.DebuggerProxyView))]
    [DebuggerDisplay("{ToString()}")]
    public class ProbabilityCollection<T> : Collection<T>, IEnumerable<KeyValuePair<T, decimal>>, IEnumerable<T>, ICollection<T>, IDictionary<T, decimal>
    {
        // indexed defined probabilities to calculate Probability per item faster
        private readonly List<int> _definedProbabilitiesIndexes;
        public ProbabilityCollection()
        {
            Probabilities = new List<decimal>();
            _definedProbabilitiesIndexes = new List<int>();
            FreeProbability = 1;
        }

        public ProbabilityCollection(IList<T> list) : base(list)
        {
            Probabilities = Enumerable.Repeat(-1.0m, list.Count).ToList();
            _definedProbabilitiesIndexes = new List<int>();
            FreeProbability = 1;
        }

        public ProbabilityCollection(IDictionary<T, decimal> list) : this()
        {
            foreach (var item in list)
            {
                Add(item);
            }
        }

        public decimal FreeProbability { get; private set; }

        public bool HasDefinedProbabilities => FreeProbability < 1;

        // if probability == -1 - it means need to take free probability
        protected IList<decimal> Probabilities { get; }

        // if set <0 will be used default probability per item
        public decimal this[T key]
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

        public new KeyValuePair<T, decimal> this[int index]
        {
            get
            {
                var item = base[index];
                return new KeyValuePair<T, decimal>(item, this[item]);
            }
        }

        ICollection<T> IDictionary<T, decimal>.Keys => AsList.ToList();

        ICollection<decimal> IDictionary<T, decimal>.Values => RawProbabilities.ToList();

        public IEnumerable<decimal> RawProbabilities => new ReadOnlyCollection<decimal>(Probabilities);

        public IEnumerable<decimal> CalculatedProbabilities
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

        public IDictionary<T, decimal> AsDictionary => this;

        protected decimal ProbabilityPerItem
        {
            get
            {
                if (FreeProbability <= 0) return 0;
                var devider = Items.Count - _definedProbabilitiesIndexes.Count;
                if (devider == 0) devider = 1;
                return FreeProbability / devider;
            }
        }

        public void Add(T key, decimal value)
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

        public void Add(KeyValuePair<T, decimal> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<T, decimal> item)
        {
            return ContainsKey(item.Key);
        }

        public bool ContainsKey(T key)
        {
            return base.Contains(key);
        }

        public void CopyTo(KeyValuePair<T, decimal>[] array, int arrayIndex)
        {
            for (int i = 0; i < Count; i++)
            {
                array[i + arrayIndex] = new KeyValuePair<T, decimal>(Items[i], this[Items[i]]);
            }
        }

        public bool Remove(KeyValuePair<T, decimal> item)
        {
            return base.Remove(item.Key);
        }

        public bool TryGetValue(T key, out decimal value)
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

        public override string ToString()
        {
            return $"Count: {Count}, Free Probability: {FreeProbability}";
        }

        private IEnumerable<KeyValuePair<T, decimal>> EnumerateProbabilities()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return new KeyValuePair<T, decimal>(Items[i], this[Items[i]]);
            }
        }

        public new IEnumerator<KeyValuePair<T, decimal>> GetEnumerator()
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

        protected void ThrowIfProbilityWillExceed(decimal toAddValue)
        {
            if (toAddValue > FreeProbability)
                throw new InvalidOperationException("Cannot add probability. Total probability cannot exceed 1.");
        }

        protected void DecreaseFreeProbability(decimal value)
        {
            if (value <= 0)
                return;
            ThrowIfProbilityWillExceed(value);
            FreeProbability -= value;
        }

        protected void IncreaseFreeProbability(decimal value)
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

        protected void AddProbability(decimal value)
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

        protected void InsertProbability(int index, decimal value)
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

        protected void SetProbability(int index, decimal value)
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
            public DisplayPair[] Items => ((IDictionary<T, decimal>)_collection)
                        .Select(p => new DisplayPair(p.Key, p.Value))
                        .ToArray();
        }

        private class DisplayPair
        {
            public DisplayPair(object key, decimal probability)
            {
                Key = key;
                Probability = probability;
            }

            public object Key { get; }
            public decimal Probability { get; }

            public override string ToString() => $"\"{Key}\", {Probability * 100}%";
        }
        #endregion

    }
}