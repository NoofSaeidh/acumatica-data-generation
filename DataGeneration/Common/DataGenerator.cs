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
        public virtual IList<T> GenerateList(int count)
        {
            using (StopwatchLoggerFactory.LogDispose(nameof(DataGenerator<T>),
                "GenerateList completed, Count = {count}", count))
            {
                return Faker.Generate(count);
            }
        }

        public virtual IEnumerable<T> GenerateEnumeration() => Faker.GenerateForever();
    }
}