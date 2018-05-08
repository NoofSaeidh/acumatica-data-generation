﻿using CrmDataGeneration.Common;
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

        public async Task GenerateAllOptions(CancellationToken cancellationToken = default)
        {
            if (Config.GenerationOptions == null)
                throw new InvalidOperationException($"{nameof(Config.GenerationOptions)} options is not specified for {nameof(Config)}.");
            _logger.Info("Start generation for all options. {@options}", Config.GenerationOptions);
            foreach (var option in Config.GenerationOptions)
            {
                _logger.Info("Start generation for option: {@option}", option);
                cancellationToken.ThrowIfCancellationRequested();
                await option.RunGeneration(this, cancellationToken);
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
                entities = GetRandomizer<T>(option.RandomizerSettings).GenerateList(option.Count);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Cannot create randomized {entity} collection.", typeof(T).Name);
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

            // logger in api client, so without try catch
            if (option.GenerateInParallel)
                return await apiClient.CreateAllInParallel(entities, option.MaxExecutionThreadsParallel, cancellationToken);
            else
                return await apiClient.CreateAllSequentially(entities, option.SkipErrorsSequent, cancellationToken);
        }

        public async Task<T> GenerateSingle<T>(IRandomizerSettings<T> randomizerSettings, CancellationToken cancellationToken = default) where T : OpenApi.Reference.Entity
        {
            _logger.Debug("Start generating single {entity}", typeof(T).Name);
            T entity;
            IApiWrappedClient<T> apiClient;
            try
            {
                entity = GetRandomizer<T>(randomizerSettings).Generate();
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

        public virtual async Task Login() => await _loginClient.Login();

        public virtual async Task Logout() => await _loginClient.Logout();

        protected virtual IRandomizer<T> GetRandomizer<T>(IRandomizerSettings<T> randomizerSettings) where T : OpenApi.Reference.Entity
        {
            return new Randomizer<T>(randomizerSettings);
        }

        protected virtual IApiWrappedClient<T> GetApiWrappedClient<T>() where T : OpenApi.Reference.Entity
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
