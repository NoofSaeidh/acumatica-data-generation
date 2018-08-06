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
    public abstract class GenerationRunner
    {
        public abstract Task RunGeneration(CancellationToken cancellationToken = default);
    }

    public abstract class GenerationRunner<TEntity, TGenerationSettings> : GenerationRunner 
        where TEntity : Soap.Entity
        where TGenerationSettings : class, IGenerationSettings<TEntity>
    {
        private Bogus.Randomizer _bogusRandomizer;
        protected GenerationRunner(ApiConnectionConfig apiConnectionConfig, TGenerationSettings generationSettings)
        {
            ApiConnectionConfig = apiConnectionConfig ?? throw new ArgumentNullException(nameof(apiConnectionConfig));
            GenerationSettings = generationSettings ?? throw new ArgumentNullException(nameof(generationSettings));
        }

        public ApiConnectionConfig ApiConnectionConfig { get; }
        public TGenerationSettings GenerationSettings { get; }
        protected ILogger Logger => LogConfiguration.DefaultLogger;
        protected Bogus.Randomizer Randomizer => _bogusRandomizer ?? (_bogusRandomizer = new Bogus.Randomizer(GenerationSettings.RandomizerSettings.Seed));


        public override async Task RunGeneration(CancellationToken cancellationToken = default)
        {
            GenerationSettings.Validate();

            Logger.Info("Generation of {type} with count: {count} started. Settings: {@settings}",
                GenerationSettings.GenerationEntity, GenerationSettings.Count, GenerationSettings);
            var stopwatch = new Stopwatch();

            try
            {
                stopwatch.Start();
                switch (GenerationSettings.ExecutionTypeSettings.ExecutionType)
                {
                    case ExecutionType.Sequent:
                        await RunGenerationSequentRaw(GenerationSettings.Count, cancellationToken);
                        break;
                    case ExecutionType.Parallel:
                        await RunGenerationParallel(cancellationToken);
                        break;
                    default:
                        break;
                }
                stopwatch.Stop();
            }
            catch(Exception e)
            {
                Logger.Error(e, "Generation failed.");
                throw;
            }

            Logger.Info("Generation of {type} with count: {count} completed. Time elapsed: {time}.",
                GenerationSettings.GenerationEntity, GenerationSettings.Count, stopwatch.Elapsed);
        }

        protected Task RunGenerationParallel(CancellationToken cancellationToken)
        {
            var threads = GenerationSettings.ExecutionTypeSettings.ParallelThreads;
            var tasks = new Task[threads];

            var count = GenerationSettings.Count / threads;
            var remain = GenerationSettings.Count % threads;

            for (int i = 0, rem = 0; i < threads; i++)
            {
                // remaining of division
                if (remain > i) rem = 1;

                var currentCount = count + rem;
                tasks[i] = RunGenerationSequentRaw(currentCount, cancellationToken);
            }

            return Task.WhenAll(tasks);
        }

        protected abstract Task RunGenerationSequentRaw(int count, CancellationToken cancellationToken);

        protected Task<Soap.AcumaticaSoapClient> GetLoginLogoutClient() => Soap.AcumaticaSoapClient.LoginLogoutClientAsync(ApiConnectionConfig, !GenerationSettings.ExecutionTypeSettings.IgnoreProcessingErrors);

        protected IList<TEntity> GenerateRandomizedList(int count) => GenerationSettings.RandomizerSettings.GetDataGenerator().GenerateList(count);
    }
}
