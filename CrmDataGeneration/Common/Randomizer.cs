﻿using CrmDataGeneration.OpenApi.Reference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmDataGeneration.Common
{
    public class Randomizer<T> : IRandomizer<T> where T : Entity
    {
        public Randomizer(RandomizerSettings<T> settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        protected RandomizerSettings<T> Settings { get; }

        public virtual T Generate()
        {
            return Settings.GetFaker().Generate();
        }
        public virtual IEnumerable<T> GenerateList(int count)
        {
            return Settings.GetFaker().Generate(count);
        }
    }
}
