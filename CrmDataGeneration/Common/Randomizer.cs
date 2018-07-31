using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmDataGeneration.Common
{
    public class Randomizer<T> : IRandomizer<T> where T : Soap.Entity
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
        public virtual IList<T> GenerateList(int count)
        {
            return Settings.GetFaker().Generate(count);
        }
    }
}
