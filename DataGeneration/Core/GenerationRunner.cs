﻿using DataGeneration.Core.Api;
using DataGeneration.Core.Cache;
using DataGeneration.Core.Logging;
using DataGeneration.Core.Settings;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DataGeneration.Core.Common;

namespace DataGeneration.Core
{
    public abstract class GenerationRunner
    {
        // HACK: set this if you want use another api client
        public static Lazy<Func<EndpointSettings, ILogoutApiClient>> ApiClientFactoryInitializer =
            new Lazy<Func<EndpointSettings, ILogoutApiClient>>(() => Soap.AcumaticaSoapClient.LogoutClient);

        public static Func<EndpointSettings, ILogoutApiClient> ApiLogoutClientFactory => ApiClientFactoryInitializer.Value;

        public static Func<ApiConnectionConfig, Task<ILogoutApiClient>> ApiLoginLogoutClientFactory
        {
            get
            {
                return async config =>
                {
                    var client = ApiLogoutClientFactory(config.EndpointSettings);
                    await client.LoginAsync(config.LoginInfo);
                    return client;
                };
            }
        }

        protected static ILogger Logger { get; } = LogHelper.GetLogger(LogHelper.LoggerNames.GenerationRunner);

        public abstract Task RunGeneration(CancellationToken cancellationToken = default);

