using CrmDataGeneration.Common;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmDataGeneration.Core
{
    public interface IGeneratorApiWrappedClient<T> where T : OpenApi.Reference.Entity
    {
        Task<T> Create(T entity);
        Task<IEnumerable<T>> CreateAll(IEnumerable<T> entities);
    }
    public abstract class GeneratorApiWrappedClient<T> : IGeneratorApiWrappedClient<T> where T : OpenApi.Reference.Entity
    {
        private static readonly ILogger _logger = LogSettings.DefaultLogger;
        public async Task<T> Create(T entity)
        {
            try
            {
                var res = await CreateRaw(entity);
                _logger.Info("{entityName} was created. Result: {@entity}", typeof(T).Name, res);
                return res;
            }
            catch (Exception e)
            {
                _logger.Error(e, "{entityName} wasn't created. {@entity}", typeof(T).Name, entity);
                throw;
            }
        }

        public async Task<IEnumerable<T>> CreateAll(IEnumerable<T> entities)
        {
            try
            {
                var res = await CreateAllRaw(entities);
                _logger.Info("Collection of {entityName} was created. Result: {entities}", typeof(T).Name, res);
                return res;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Collection of {entityName} wasn't created.", typeof(T).Name, entities);
                throw;
            }
        }

        protected abstract Task<T> CreateRaw(T entity); //without logging and exceptions
        protected abstract Task<IEnumerable<T>> CreateAllRaw(IEnumerable<T> entity); //without logging and exceptions
    }
}
