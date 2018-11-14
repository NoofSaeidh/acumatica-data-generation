using Bogus;
using System;
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


    public class SourceCollectionDataGenerator<T> : IDataGenerator<T>
    {
        private T[] _array;
        public SourceCollectionDataGenerator(IEnumerable<T> items)
        {
            _array = items?.ToArray() ?? throw new ArgumentNullException(nameof(items));
        }

        public IList<T> GenerateList(int count)
        {
            if (_array.Length < count)
                throw new InvalidOperationException(
                    $"Cannot return so much entities. " +
                    $"Requested count: {count}, available count: {_array.Length}.");
            return _array;
        }

        public T Generate() => throw new NotSupportedException();
        public IEnumerable<T> GenerateEnumeration() => _array;
    }
}