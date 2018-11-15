using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace DataGeneration.Common
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
    public class ProbabilityCollection<T> : IList<T>, IDictionary<T, decimal>, IDictionary<T, decimal?>
    {
        // indexed defined probabilities to calculate Probability per item faster
        private readonly List<int> _definedProbabilitiesIndexes;

        private readonly List<T> _items;
        private readonly List<decimal?> _probabilities;

        public ProbabilityCollection()
        {
            _items = new List<T>();
            _probabilities = new List<decimal?>();
            _definedProbabilitiesIndexes = new List<int>();
            FreeProbability = 1;
        }

        public ProbabilityCollection(IList<T> list)
        {
            _items = list.ToList();
            _probabilities = Enumerable.Repeat<decimal?>(null, list.Count).ToList();
            _definedProbabilitiesIndexes = new List<int>();
            FreeProbability = 1;
        }

        public ProbabilityCollection(IEnumerable<KeyValuePair<T, decimal>> pairs) : this()
        {
            foreach (var item in pairs)
            {
                Add(item);
            }
        }

        public ProbabilityCollection(IEnumerable<KeyValuePair<T, decimal?>> pairs) : this()
        {
            foreach (var item in pairs)
            {
                Add(item);
            }
        }

        protected decimal ProbabilityPerItem
        {
            get
            {
                if (FreeProbability <= 0) return 0;
                var devider = _items.Count - _definedProbabilitiesIndexes.Count;
                if (devider == 0) devider = 1;
                return FreeProbability / devider;
            }
        }

        public IDictionary<T, decimal> AsDictionary => this;

        // because linq doesn't work when both interfaces defined
        public IList<T> AsList => this;

        public decimal FreeProbability { get; private set; }

        public bool HasDefinedProbabilities => FreeProbability < 1;

        public bool IsReadOnly => false;

        public IEnumerable<KeyValuePair<T, decimal>> GetItemsWithProbabilities(bool addDefaultForFreeProbability = true)
        {
            var items = EnumerateProbabilities();
            if (addDefaultForFreeProbability && FreeProbability != 0)
                items = items.Concat(new KeyValuePair<T, decimal>(default, FreeProbability));
            return items;
        }

        public IList<decimal> Probabilities
        {
            get
            {
                var probPerItem = ProbabilityPerItem;
                return _probabilities.Select(x => x == null ? probPerItem : (decimal)x).ToList();
            }
        }

        public IList<decimal?> RawProbabilities => new ReadOnlyCollection<decimal?>(_probabilities);

        public int Count => _items.Count;

        ICollection<T> IDictionary<T, decimal>.Keys => _items.ToList();
        ICollection<T> IDictionary<T, decimal?>.Keys => _items.ToList();
        ICollection<decimal> IDictionary<T, decimal>.Values => Probabilities.ToList();
        ICollection<decimal?> IDictionary<T, decimal?>.Values => RawProbabilities;

        public KeyValuePair<T, decimal> this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return new KeyValuePair<T, decimal>(_items[index], _probabilities[index] ?? FreeProbability);
            }
        }

        // if raw probability is null - will return calculated probability
        public decimal this[T key]
        {
            get
            {
                var index = IndexOf(key);
                if (index < 0)
                {
                    throw new KeyNotFoundException();
                }
                var prob = _probabilities[index];
                if (prob == null)
                {
                    return ProbabilityPerItem;
                }
                return (decimal)prob;
            }
            set
            {
                var index = IndexOf(key);
                if (index < 0)
                {
                    Add(key, value);
                }
                else
                {
                    SetRawProbability(index, value);
                }
            }
        }

        decimal? IDictionary<T, decimal?>.this[T key]
        {
            get => GetRawProbability(key);
            set => SetRawProbability(key, value);
        }

        T IList<T>.this[int index] { get => _items[index]; set => _items[index] = value; }

        // adjust free probability depending on new probability value
        // reduce free probability if new value is bigger that zero
        // enlarge - if less
        private void AdjustFreeProbability(decimal? value)
        {
            if (value == null || value == 0)
                return;

            var newProb = FreeProbability - (decimal)value;
            if (newProb < 0 || newProb > 1)
                throw new InvalidOperationException($"Cannot adjust {nameof(FreeProbability)}. Adjusting value is outside of acceptable range. Resulted {nameof(FreeProbability)} must be in the range of 0 and 1.");

            FreeProbability = newProb;
        }

        private IEnumerable<KeyValuePair<T, decimal>> EnumerateProbabilities()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return new KeyValuePair<T, decimal>(_items[i], _probabilities[i] ?? ProbabilityPerItem);
            }
        }

        private IEnumerable<KeyValuePair<T, decimal?>> EnumerateRawProbabilities()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return new KeyValuePair<T, decimal?>(_items[i], _probabilities[i]);
            }
        }

        public void Add(T key, decimal value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "The value must be non negative.");

            if (Contains(key))
            {
                throw new ArgumentException("An item with the same key has already been added.", nameof(key));
            }

            AdjustFreeProbability(value);
            _items.Add(key);
            _probabilities.Add(value);
            _definedProbabilitiesIndexes.Add(Count - 1);
        }

        public void Add(T key, decimal? value)
        {
            if (value == null)
                Add(key);
            else
                Add(key, (decimal)value);
        }

        public void Add(KeyValuePair<T, decimal> item)
        {
            Add(item.Key, item.Value);
        }

        public void Add(KeyValuePair<T, decimal?> item)
        {
            Add(item.Key, item.Value);
        }

        public void Add(T item)
        {
            if (Contains(item))
            {
                throw new ArgumentException("An item with the same key has already been added.", nameof(item));
            }

            _items.Add(item);
            _probabilities.Add(null);
        }

        public void Clear()
        {
            _items.Clear();
            ClearProbabilities();
        }

        public void ClearProbabilities()
        {
            FreeProbability = 1;
            _definedProbabilitiesIndexes.Clear();
            _probabilities.Clear();
        }

        public bool Contains(T item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<T, decimal>> GetEnumerator()
            => EnumerateProbabilities().GetEnumerator();

        public decimal? GetRawProbability(T key)
        {
            var index = IndexOf(key);
            if (index < 0)
                throw new KeyNotFoundException();
            return GetRawProbability(index);
        }

        public decimal? GetRawProbability(int index)
        {
            return _probabilities[index];
        }

        public int IndexOf(T item)
        {
            return _items.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _items.Insert(index, item);
            _probabilities.Insert(index, null);
        }

        public bool Remove(T item)
        {
            var index = _items.IndexOf(item);
            if (index < 0)
                return false;
            RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
            var prob = _probabilities[index];
            if (prob != null)
            {
                AdjustFreeProbability(-prob);
                _definedProbabilitiesIndexes.Remove(index);
            }
            _probabilities.RemoveAt(index);
        }

        public void SetRawProbability(int index, decimal? value)
        {
            if (index < 0 || index >= _probabilities.Count)
                throw new ArgumentOutOfRangeException(nameof(index), index, "An index was out of range.");

            if (value != null && value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Value cannot be less than zero.");

            var defIndex = _definedProbabilitiesIndexes.IndexOf(index);
            if (defIndex >= 0)
            {
                AdjustFreeProbability(-_probabilities[index]);
                _definedProbabilitiesIndexes.Remove(defIndex);
            }

            if (value == null)
            {
                _probabilities[index] = null;
            }
            else
            {
                AdjustFreeProbability(value);
                _probabilities[index] = value;
                _definedProbabilitiesIndexes.Add(index);
            }
        }

        public void SetRawProbability(T key, decimal? value)
        {
            var index = IndexOf(key);
            if (index < 0)
                throw new KeyNotFoundException();
            SetRawProbability(index, value);
        }

        public override string ToString()
        {
            return $"Count: {Count}, Free Probability: {FreeProbability}";
        }

        public bool TryGetValue(T key, out decimal value)
        {
            var index = _items.IndexOf(key);
            if (index < 0)
            {
                value = default;
                return false;
            }
            value = _probabilities[index] ?? ProbabilityPerItem;
            return true;
        }

        void ICollection<KeyValuePair<T, decimal?>>.Clear() => Clear();

        public bool Contains(KeyValuePair<T, decimal> item) => Contains(item.Key);

        bool IDictionary<T, decimal>.ContainsKey(T key) => Contains(key);

        bool IDictionary<T, decimal?>.ContainsKey(T key) => Contains(key);

        void ICollection<KeyValuePair<T, decimal>>.CopyTo(KeyValuePair<T, decimal>[] array, int arrayIndex)
        {
            for (int i = 0; i < Count; i++)
            {
                array[i + arrayIndex] = new KeyValuePair<T, decimal>(_items[i], _probabilities[i] ?? FreeProbability);
            }
        }

        void ICollection<KeyValuePair<T, decimal?>>.CopyTo(KeyValuePair<T, decimal?>[] array, int arrayIndex)
        {
            for (int i = 0; i < Count; i++)
            {
                array[i + arrayIndex] = new KeyValuePair<T, decimal?>(_items[i], _probabilities[i] ?? FreeProbability);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IList<T>)_items).GetEnumerator();

        IEnumerator<KeyValuePair<T, decimal?>> IEnumerable<KeyValuePair<T, decimal?>>.GetEnumerator() => EnumerateRawProbabilities().GetEnumerator();

        bool ICollection<KeyValuePair<T, decimal>>.Remove(KeyValuePair<T, decimal> item) => Remove(item.Key);

        bool ICollection<KeyValuePair<T, decimal?>>.Remove(KeyValuePair<T, decimal?> item) => Remove(item.Key);

        bool IDictionary<T, decimal?>.TryGetValue(T key, out decimal? value)
        {
            var index = _items.IndexOf(key);
            if (index < 0)
            {
                value = default;
                return false;
            }
            value = _probabilities[index];
            return true;
        }

        bool ICollection<KeyValuePair<T, decimal?>>.Contains(KeyValuePair<T, decimal?> item) => Contains(item.Key);

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