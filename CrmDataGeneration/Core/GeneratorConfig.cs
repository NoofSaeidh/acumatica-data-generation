using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrmDataGeneration.OpenApi;
using Newtonsoft.Json;

namespace CrmDataGeneration.Core
{
    public class GeneratorConfig
    {
        public const string ConfigFileName = "config.json";
        public const string ConfigCredsFileName = "config.creds.json";

        public OpenApiSettings OpenApiSettings { get; set; }

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
