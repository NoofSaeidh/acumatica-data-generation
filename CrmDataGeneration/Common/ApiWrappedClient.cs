﻿using CrmDataGeneration.Common;
using CrmDataGeneration.OpenApi;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmDataGeneration.Core
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
                var res = await CreateRaw(entity);
                _logger.Info("{entityName} was created. Result: {entity}", typeof(T).Name, res);
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
                var input = entities.ToList();
                var output = new List<T>(input.Count);
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
                _logger.Info("Collection of {entityName} was created. Result: {entities}", typeof(T).Name, output);
                return output;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Collection of {entityName} wasn't created.", typeof(T).Name, entities);
                throw;
            }
        }

        public async Task<IEnumerable<T>> CreateAllParallel(IEnumerable<T> entities, int threadsCount = 0)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            try
            {
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
                _logger.Info("Collection of {entityName} was created. Result: {entities}", typeof(T).Name, output);
                return output;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Collection of {entityName} wasn't created.", typeof(T).Name, entities);
                throw;
            }
        }

        protected abstract Task<T> CreateRaw(T entity); //without logging and exceptions
    }
}
