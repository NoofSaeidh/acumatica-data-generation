using DataGeneration.Common;
using NLog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace DataGeneration
{
    public class GeneratorClient
    {
        private static ILogger _logger => LogConfiguration.DefaultLogger;

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

        public async Task GenerateAllOptions(CancellationToken cancellationToken = default)
        {
            Config.Validate();
            _logger.Info("Start generation for all settings");
            foreach (var settings in Config.GetInjectedGenerationSettingsCollection())
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await settings.GetGenerationRunner(Config.ApiConnectionConfig).RunGeneration(cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException oce)
                {
                    _logger.Fatal(oce, "Operation was canceled.");
                    throw;
                }
                catch (GenerationException)
                {
                    if (Config.StopProccesingAtExeception)
                        return;
                }
                catch (ValidationException ve)
                {
                    _logger.Error(ve, "Generation not started because of invalid configuration");
                }
                catch (Exception e)
                {
                    _logger.Fatal(e, "Generation failed with unexpected error");
                    return;
                }
            }
            _logger.Info("Generation all settings completed");
        }
    }
}