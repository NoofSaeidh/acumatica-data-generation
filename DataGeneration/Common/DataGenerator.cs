using Bogus;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DataGeneration.Common
{
    public class FakerDataGenerator<T> : IDataGenerator<T> where T : class
    {
        public FakerDataGenerator(Faker<T> faker)
        {
            Faker = faker ?? throw new ArgumentNullException(nameof(faker));
        }

        public Faker<T> Faker { get; }

        public virtual T Generate() => Faker.Generate();
        public virtual IList<T> GenerateList(int count)
        {
            using (StopwatchLoggerFactory.LogDispose(nameof(FakerDataGenerator<T>),
                "GenerateList completed, Count = {count}", count))
            {
                return Faker.Generate(count);
            }
        }

        public virtual IEnumerable<T> GenerateEnumeration() => Faker.GenerateForever();
    }


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