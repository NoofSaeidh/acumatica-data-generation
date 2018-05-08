using Bogus;
using CrmDataGeneration.OpenApi.Reference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        bool GenerateInParallel { get; }
        int MaxExecutionThreadsParallel { get; } // For parallel
        bool SkipErrorsSequent { get; } // For sequent
    }

    public interface IApiWrappedClient<T> where T : Entity
    {
        Task<T> Create(T entity, CancellationToken cancellationToken = default);
        // skipErrors if true exception will be thrown if any entity will not be processed
        // if false every entity will be processed (or tried to be processed)
        Task<IEnumerable<T>> CreateAllSequentially(IEnumerable<T> entities, bool skipErrors = false, CancellationToken cancellationToken = default);
        // threadsCount = 0 means unlimited
        Task<IEnumerable<T>> CreateAllInParallel(IEnumerable<T> entities, int threadsCount = 0, CancellationToken cancellationToken = default); 
    }
}
