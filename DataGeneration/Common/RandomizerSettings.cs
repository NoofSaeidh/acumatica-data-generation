using Bogus;
using DataGeneration.Soap;
using System;

namespace DataGeneration.Common
{
    public abstract class RandomizerSettingsBase
    {
        private int? _seed;

        // if not initialized manually - use random seed.
        public int Seed
        {
            get => _seed ?? (int)(_seed = new Random().Next());
            set => _seed = value;
        }

        public Randomizer GetRandomizer() => GetRandomizer(Seed);
        public static Randomizer GetRandomizer(int seed) => new Randomizer(seed);
        public static Randomizer GetRandomizer<T>(IRandomizerSettings<T> randomizer) where T : Entity  
            => GetRandomizer(randomizer?.Seed ?? throw new ArgumentNullException(nameof(randomizer)));

        public static Faker<T> GetFaker<T>(IRandomizerSettings<T> randomizer) where T : Entity
        {
            if (randomizer == null)
                throw new ArgumentNullException(nameof(randomizer));
            if(randomizer.GetDataGenerator() is DataGenerator<T> dg)
            {
                return dg.Faker;
            }
            throw new NotSupportedException($"Randomizer should return {typeof(DataGenerator<T>)} for {nameof(randomizer.GetDataGenerator)} method in order to get faker.");
        }
    }

    public abstract class RandomizerSettings<T> : RandomizerSettingsBase, IRandomizerSettings<T> where T : Entity
    {
        private IDataGenerator<T> _statefullGenerator;

        public IDataGenerator<T> GetStatelessDataGenerator() => new DataGenerator<T>(GetFaker());

        /// <summary>
        ///     The same as <see cref="GetStatelessDataGenerator"/> but after first call it will persist in memory,
        /// and use the same generator at each call.
        /// This required to generate unique data from diferent places with the same seed.
        /// This method is prefered.
        /// </summary>
        /// <param name="forceInitialize">Indicate should statefull generator be reinitialized.
        /// You should specify true if any property was changed.</param>
        /// <returns></returns>
        public IDataGenerator<T> GetStatefullDataGenerator(bool forceInitialize = false)
        {
            return (forceInitialize || _statefullGenerator == null)
                ? (_statefullGenerator = GetStatelessDataGenerator())
                : _statefullGenerator;
        }

        IDataGenerator<T> IRandomizerSettings<T>.GetDataGenerator() => GetStatefullDataGenerator();

        public virtual Faker<T> GetFaker()
        {
            ValidateHelper.ValidateObject(this);
            var faker = new Faker<T>();
            faker.UseSeed(Seed);
            return faker;
        }

        public void Validate() => ValidateHelper.ValidateObject(this);
    }
}