using CrmDataGeneration.OpenApi;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrmDataGeneration.Common
{
    public abstract class ApiWrappedClient<T> : IApiWrappedClient<T> where T : OpenApi.Reference.Entity
    {
        private const string _wrappedAction = "Wrapped action";
        private static ILogger _logger => LogConfiguration.DefaultLogger;

        protected ApiWrappedClient(OpenApiState openApiState)
        {
            OpenApiState = openApiState ?? throw new ArgumentNullException(nameof(openApiState));
        }
        protected OpenApiState OpenApiState { get; }

        public async Task<T> Create(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            try
            {
                var sw = new Stopwatch();
                sw.Start();
                var res = await CreateRaw(entity, cancellationToken);
                sw.Stop();
                _logger.Info("{entityName} was created. Time elapsed: {time} Result: {entity}", typeof(T).Name, sw.Elapsed, res);
                return res;
            }
            catch (Exception e)
            {
                _logger.Error(e, "{entityName} wasn't created. {@entity}", typeof(T).Name, entity);
                throw;
            }
        }

        public async Task<IEnumerable<T>> CreateAllSequentially(IEnumerable<T> entities, bool skipErrors = false, CancellationToken cancellationToken = default)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            if (!entities.Any())
            {
                _logger.Warn("Creation of empty collection {entityName} was requested.", typeof(T).Name);
                return entities;
            }

            try
            {
                _logger.Info("Start creating {entityName} collection sequentially.", typeof(T).Name);
                var input = entities.ToList();
                var output = new List<T>(input.Count);
                var sw = new Stopwatch();
                sw.Start();
                foreach (var item in input)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        output.Add(await CreateRaw(item, cancellationToken));
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "{entityName} wasn't created. {@entity}", typeof(T).Name, item);
                        if (!skipErrors)
                            throw;
                    }
                }
                sw.Stop();
                _logger.Info("Collection of {entityName} was created sequentially. Time elapsed {time}. Result: {entities}", typeof(T).Name, sw.Elapsed, output);
                return output;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Collection of {entityName} wasn't created sequentially.", typeof(T).Name, entities);
                throw;
            }
        }

        public async Task<IEnumerable<T>> CreateAllInParallel(IEnumerable<T> entities,
            int threadsCount = 0,
            CancellationToken cancellationToken = default)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            if(!entities.Any())
            {
                _logger.Warn("Creation of empty collection {entityName} was requested.", typeof(T).Name);
                return entities;
            }

            try
            {
                _logger.Info("Start creating {entityName} collection parallel.", typeof(T).Name);
                var input = entities.ToList();
                var output = new List<T>(input.Count);
                List<Task<T>> tasks;
                if (threadsCount == 0)
                {
                    tasks = new List<Task<T>>(input.Count);
                }
                else
                {
                    tasks = new List<Task<T>>(threadsCount);
                }
                var sw = new Stopwatch();
                sw.Start();
                foreach (var item in input)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            var result = await CreateRaw(item, cancellationToken);
                            output.Add(result);
                            return result;
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e, "{entityName} wasn't created. {@entity}", typeof(T).Name, item);
                            throw;
                        }
                    }));
                    if (tasks.Count == tasks.Capacity)
                    {
                        // await for special count
                        await Task.WhenAll(tasks);
                        tasks.Clear();
                    }
                }
                // if last count of task a fewer that capacity (threads count)
                if (tasks.Any())
                    await Task.WhenAll(tasks);
                sw.Stop();
                _logger.Info("Collection of {entityName} was created in parallel. Time elapsed: {time}. Result: {entities}", typeof(T).Name, sw.Elapsed, output);
                return output;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Collection of {entityName} wasn't created in parallel.", typeof(T).Name, entities);
                throw;
            }
        }

        protected abstract Task<T> CreateRaw(T entity, CancellationToken cancellationToken = default); //without logging and exceptions

        public async Task WrapAction(Task action)
        {
            await WrapAction(_wrappedAction, action);
        }
        public async Task WrapAction(string actionName, Task action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (actionName == null)
                actionName = _wrappedAction;

            try
            {
                var sw = new Stopwatch();
                sw.Start();
                await action;
                sw.Stop();
                _logger.Info("{action} performed. Entity: {entity}. Time elapsed: {time}.", actionName, typeof(T).Name, sw.Elapsed);
            }
            catch (Exception e)
            {
                _logger.Error(e, "{action} wasn't performed. Entity {entity}.", actionName, typeof(T).Name);
                throw;
            }
        }

        public async Task<T> WrapAction(Task<T> action)
        {
            return await WrapAction(_wrappedAction, action);
        }
        public async Task<T> WrapAction(string actionName, Task<T> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (actionName == null)
                actionName = _wrappedAction;

            try
            {
                var sw = new Stopwatch();
                sw.Start();
                var res = await action;
                sw.Stop();
                _logger.Info("{action} performed. Entity: {entity}. Time elapsed: {time}. Result: {result}", actionName, typeof(T).Name, sw.Elapsed, res);
                return res;
            }
            catch (Exception e)
            {
                _logger.Error(e, "{action} wasn't performed. Entity {entity}.", actionName, typeof(T).Name);
                throw;
            }
        }

        public async Task<IEnumerable<T>> WrapAction(Task<IEnumerable<T>> action)
        {
            return await WrapAction(_wrappedAction, action);
        }
        public async Task<IEnumerable<T>> WrapAction(string actionName, Task<IEnumerable<T>> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (actionName == null)
                actionName = _wrappedAction;

            try
            {
                var sw = new Stopwatch();
                sw.Start();
                var res = await action;
                sw.Stop();
                _logger.Info("{action} performed. Entity: {entity}. Time elapsed: {time}. Result {result}", actionName, typeof(T).Name, sw.Elapsed, res);
                return res;
            }
            catch (Exception e)
            {
                _logger.Error(e, "{action} wasn't performed. Entity {entity}.", actionName, typeof(T).Name);
                throw;
            }
        }
    }
}
