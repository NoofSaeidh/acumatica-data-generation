using Bogus;
using DataGeneration.Core.Common;
using DataGeneration.Core.DataGeneration;
using DataGeneration.Core.Logging;
using DataGeneration.Soap;
using NLog;
using System;
using System.ComponentModel.DataAnnotations;

namespace DataGeneration.Core.Settings
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
    }

    public abstract class RandomizerSettings<T> : RandomizerSettingsBase, IRandomizerSettings<T>, IValidatable where T : class
    {
        protected static ILogger Logger { get; } = LogHelper.GetLogger(LogHelper.LoggerNames.GenerationRandomizer);

        private IDataGenerator<T> _statefullGenerator;

        public virtual IDataGenerator<T> GetStatelessDataGenerator() => new FakerDataGenerator<T>(GetFaker());

        /// <summary>
        ///     The same as <see cref="GetStatelessDataGenerator"/> but after first call it will persist in memory,
        /// and use the same generator at each call.
        /// This required to generate unique data from different places with the same seed.
        /// This method is preferred.
        /// </summary>
        /// <param name="forceInitialize">Indicate should state full generator be reinitialized.
        /// You should specify true if any property was changed.</param>
        /// <returns></returns>
        public IDataGenerator<T> GetStatefullDataGenerator(bool forceInitialize = false)
        {
            return (forceInitialize || _statefullGenerator == null)
                ? (_statefullGenerator = GetStatelessDataGenerator())
                : _statefullGenerator;
        }

        IDataGenerator<T> IRandomizerSettings<T>.GetDataGenerator() => GetStatefullDataGenerator();

        protected virtual Faker<T> GetFaker()
        {
            Validate();
            return GetFaker<T>();
        }

        protected Faker<TOut> GetFaker<TOut>() where TOut : class
        {
            return new Faker<TOut>().UseSeed(Seed);
        }

        public virtual void Validate() => ValidateHelper.ValidateObject(this);
    }
}