using Bogus;
using DataGeneration.Core.Logging;
using System;
using System.Collections.Generic;
using LogLevel = NLog.LogLevel;

namespace DataGeneration.Core.DataGeneration
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
            using (StopwatchLoggerFactory.LogDispose(
                LogHelper.DefaultLogger,
                LogLevel.Trace,
                nameof(FakerDataGenerator<T>),
                "GenerateList completed, Count = {count}",
                count))
            {
                return Faker.Generate(count);
            }
        }

        public virtual IEnumerable<T> GenerateEnumeration() => Faker.GenerateForever();
    }
}