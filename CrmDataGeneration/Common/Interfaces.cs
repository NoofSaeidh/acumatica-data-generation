using Bogus;
using CrmDataGeneration.OpenApi.Reference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VoidTask = System.Threading.Tasks.Task;

namespace CrmDataGeneration.Common
{
    public interface IRandomizer<T> where T : Entity
    {
        T Generate();
        IEnumerable<T> GenerateList(int count);
    }

    public interface IRandomizerSettings<T> where T : Entity
    {
        int? Seed { get; }
        Faker<T> GetFaker();
    }

    public interface IApiWrappedClient<T> where T : Entity
    {
        Task<T> Create(T entity, CancellationToken cancellationToken = default);
        // skipErrors if true exception will be thrown if any entity will not be processed
        // if false every entity will be processed (or tried to be processed)
        Task<IEnumerable<T>> CreateAllSequentially(IEnumerable<T> entities, bool skipErrors = false, CancellationToken cancellationToken = default);
        // threadsCount = 0 means unlimited
        Task<IEnumerable<T>> CreateAllInParallel(IEnumerable<T> entities, int threadsCount = 0, CancellationToken cancellationToken = default);
        // wrap with logger
        VoidTask WrapAction(VoidTask action);
        VoidTask WrapAction(string actionName, VoidTask action);
        Task<T> WrapAction(Task<T> action);
        Task<T> WrapAction(string actionName, Task<T> action);
    }
}
