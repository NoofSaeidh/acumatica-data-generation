using Bogus;
using System;
using System.Collections.Generic;

namespace DataGeneration.Common
{
    public class DataGenerator<T> : IDataGenerator<T> where T : class
    {
        public DataGenerator(Faker<T> faker)
        {
            Faker = faker ?? throw new ArgumentNullException(nameof(faker));
        }

        public Faker<T> Faker { get; }

        public virtual T Generate() => Faker.Generate();
        public virtual IList<T> GenerateList(int count) => Faker.Generate(count);
        public virtual IEnumerable<T> GenerateEnumeration() => Faker.GenerateForever();
    }
}