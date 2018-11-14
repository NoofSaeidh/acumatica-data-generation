using DataGeneration.Entities;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DataGeneration.Common
{
    public abstract class GenerationRunner
    {
        private static Func<ApiConnectionConfig, CancellationToken, Task<ILoginLogoutApiClient>> _apiClientFactory;

        // HACK: set this if you want use another api client
        public static Func<ApiConnectionConfig, CancellationToken, Task<ILoginLogoutApiClient>> ApiClientFactory
        {
            get => _apiClientFactory ?? (_apiClientFactory = async (config, ct) => (ILoginLogoutApiClient)await Soap.AcumaticaSoapClient.LoginLogoutClientAsync(config, ct));
            set => _apiClientFactory = value;
        }

        public abstract Task RunGeneration(CancellationToken cancellationToken = default);
    }

    public abstract class GenerationRunner<TEntity, TGenerationSettings> : GenerationRunner
        where TEntity : class
        where TGenerationSettings : class, IGenerationSettings<TEntity>
    {
        private Bogus.Randomizer _randomizer;

        protected GenerationRunner(ApiConnectionConfig apiConnectionConfig, TGenerationSettings generationSettings)
        {
            ApiConnectionConfig = apiConnectionConfig ?? throw new ArgumentNullException(nameof(apiConnectionConfig));
            GenerationSettings = generationSettings ?? throw new ArgumentNullException(nameof(generationSettings));
        }

        public ApiConnectionConfig ApiConnectionConfig { get; }
        public TGenerationSettings GenerationSettings { get; }
        protected static ILogger Logger { get; } = LogManager.GetLogger(LogManager.LoggerNames.GenerationRunner);
        protected Bogus.Randomizer Randomizer => _randomizer ?? (_randomizer = new Bogus.Randomizer(GenerationSettings.RandomizerSettings.Seed));

        protected virtual void ValidateGenerationSettings()
        {
            GenerationSettings.Validate();
        }

        // please do not override this (it contains loggers, try catches and stopwatch)
        // instead override RunGenerationSequentRaw (for single thread)
        // or RunBeforeGeneration (for all threads before entire generation)
        public override async Task RunGeneration(CancellationToken cancellationToken = default)
        {
            ValidateGenerationSettings();
            Logger.Info("Generation with following settings is going to start {@settings}", GenerationSettings);

            try
            {
                // log only if it takes some time
                using (StopwatchLoggerFactory.ForceLogDisposeTimeCheck(Logger, TimeSpan.FromSeconds(10),
                    "Before Generation completed. " + LogArgs.Type_Id, 
                    GenerationSettings.GenerationType, GenerationSettings.Id))
                {
                    await RunBeforeGeneration(cancellationToken);
                }

                if (GenerationSettings.Count == 0)
                {
                    Logger.Warn("Generation was not started. No entities could be generated. " + LogArgs.Type_Id_Count, 
                        GenerationSettings.GenerationType, GenerationSettings.Id, GenerationSettings.Count);
                    return;
                }

                using (StopwatchLoggerFactory.ForceLogStartDispose(Logger,
                    "Generation started. " + LogArgs.Type_Id_Count,
                    "Generation completed. " + LogArgs.Type_Id_Count,
                    GenerationSettings.GenerationType, GenerationSettings.Id, GenerationSettings.Count))
                {
                    switch (GenerationSettings.ExecutionTypeSettings.ExecutionType)
                    {
                        case ExecutionType.Sequent:
                            await RunGenerationSequent(GenerationSettings.Count, cancellationToken);
                            break;

                        case ExecutionType.Parallel:
                            await RunGenerationParallel(cancellationToken);
                            break;

                        default:
                            break;
                    }
                }
            }
            catch (OperationCanceledException oce)
            {
                Logger.Error(oce, "Generation was canceled. " + LogArgs.Type_Id,
                    GenerationSettings.GenerationType, GenerationSettings.Id);
                throw;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Generation failed. " + LogArgs.Type_Id,
                    GenerationSettings.GenerationType, GenerationSettings.Id);
                throw GenerationException.NewFromEntityType<TEntity>(e);
            }

            Logger.Info("Generation completed successfully. " + LogArgs.Type_Id,
                    GenerationSettings.GenerationType, GenerationSettings.Id);
        }

        protected virtual Task RunBeforeGeneration(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        protected async Task RunGenerationParallel(CancellationToken cancellationToken)
        {
            var threads = GenerationSettings.ExecutionTypeSettings.ParallelThreads;
            if (GenerationSettings.Count < threads)
                threads = GenerationSettings.Count;
            var tasks = new Task[threads];

            var countPerThread = GenerationSettings.Count / threads;
            var remainUnits = GenerationSettings.Count % threads;

            using (StopwatchLoggerFactory.ForceLogStartDispose(Logger, LogLevel.Debug,
                "Generation Parallel started. " + LogArgs.Type_Id_Count_Threads,
                "Generation Parallel completed. " + LogArgs.Type_Id_Count_Threads,
                GenerationSettings.GenerationType, GenerationSettings.Id, GenerationSettings.Count, threads))
            {
                for (int i = 0, rem = 1; i < threads; i++)
                {
                    // remaining of division
                    if (i >= remainUnits) rem = 0;

                    var currentCount = countPerThread + rem;
                    tasks[i] = RunGenerationSequent(currentCount, cancellationToken);
                }

                await Task.WhenAll(tasks);
            }
        }

        protected async Task RunGenerationSequent(int count, CancellationToken cancellationToken)
        {
            using (StopwatchLoggerFactory.ForceLogStartDispose(Logger, LogLevel.Debug,
                "Generation Sequent started. " + LogArgs.Type_Id_Count,
                "Generation Sequent completed. " + LogArgs.Type_Id_Count,
                GenerationSettings.GenerationType, GenerationSettings.Id, count))
            {
                var entities = GenerateRandomizedList(count);

                cancellationToken.ThrowIfCancellationRequested();

                using (var client = await GetLoginLogoutClient(cancellationToken))
                {
                    foreach (var entity in entities)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        try
                        {
                            await GenerateSingle(client, entity, cancellationToken);
                        }
                        catch (OperationCanceledException) { throw; }
                        catch (ApiException ae)
                        {
                            Logger.Error(ae, "Generation {$entity} failed", typeof(TEntity));
                            if (!GenerationSettings.ExecutionTypeSettings.IgnoreProcessingErrors)
                                throw;
                        }
                        catch (Exception e)
                        {
                            Logger.Fatal(e, "Unexpected exception has occurred");
                            throw;
                        }
                    }
                }
            }
        }

        protected abstract Task GenerateSingle(IApiClient client, TEntity entity, CancellationToken cancellationToken);

        protected async Task<ILoginLogoutApiClient> GetLoginLogoutClient(CancellationToken cancellationToken = default)
        {
            return await ApiClientFactory(ApiConnectionConfig, cancellationToken);
        }

        protected virtual IList<TEntity> GenerateRandomizedList(int count) => GenerationSettings.RandomizerSettings.GetDataGenerator().GenerateList(count);

        protected void ChangeGenerationCount(int count, string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            var originCount = GenerationSettings.Count;
            GenerationSettings.Count = count;
            Logger.Info("Count changed from {originCount} to {count}. Reason: {message}, caller: {callerinfo}", 
                originCount, count, message, $"{memberName} at {sourceFilePath}:{sourceLineNumber}");
        }

        private async Task<IEnumerable<Soap.Entity>> GetListFactory(Soap.Entity entity, CancellationToken ct)
        {
            using (var client = await GetLoginLogoutClient(ct))
            {
                return await client.GetListAsync(entity, ct);
            }
        }

        protected EntitySearcher GetEntitySearcher(Func<Soap.Entity> factory)
        {
            return new EntitySearcher(GetListFactory, factory);
        }

        protected EntitySearcher GetEntitySearcher(string entityType) => GetEntitySearcher(() => EntityHelper.InitializeFromType(entityType));

        protected Task<IList<Soap.Entity>> GetEntities(SearchPattern searchPattern, CancellationToken ct)
        {
            return GetEntities(searchPattern, null, ct);
        }

        protected async Task<IList<Soap.Entity>> GetEntities(SearchPattern searchPattern, Action<EntitySearcher> searcherAdjustment,  CancellationToken ct)
        {
            if (searchPattern == null)
                throw new ArgumentNullException(nameof(searchPattern));


            var searcher = GetEntitySearcher(searchPattern.EntityType)
                .AdjustInput(adj =>
                    adj.Adjust(e => e.ReturnBehavior = Soap.ReturnBehavior.OnlySpecified)
                        // perhaps it is not a better design
                       .AdjustIfIs<Soap.IAdjustReturnBehaviorEntity>(e => e.AdjustReturnBehavior()));

            searchPattern.AdjustSearcher(searcher);
            searcherAdjustment?.Invoke(searcher);
            
            return await searcher.ExecuteSearch(ct);
        }

        protected ComplexQueryExecutor GetComplexQueryExecutor() => new ComplexQueryExecutor(GetListFactory);


        private static class LogArgs
        {
            public const string Type_Id = "Type = {type}, Id = {id}";
            public const string Type_Id_Count = Type_Id + ", Count = {count}";
            public const string Type_Id_Count_Threads = Type_Id_Count + ", Threads = {threads}";
        }


    }

    // provides search in RunBeforeGeneration
    // and changes count
    public abstract class EntitiesSearchGenerationRunner<TEntity, TGenerationSettings> : GenerationRunner<TEntity, TGenerationSettings>
        where TEntity : class
        where TGenerationSettings : class, IGenerationSettings<TEntity>, IEntitiesSearchGenerationSettings
    {
        protected EntitiesSearchGenerationRunner(ApiConnectionConfig apiConnectionConfig, TGenerationSettings generationSettings) : base(apiConnectionConfig, generationSettings)
        {
        }

        // override to true if no need to GetEntities in RunBeforeGeneration
        protected virtual bool SkipEntitiesSearch => false;

        protected abstract void UtilizeFoundEntities(IList<Soap.Entity> entities);
        protected virtual void AdjustEntitySearcher(EntitySearcher searcher)
        {
        }

        protected override async Task RunBeforeGeneration(CancellationToken cancellationToken = default)
        {
            await base.RunBeforeGeneration(cancellationToken);

            if (SkipEntitiesSearch)
                return;

            var entities = await GetEntities(GenerationSettings.SearchPattern, AdjustEntitySearcher, cancellationToken);
            var complexEntities = entities.OfType<Soap.IComplexQueryEntity>();
            if (complexEntities.Any())
            {
                await GetComplexQueryExecutor().Execute(complexEntities, cancellationToken);
            }
            UtilizeFoundEntities(entities);
        }

        protected override void ValidateGenerationSettings()
        {
            base.ValidateGenerationSettings();
            if (GenerationSettings.SearchPattern is null)
                throw new ValidationException($"Property {nameof(SearchPattern)} of {nameof(GenerationSettings)} must be not null in order to search entities in {nameof(RunBeforeGeneration)}");
        }
    }
}