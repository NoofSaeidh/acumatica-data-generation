using DataGeneration.Core;
using DataGeneration.Core.Logging;
using DataGeneration.Core.SystemManagement;
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
        private static ILogger _logger { get; } = LogHelper.GetLogger(LogHelper.LoggerNames.GenerationClient);

        public async Task<AllBatchesResult> GenerateAll(
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

            config.ServicePointSettings?.ApplySettings();

            var batches = config.GetAllBatches(out var unqiueCount).ToList();

            var results = new List<AllGenerationsResult>(batches.Count);
            using (StopwatchLoggerFactory.ForceLogStartDispose(
                _logger,
                "Start generation all settings for all batches, " +
                "Lunches count {count}, Config = {@config}",
                "Generation for all batches completed",
                batches.Count, config))
            {
                foreach (var batch in batches)
                {
                    results.Add(await Generate(config, batch, ct));
                }
            }
            return new AllBatchesResult(results);
        }

        protected async Task<AllGenerationsResult> Generate(
            GeneratorConfig config,
            BatchSettings batchSettings,
            CancellationToken ct = default)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (batchSettings == null)
                throw new ArgumentNullException(nameof(batchSettings));

            try
            {
                config.Validate();
                batchSettings.Validate();
            }
            catch (ValidationException ve)
            {
                _logger.Fatal(ve, "Cannot start generation, configuration is invalid");
                throw;
            }

            if (batchSettings.RestartIisBeforeBatch)
            {
                try
                {
                    using (StopwatchLoggerFactory.ForceLogDispose(_logger, LogLevel.Debug, "iisreset completed"))
                    {
                        IisManager.Instance.RestartIis();
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Cannot restart IIS. Perhaps application was batched without administrator rights");
                }
            }

            if (batchSettings.CollectGarbageBeforeBatch)
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug("Memory before GC.Collect: {byte:N0}", GC.GetTotalMemory(false));
                    GC.Collect();
                    _logger.Debug("Memory after GC.Collect: {byte:N0}", GC.GetTotalMemory(true));
                }
                else
                    GC.Collect();
            }

            var generatationResults = new List<GenerationResult>();
            var settingsCollection = batchSettings.GetPreparedGenerationSettings().ToList();

            bool stopProcessing = false;
            using (StopwatchLoggerFactory.ForceLogStartDispose(
                _logger,
                "Start generation all settings for batch, " +
                "Count = {count}, Id = {id}",
                "Generation all settings for batch completed, " +
                "Count = {count}, Id = {id}",
                settingsCollection.Count, batchSettings.Id, batchSettings))
            {
                foreach (var settings in settingsCollection)
                {
                    GenerationResult result;
                    try
                    {
                        ct.ThrowIfCancellationRequested();

                        var runner = settings.GetGenerationRunner(config.ApiConnectionConfig);
                        config.SubscriptionManager?.SubscribeGenerationRunner(runner);
                        await runner.RunGeneration(ct).ConfigureAwait(false);
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
                        if (batchSettings.StopProcessingAtException)
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
                        result = new GenerationResult(settings, e);
                        if (batchSettings.StopProcessingAtException)
                            stopProcessing = true;
                    }
                    generatationResults.Add(result);
                    if (stopProcessing)
                        break;
                }
            }
            return new AllGenerationsResult(generatationResults, stopProcessing);
        }
    }
}