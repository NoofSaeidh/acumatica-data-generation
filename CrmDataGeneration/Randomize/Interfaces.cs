using Bogus;
using CrmDataGeneration.OpenApi.Reference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmDataGeneration.Randomize
{
    public interface IRandomizer<T> where T : Entity
    {
        T Generate();
        IEnumerable<T> GenerateList();
    }

    public interface IRandomizerSettings<T> where T : Entity
    {
        int Count { get; }
        Faker<T> GetFaker();
    }
}
