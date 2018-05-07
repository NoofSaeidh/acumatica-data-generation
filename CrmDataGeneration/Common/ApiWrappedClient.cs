using CrmDataGeneration.OpenApi;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmDataGeneration.Common
{
    public abstract class ApiWrappedClient<T> : IApiWrappedClient<T> where T : OpenApi.Reference.Entity
    {
        private static ILogger _logger => LogConfiguration.DefaultLogger;

        protected ApiWrappedClient(OpenApiState openApiState)
        {
            OpenApiState = openApiState ?? throw new ArgumentNullException(nameof(openApiState));
        }
        protected OpenApiState OpenApiState { get; }

        public async Task<T> Create(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            try
            {
                var sw = new Stopwatch();
                sw.Start();
                var res = await CreateRaw(entity);
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

        public async Task<IEnumerable<T>> CreateAllSequentially(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            try
            {
                _logger.Info("Start creating {entityName} collection sequentially.", typeof(T).Name);
                var input = entities.ToList();
                var output = new List<T>(input.Count);
                var sw = new Stopwatch();
                sw.Start();
                foreach (var item in input)
                {
                    try
                    {
                        output.Add(await CreateRaw(item));
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "{entityName} wasn't created. {@entity}", typeof(T).Name, item);
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

        public async Task<IEnumerable<T>> CreateAllParallel(IEnumerable<T> entities, int threadsCount = 0)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

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
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            var result = await CreateRaw(item);
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

        protected abstract Task<T> CreateRaw(T entity); //without logging and exceptions
    }
}
