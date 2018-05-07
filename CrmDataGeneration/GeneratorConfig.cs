using CrmDataGeneration.OpenApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CrmDataGeneration.OpenApi.Reference;
using CrmDataGeneration.Common;
using CrmDataGeneration.Generation.Leads;

namespace CrmDataGeneration
{
    public class GeneratorConfig
    {
        public const string ConfigFileName = "config.json";
        public const string ConfigCredsFileName = "config.creds.json";

        public int GlobalSeed { get; set; }
        public OpenApiSettings OpenApiSettings { get; set; }
        public LeadRandomizerSettings LeadRandomizerSettings { get; set; }

        // add all your settings here.
        // it required for generic support methods in GeneratorClient
        public IRandomizerSettings<T> GetRandomizerSettings<T>() where T : Entity
        {
            switch (typeof(T).Name)
            {
                // typeof cannot be used in switch clause
                case nameof(Lead):
                    return (IRandomizerSettings<T>)LeadRandomizerSettings;
                default:
                    throw new NotSupportedException($"This type of generator is not supported. Type: {typeof(T).Name}");
            }
        }

        public void SaveConfig(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static GeneratorConfig ReadConfig(string path)
        {
            return JsonConvert.DeserializeObject<GeneratorConfig>(File.ReadAllText(path));
        }

        /// <summary>
        ///     Read default config files from current folder.
        /// Read both files, <see cref="ConfigFileName"/> and user's <see cref="ConfigCredsFileName"/>.
        /// First is added to source control while second is not, so you can change second file from the solution.
        /// Second file by default doesn't exist, but you can create it (it will be automatically added to solution).
        /// </summary>
        /// <returns></returns>
        public static GeneratorConfig ReadConfigDefault()
        {
            var config = ReadConfig(ConfigFileName);
            if (!File.Exists(ConfigCredsFileName))
                return config;

            JsonConvert.PopulateObject(File.ReadAllText(ConfigCredsFileName), config);
            return config;
        }
    }
}
