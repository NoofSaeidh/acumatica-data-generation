using Bogus;
using CrmDataGeneration.Soap;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmDataGeneration.Common
{
    public abstract class BaseRandomizerSettings
    {
        private int? _seed;
        // if not initialized manually - use random seed.
        public int Seed
        {
            get => _seed ?? (int)(_seed = new Random().Next());
            set => _seed = value;
        }
    }
    public abstract class RandomizerSettings<T> : BaseRandomizerSettings, IRandomizerSettings<T> where T : Entity
    {
        public IRandomizer<T> Randomizer => new Randomizer<T>(this);

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
