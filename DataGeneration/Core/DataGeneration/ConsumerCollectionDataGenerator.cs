using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DataGeneration.Core.DataGeneration
{
    public class ConsumerCollectionDataGenerator<T> : IDataGenerator<T>
    {
        private IProducerConsumerCollection<T> _items;
        public ConsumerCollectionDataGenerator(IProducerConsumerCollection<T> items)
        {
            _items = items ?? throw new ArgumentNullException(nameof(items));
        }

        public IList<T> GenerateList(int count) => GenerateEnumeration().Take(count).ToArray();
        public T Generate() => GenerateEnumeration().First();
        public IEnumerable<T> GenerateEnumeration()
        {
            while(_items.TryTake(out var item))
            {
                yield return item;
            }
            throw new GenerationException("Cannot get item. No items remain.");
        }
    }
}