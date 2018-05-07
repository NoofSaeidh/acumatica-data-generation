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

    public interface IGenerationSettings<T> where T : Entity
    {
        bool GenerateInParallel { get; set; }
        int MaxExecutionThreads { get; set; }
    }

    public interface IApiWrappedClient<T> where T : Entity
    {
        Task<T> Create(T entity);
        Task<IEnumerable<T>> CreateAllSequentially(IEnumerable<T> entities);
        // threadsCount = 0 means unlimited
        Task<IEnumerable<T>> CreateAllParallel(IEnumerable<T> entities, int threadsCount = 0); 
    }
}
