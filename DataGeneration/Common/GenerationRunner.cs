using NLog;
using System;
using System.Collections.Generic;
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
        protected static ILogger Logger => LogManager.GetLogger(LogManager.LoggerNames.GenerationRunner);
        protected Bogus.Randomizer Randomizer => _randomizer ?? (_randomizer = new Bogus.Randomizer(GenerationSettings.RandomizerSettings.Seed));

        // please do not override this (it contains loggers, try catches and stopwatch)
        // instead override RunGenerationSequentRaw (for single thread)
        // or RunBeforeGeneration (for all threads before entiry generation)
        public override async Task RunGeneration(CancellationToken cancellationToken = default)
        {
            GenerationSettings.Validate();

            Logger.Info("Generation {type} started. Count: {count}. Settings: {@settings}",
                GenerationSettings.GenerationEntity, GenerationSettings.Count, GenerationSettings);
            var stopwatch = new Stopwatch();

            try
            {
                stopwatch.Start();
                using (StopwatchLoggerFactory.Log(LogManager.LoggerNames.GenerationRunner, "Before Generation"))
                {
                    await RunBeforeGeneration(cancellationToken);
                }
                if (GenerationSettings.Count == 0)
                {
                    Logger.Warn("Generation {type} was not started. No entities could be generated. Count: 0");
                    return;
                }
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
                stopwatch.Stop();
            }
            catch (OperationCanceledException oce)
            {
                Logger.Error(oce, "Generation was canceled");
                throw;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Generation failed");
                throw GenerationException.NewFromEntityType<TEntity>(e);
            }

            Logger.Info("Generation {type} completed. Count: {count}. Time elapsed: {time}",
                GenerationSettings.GenerationEntity, GenerationSettings.Count, stopwatch.Elapsed);
        }

        protected virtual Task RunBeforeGeneration(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        protected Task RunGenerationParallel(CancellationToken cancellationToken)
        {
            var threads = GenerationSettings.ExecutionTypeSettings.ParallelThreads;
            if (GenerationSettings.Count < threads)
                threads = GenerationSettings.Count;
            var tasks = new Task[threads];

            var countPerThread = GenerationSettings.Count / threads;
            var remainUnits = GenerationSettings.Count % threads;

            using (StopwatchLoggerFactory.Log(LogManager.LoggerNames.GenerationRunner, "Generation Parallel. {count}, {threads}", GenerationSettings.Count, threads))
            {
                for (int i = 0, rem = 1; i < threads; i++)
                {
                    // remaining of division
                    if (i >= remainUnits) rem = 0;

                    var currentCount = countPerThread + rem;
                    tasks[i] = RunGenerationSequent(currentCount, cancellationToken);
                }

                return Task.WhenAll(tasks);
            }
        }

        protected async Task RunGenerationSequent(int count, CancellationToken cancellationToken)
        {
            using (StopwatchLoggerFactory.Log(LogManager.LoggerNames.GenerationRunner, "Generation Sequent. {count}", count))
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

        protected IList<TEntity> GenerateRandomizedList(int count) => GenerationSettings.RandomizerSettings.GetDataGenerator().GenerateList(count);

        protected void ChangeGenerationCount(int count, string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            GenerationSettings.Count = count;
            Logger.Info("Count changed to {count}; Reason :{message}; caller: {callerinfo}", count, message, $"{memberName} at {sourceFilePath}:{sourceLineNumber}");
        }
    }
}