        #region Events
        public event EventHandler<RunBeforeGenerationStartedEventArgs> RunBeforeGenerationStarted;
        public event EventHandler<RunGenerationStartedEventArgs> RunGenerationStarted;
        public event EventHandler<RunGenerationCompletedEventArgs> RunGenerationCompleted;
        public event EventHandler<RunAfterGenerationStartedEventArgs> RunAfterGenerationStarted;
        public event EventHandler<GenerationCompletedEventArgs> GenerationCompleted;
        protected virtual void OnRunBeforeGenerationStarted(RunBeforeGenerationStartedEventArgs e)
        {
            try
            {
                RunBeforeGenerationStarted?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"{nameof(RunBeforeGenerationStarted)} event failed.");
            }
        }
        protected virtual void OnRunGenerationStarted(RunGenerationStartedEventArgs e)
        {
            try
            {
                RunGenerationStarted?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"{nameof(RunGenerationStarted)} event failed.");
            }
        }
        protected virtual void OnRunGenerationCompleted(RunGenerationCompletedEventArgs e)
        {
            try
            {
                RunGenerationCompleted?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"{nameof(RunGenerationCompleted)} event failed.");
            }
        }
        protected virtual void OnRunAfterGenerationStarted(RunAfterGenerationStartedEventArgs e)
        {
            try
            {
                RunAfterGenerationStarted?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"{nameof(RunAfterGenerationStarted)} event failed.");
            }
        }
        protected virtual void OnGenerationCompleted(GenerationCompletedEventArgs e)
        {
            try
            {
                GenerationCompleted?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"{nameof(GenerationCompleted)} event failed.");
            }
        }
        #endregion
    }

    public abstract class GenerationRunner<TEntity, TGenerationSettings> : GenerationRunner
        where TEntity : class
        where TGenerationSettings : class, IGenerationSettings<TEntity>
    {
        private const string ThreadIndexParam = "ThreadIndex";
        private Bogus.Randomizer _randomizer;

        protected GenerationRunner(ApiConnectionConfig apiConnectionConfig, TGenerationSettings generationSettings)
        {
            ApiConnectionConfig = apiConnectionConfig ?? throw new ArgumentNullException(nameof(apiConnectionConfig));
            GenerationSettings = generationSettings ?? throw new ArgumentNullException(nameof(generationSettings));
        }

        public ApiConnectionConfig ApiConnectionConfig { get; }
        public TGenerationSettings GenerationSettings { get; }
        protected Bogus.Randomizer Randomizer => _randomizer ?? (_randomizer = new Bogus.Randomizer(GenerationSettings.RandomizerSettings.Seed));

        protected virtual void ValidateGenerationSettings()
        {
            GenerationSettings.Validate();
        }

        public sealed override async Task RunGeneration(CancellationToken cancellationToken = default)
        {
            ValidateGenerationSettings();
            Logger.Info("Generation is going to start. " + LogArgs.Type_Id_Count + ", {@settings}",
                GenerationSettings.GenerationType, GenerationSettings.Id, GenerationSettings.Count, GenerationSettings);

            try
            {
                if (GenerationSettings.CollectGarbageBeforeGeneration)
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Memory before GC.Collect: {byte:N0}", GC.GetTotalMemory(false));
                        GC.Collect();
                        Logger.Debug("Memory after GC.Collect: {byte:N0}", GC.GetTotalMemory(true));
                    }
                    else
                        GC.Collect();
                }

                OnRunBeforeGenerationStarted(new RunBeforeGenerationStartedEventArgs(GenerationSettings));

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

                OnRunGenerationStarted(new RunGenerationStartedEventArgs(GenerationSettings));
                using (StopwatchLoggerFactory.ForceLogStartDispose(Logger,
                    LogLevel.Info,
                    "Generation started. " + LogArgs.Type_Id_Count,
                    "Generation completed. " + LogArgs.Type_Id_Count,
                    args: Params.ToArray<object>(GenerationSettings.GenerationType, GenerationSettings.Id, GenerationSettings.Count),
                    callback: LogResults))
                {
                    switch (GenerationSettings.ExecutionTypeSettings.ExecutionType)
                    {
                        case ExecutionType.Sequent:
                            await RunGenerationSequent(GenerationSettings.Count, 0, cancellationToken);
                            break;

                        case ExecutionType.Parallel:
                            await RunGenerationParallel(cancellationToken);
                            break;

                        default:
                            throw new NotSupportedException();
                    }
                    Logger.Info("Generation completed. " + LogArgs.Type_Id,
                        GenerationSettings.GenerationType, GenerationSettings.Id);
                }

                OnRunAfterGenerationStarted(new RunAfterGenerationStartedEventArgs(GenerationSettings));

                // log only if it takes some time
                using (StopwatchLoggerFactory.ForceLogDisposeTimeCheck(Logger, TimeSpan.FromSeconds(10),
                    "After Generation completed. " + LogArgs.Type_Id,
                    GenerationSettings.GenerationType, GenerationSettings.Id))
                {
                    await RunAfterGeneration(cancellationToken);
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
            finally
            {
                OnGenerationCompleted(new GenerationCompletedEventArgs(GenerationSettings));
            }
        }

        protected virtual Task RunBeforeGeneration(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        protected virtual Task RunAfterGeneration(CancellationToken cancellationToken = default)
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
                args: Params.ToArray<object>(
                    GenerationSettings.GenerationType,
                    GenerationSettings.Id,
                    GenerationSettings.Count,
                    threads)))
            {
                for (int i = 0, rem = 1; i < threads; i++)
                {
                    // remaining of division
                    if (i >= remainUnits) rem = 0;

                    var currentCount = countPerThread + rem;
                    int threadIndex = i;
                    tasks[i] = RunGenerationSequent(currentCount, threadIndex, cancellationToken);
                }

                await Task.WhenAll(tasks);
            }
        }

        protected async Task RunGenerationSequent(int count, int threadIndex, CancellationToken cancellationToken)
        {
            using (StopwatchLoggerFactory.ForceLogStartDispose(Logger, LogLevel.Debug,
                "Generation Sequent started. " + LogArgs.Type_Id_Count,
                "Generation Sequent completed. " + LogArgs.Type_Id_Count,
                args: Params.ToArray<object>(GenerationSettings.GenerationType, GenerationSettings.Id, count),
                eventParams: Params.ToArray<(object, object)>((ThreadIndexParam, threadIndex))))
            {
                var entities = GenerateRandomizedList(count);

                cancellationToken.ThrowIfCancellationRequested();

                using (var client = await GetLoginLogoutClient(threadIndex, cancellationToken))
                {
                    foreach (var entity in entities)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        try
                        {
                            await GenerateSingle(client, entity, cancellationToken);
                        }
                        catch (OperationCanceledException) { throw; }
                        catch (Exception e)
                        {
                            var message = e is ApiException
                                ? "Generation {$entity} failed"
                                : "Unexpected exception has occurred while processing {entity}";

                            Logger.LogWithEventParams( 
                                LogLevel.Error,
                                message,
                                args: Params.ToArray(typeof(TEntity)),
                                exception: e,
                                eventParams: Params.ToArray<(object, object)>((ThreadIndexParam, threadIndex)));

                            if (!GenerationSettings.ExecutionTypeSettings.IgnoreProcessingErrors)
                                break;
                        }
                    }
                }
            }
        }

        protected abstract Task GenerateSingle(IApiClient client, TEntity entity, CancellationToken ct);

        protected ILogoutApiClient GetLogoutClient()
        {
            var client = ApiLogoutClientFactory(ApiConnectionConfig.EndpointSettings);
            client.RetryCount = GenerationSettings.ExecutionTypeSettings.RetryCount;
            return client;
        }

        protected async Task<ILogoutApiClient> GetLoginLogoutClient(CancellationToken ct = default)
        {
            var client = GetLogoutClient();
            await client.LoginAsync(ApiConnectionConfig.LoginInfo, ct);
            return client;
        }

        protected async Task<ILogoutApiClient> GetLoginLogoutClient(int threadIndex, CancellationToken ct = default)
        {
            var client = GetLogoutClient();
            if (client is ILoggerInjectable inj) inj.InjectEventParameters((ThreadIndexParam, threadIndex));
            await client.LoginAsync(ApiConnectionConfig.LoginInfo, ct);
            return client;
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

        protected virtual void LogResults(TimeSpan time)
        {
            LogResultsArgs(out var entity, out var parentEntity, out var action);
            LogHelper.ResultsLogger
                .Info("Entity: {entity}, " +
                      "Parent Entity: {parentEntity}, " +
                      "Action: {action}, " +
                      "Count: {count}, " +
                      "Time: {time} ({time-sec} (sec)), " +
                      "Count per sec: {count-per-sec}",
                    entity,
                    parentEntity,
                    action,
                    GenerationSettings.Count,
                    time,
                    time.TotalSeconds,
                    GenerationSettings.Count / time.TotalSeconds
                );
        }

        protected virtual void LogResultsArgs(out string entity, out string parentEntity, out string action)
        {
            entity = typeof(TEntity).Name;
            parentEntity = entity;
            action = null;
        }

        private static class LogArgs
        {
            public const string Type_Id = "Type = {type}, Id = {id}";
            public const string Type_Id_Count = Type_Id + ", Count = {count}";
            public const string Type_Id_Count_Threads = Type_Id_Count + ", Threads = {threads}";
        }
    }
}