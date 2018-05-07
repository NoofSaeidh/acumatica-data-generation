using Bogus;
using CrmDataGeneration.OpenApi.Reference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmDataGeneration.Common
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

    public interface IApiWrappedClient<T> where T : OpenApi.Reference.Entity
    {
        Task<T> Create(T entity);
        Task<IEnumerable<T>> CreateAll(IEnumerable<T> entities);
    }
}
