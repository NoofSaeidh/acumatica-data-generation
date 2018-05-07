using CrmDataGeneration.OpenApi.Reference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmDataGeneration.Randomize
{
    public class Randomizer<T> : IRandomizer<T> where T : Entity
    {
        public Randomizer(IRandomizerSettings<T> settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        protected IRandomizerSettings<T> Settings { get; }

        public virtual T Generate()
        {
            return Settings.GetFaker().Generate();
        }
        public virtual IEnumerable<T> GenerateList()
        {
            return Settings.GetFaker().Generate(Settings.Count);
        }
    }
}
