using DataGeneration.Common;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataGeneration
{
    public class GeneratorClient
    {
        private static ILogger _logger => Common.LogManager.GetLogger(Common.LogManager.LoggerNames.GenerationClient);

        static GeneratorClient()
        {
            // hack: set it not much bigger that count of used threads
            System.Net.ServicePointManager.DefaultConnectionLimit = 42;
        }

        public GeneratorClient(GeneratorConfig config)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            if (Config.ApiConnectionConfig == null) throw new ArgumentException($"Property {nameof(Config.ApiConnectionConfig)}" +
                 $" of argument {nameof(config)} must not be null.");
        }

        public GeneratorConfig Config { get; }

        /// <summary>
        ///     Start generation all options defined in <see cref="Config"/>.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>Generation result for each generation setting.</returns>
        /// <exception cref="ValidationException">Config is invalid.</exception>
        /// <exception cref="OperationCanceledException">Operation was canceled.</exception>
        public async Task<AllGenerationsResult> GenerateAllOptions(CancellationToken cancellationToken = default)
        {
            try
            {
                Config.Validate();
            }
            catch (ValidationException ve)
            {
                _logger.Fatal(ve, "Cannot start generation, Config is invalid");
                throw;
            }
            var generatationResults = new List<GenerationResult>();
            var settingsCollection = Config.GetInjectedGenerationSettingsCollection().ToList();
            // count == 0 checked in Validate method
            if (settingsCollection.Count == 1)
                _logger.Info("Start generation all settings, Count = {count}", settingsCollection.Count);
            else
                _logger.Info("Start generation all settings, Count = {count}, {@settings}", settingsCollection.Count, settingsCollection);
            bool stopProcessing = false;
            foreach (var settings in settingsCollection)
            {
                GenerationResult result;
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await settings.GetGenerationRunner(Config.ApiConnectionConfig).RunGeneration(cancellationToken).ConfigureAwait(false);
                    result = new GenerationResult(settings);
                }
                catch (OperationCanceledException oce)
                {
                    _logger.Fatal(oce, "Operation was canceled");
                    throw;
                }
                catch (GenerationException ge)
                {
                    _logger.Error(ge, "Generation failed");
                    result = new GenerationResult(settings, ge);
                    if (Config.StopProccesingAtExeception)
                        stopProcessing = true;
                }
                catch (ValidationException ve)
                {
                    _logger.Error(ve, "Generation not started because of invalid configuration");
                    result = new GenerationResult(settings, ve);
                }
                // this should not happen
                catch (Exception e)
                {
                    _logger.Fatal(e, "Generation failed with unexpected error");
                    throw;
                }
                generatationResults.Add(result);
                if (stopProcessing)
                    break;
            }
            _logger.Info("Generation all settings completed");
            return new AllGenerationsResult(generatationResults, stopProcessing);
        }
    }

    public class AllGenerationsResult
    {
        internal AllGenerationsResult(IEnumerable<GenerationResult> generationResults, bool processingStopped = false)
        {
            GenerationResults = generationResults.ToArray();
            ProcessingStopped = processingStopped;
        }

        public bool ProcessingStopped { get; }
        public bool AllSucceeded => GenerationResults.All(g => g.Success);
        public bool AllFailed => GenerationResults.All(g => !g.Success);
        public GenerationResult[] GenerationResults { get; }
    }

    public class GenerationResult
    {
        internal GenerationResult(IGenerationSettings generationSettings, Exception exception = null)
        {
            GenerationSettings = generationSettings;
            Exception = exception;
        }

        public IGenerationSettings GenerationSettings { get; }
        public Exception Exception { get; }
        public bool Success => Exception == null;
    }
}