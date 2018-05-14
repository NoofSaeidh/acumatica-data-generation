using CrmDataGeneration.Common;
using CrmDataGeneration.Entities.Emails;
using CrmDataGeneration.Entities.Leads;
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

        public T GetRawApiClient<T>() where T : OpenApiBaseClient
        {
            try
            {
                var result = (T)Activator.CreateInstance(typeof(T), _openApiState);
                _logger.Debug($"API client of type {typeof(T).Name} created.", result);
                return result;
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Cannot create API client of type {typeof(T).Name}.");
                throw;
            }
        }

        public T GetApiWrappedClient<T>() where T : ApiWrappedClient
        {
            try
            {
                var result = (T)Activator.CreateInstance(typeof(T), _openApiState);
                _logger.Debug($"Wrapped API client of type {typeof(T).Name} created.", result);
                return result;
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Cannot create wrapped API client of type {typeof(T).Name}.");
                throw;
            }
        }

        public IApiWrappedClient<T> GetApiWrappedClientForEntity<T>() where T : OpenApi.Reference.Entity
        {
            switch (typeof(T).Name)
            {
                case nameof(OpenApi.Reference.Lead):
                    return (IApiWrappedClient<T>)GetApiWrappedClient<LeadApiWrappedClient>();
                case nameof(OpenApi.Reference.Email):
                    return (IApiWrappedClient<T>)GetApiWrappedClient<EmailApiWrappedClient>();
                default:
                    var exception = new NotSupportedException($"There are predefined API wrapped client for {typeof(T).Name}.");
                    _logger.Error(exception);
                    throw exception;
            }
        }

        public async Task GenerateAllOptions(CancellationToken cancellationToken = default)
        {
            if (Config.GenerationOptions == null)
                throw new InvalidOperationException($"{nameof(Config.GenerationOptions)} options is not specified for {nameof(Config)}.");
            _logger.Info("Start generation for all options. {@options}", Config.GenerationOptions);
            foreach (var option in Config.GenerationOptions)
            {
                _logger.Info("Start generation for option: {@option}", option);
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await option.RunGeneration(this, cancellationToken);

                }
                catch (Exception e)
                {
                    _logger.Error(e, "Generation failed. {@option}", option);
                }
            }
        }

        public async Task<IEnumerable<T>> GenerateAll<T>(GenerationOption<T> option, CancellationToken cancellationToken = default) where T : OpenApi.Reference.Entity
        {
            if (option == null)
                throw new ArgumentNullException(nameof(option));

            IEnumerable<T> entities;
            IApiWrappedClient<T> apiClient;

            try
            {
                entities = option.RandomizerSettings.GetRandomizer().GenerateList(option.Count);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Cannot create randomized {entity} collection.", typeof(T).Name);
                throw;
            }
            try
            {
                apiClient = GetApiWrappedClientForEntity<T>();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Cannot create api wrapped client {entity}.", typeof(T).Name);
                throw;
            }

            // logger in api client, so without try catch and logger
            return await apiClient.CreateAll(entities, option.ExecutionTypeSettings, cancellationToken);
        }

        public async Task<T> GenerateSingle<T>(IRandomizerSettings<T> randomizerSettings, CancellationToken cancellationToken = default) where T : OpenApi.Reference.Entity
        {
            _logger.Debug("Start generating single {entity}", typeof(T).Name);
            T entity;
            IApiWrappedClient<T> apiClient;
            try
            {
                entity = randomizerSettings.GetRandomizer().Generate();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Cannot create randomized {entity}.", typeof(T).Name);
                throw;
            }
            try
            {
                apiClient = GetApiWrappedClientForEntity<T>();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Cannot create api wrapped client {entity}.", typeof(T).Name);
                throw;
            }

            _logger.Info("Start generating single {entity}.", typeof(T).Name);


            // logger in api client, so without try catch
            return await apiClient.CreateSingle(entity, cancellationToken);
        }

        public virtual async Task Login() => await _loginClient.Login();

        public virtual async Task Logout() => await _loginClient.Logout();

        void IDisposable.Dispose()
        {
            ((IDisposable)_openApiState).Dispose();
        }
    }
}
