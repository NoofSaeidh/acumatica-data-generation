using CrmDataGeneration.Common;
using CrmDataGeneration.Entities.Emails;
using CrmDataGeneration.Entities.Leads;
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
            if (Config.GenerationSettingsCollection == null)
                throw new InvalidOperationException($"{nameof(Config.GenerationSettingsCollection)} propety is not specified in {nameof(Config)}.");
            _logger.Info("Start generation for all settings.");
            foreach (var settings in Config.GenerationSettingsCollection)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await settings.GetGenerationRunner(Config.ApiConnectionConfig).RunGeneration(cancellationToken);
                }
                catch
                {
                    // logger in GenerationRunner
                    if (Config.StopProccesingOnExeception)
                        throw;
                }
            }
            _logger.Info("Generation all settings completed.");
        }
    }
}
