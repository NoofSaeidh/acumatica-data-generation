using CrmDataGeneration.Common;
using CrmDataGeneration.Entities.Emails;
using CrmDataGeneration.Entities.Leads;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrmDataGeneration
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
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await settings.GetGenerationRunner(Config.ApiConnectionConfig).RunGeneration(cancellationToken).ConfigureAwait(false);
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
