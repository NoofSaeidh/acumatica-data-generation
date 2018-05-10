using Bogus;
using CrmDataGeneration.OpenApi.Reference;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmDataGeneration.Common
{
    public abstract class RandomizerSettings<T> : IRandomizerSettings<T> where T : Entity
    {
        public virtual Faker<T> GetFaker()
        {
            ThrowIfSettingsNotSpecified();
            var faker = new Faker<T>();
            if (Seed != null)
                faker.UseSeed((int)Seed);
            return faker;
        }
        [JsonIgnore]
        public virtual bool RequiredSettingsSpecified => true;
        public int? Seed { get; set; }

        protected virtual void ThrowIfSettingsNotSpecified()
        {
            if (!RequiredSettingsSpecified)
                throw new InvalidOperationException("Some required settings are not specified.");
        }

        protected bool IsAllCollectionsDefined(params IEnumerable[] enumerations)
        {
            if (enumerations == null) return true;
            foreach (var enumer in enumerations)
            {
                if (enumer.IsNullOrEmpty())
                    return false;
            }
            return true;
        }

        protected bool IsAllObjectsDefined(params object[] objects)
        {
            if (objects == null) return true;
            if (objects.Any(o => o == null)) return false;
            return true;
        }
    }
}
