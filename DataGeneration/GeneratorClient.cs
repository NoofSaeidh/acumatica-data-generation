using DataGeneration.Common;
using DataGeneration.GenerationInfo;
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
        private static ILogger _logger { get; } = Common.LogManager.GetLogger(Common.LogManager.LoggerNames.GenerationClient);

        static GeneratorClient()
        {
            // hack: set it not much bigger that count of used threads
            System.Net.ServicePointManager.DefaultConnectionLimit = 42;
        }

        public async Task<AllLaunchesResult> GenerateAll(
            GeneratorConfig config, 
            CancellationToken ct = default)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            try
            {
                config.Validate();
            }
            catch (ValidationException ve)
            {
                _logger.Fatal(ve, "Cannot start generation, Config is invalid");
                throw;
            }

            var launches = config.GetAllLaunches().ToList();

            _logger.Info("Start generation all settings for all launches, " +
                "Lunch count {count}, Config = {@config}, Launches = {@launches}",
                launches.Count, config, launches);

            var results = new List<AllGenerationsResult>(launches.Count);
            foreach (var launch in launches)
            {
                results.Add(await Generate(config, launch, ct));
            }
            return new AllLaunchesResult(results);
        }

        protected async Task<AllGenerationsResult> Generate(
            GeneratorConfig config, 
            GenerationLaunchSettings launchSettings, 
            CancellationToken ct = default)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (launchSettings == null)
                throw new ArgumentNullException(nameof(launchSettings));

            try
            {
                config.Validate();
                launchSettings.Validate();
            }
            catch (ValidationException ve)
            {
                _logger.Fatal(ve, "Cannot start generation, configuration is invalid");
                throw;
            }

            var generatationResults = new List<GenerationResult>();
            var settingsCollection = launchSettings.GetPreparedGenerationSettings().ToList();

            _logger.Info("Start generation all settings for launch, Count = {count}, Id = {id}, Launch = {@launch}",
                settingsCollection.Count, launchSettings.Id, launchSettings);

            bool stopProcessing = false;
            foreach (var settings in settingsCollection)
            {
                GenerationResult result;
                try
                {
                    ct.ThrowIfCancellationRequested();

                    await settings.GetGenerationRunner(config.ApiConnectionConfig).RunGeneration(ct).ConfigureAwait(false);
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
                    if (launchSettings.StopProccesingAtExeception)
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
            _logger.Info("Generation all settings for launch completed");
            return new AllGenerationsResult(generatationResults, stopProcessing);
        }
    }
}