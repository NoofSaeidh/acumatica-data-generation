using CrmDataGeneration.Common;
using CrmDataGeneration.Generation.Leads;
using CrmDataGeneration.OpenApi;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrmDataGeneration
{
    public class GeneratorClient : IDisposable
    {
        private readonly OpenApiState _openApiState;
        private readonly OpenApiBaseClient _loginClient;
        private static ILogger _logger => LogConfiguration.DefaultLogger;

        public GeneratorClient(GeneratorConfig config)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            if (Config.OpenApiSettings == null) throw new ArgumentException($"Property {nameof(Config.OpenApiSettings)}" +
                 $" of argument {nameof(config)} must not be null.");

            _openApiState = new OpenApiState(Config.OpenApiSettings);
            _loginClient = new OpenApiBaseClient(_openApiState);
            Bogus.Randomizer.Seed = new Random(Config.GlobalSeed);
        }

        public GeneratorConfig Config { get; }

        public T GetApiClient<T>() where T : OpenApiBaseClient
        {
            try
            {
                var result = (T)Activator.CreateInstance(typeof(T), _openApiState);
                _logger.Debug($"Api client of type {typeof(T).Name} created.", result);
                return result;
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Cannot create Api client of type {typeof(T).Name}.");
                throw;
            }
        }

        public async Task<IEnumerable<T>> GenerateAll<T>(CancellationToken cancellationToken = default) where T : OpenApi.Reference.Entity
        {
            //todo: add cancellation token

            IEnumerable <T> entities;
            IApiWrappedClient<T> apiClient;
            IGenerationSettings<T> settings;

            try
            {
                entities = GetRandomizer<T>().GenerateList();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Cannot create randomized {entity} collection.", typeof(T).Name);
                throw;
            }
            try
            {
                apiClient = GetApiWrappedClient<T>();
                settings = Config.GetGenerationSettings<T>();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Cannot create api wrapped client {entity}.", typeof(T).Name);
                throw;
            }

            // logger in api client, so without try catch
            if (settings.GenerateInParallel)
                return await apiClient.CreateAllInParallel(entities, settings.MaxExecutionThreadsParallel, cancellationToken);
            else
                return await apiClient.CreateAllSequentially(entities, settings.SkipErrorsSequent, cancellationToken);
        }

        public async Task<T> GenerateSingle<T>(CancellationToken cancellationToken = default) where T : OpenApi.Reference.Entity
        {
            _logger.Debug("Start generating single {entity}", typeof(T).Name);
            T entity;
            IApiWrappedClient<T> apiClient;
            try
            {
                entity = GetRandomizer<T>().Generate();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Cannot create randomized {entity}.", typeof(T).Name);
                throw;
            }
            try
            {
                apiClient = GetApiWrappedClient<T>();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Cannot create api wrapped client {entity}.", typeof(T).Name);
                throw;
            }

            _logger.Info("Start generating single {entity}.", typeof(T).Name);


            // logger in api client, so without try catch
            return await apiClient.Create(entity, cancellationToken);
        }

        public async Task Login() => await _loginClient.Login();

        public async Task Logout() => await _loginClient.Logout();

        protected IRandomizer<T> GetRandomizer<T>() where T : OpenApi.Reference.Entity
        {
            return new Randomizer<T>(Config.GetRandomizerSettings<T>());
        }

        protected IApiWrappedClient<T> GetApiWrappedClient<T>() where T : OpenApi.Reference.Entity
        {
            switch (typeof(T).Name)
            {
                // typeof cannot be used in switch clause
                case nameof(OpenApi.Reference.Lead):
                    return (IApiWrappedClient<T>) new LeadApiWrappedClient(_openApiState);
                default:
                    throw new NotSupportedException($"This type of entity is not supported. Type: {typeof(T).Name}");
            }
        }

        void IDisposable.Dispose()
        {
            ((IDisposable)_openApiState).Dispose();
        }
    }
}
