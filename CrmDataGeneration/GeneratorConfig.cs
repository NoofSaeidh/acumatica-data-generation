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
using Newtonsoft.Json.Converters;

namespace CrmDataGeneration
{
    public class GeneratorConfig
    {
        #region Static fields

        public const string ConfigFileName = "config.json";
        public const string ConfigCredsFileName = "config.creds.json";

        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            TypeNameHandling = TypeNameHandling.Auto,
            Converters = new JsonConverter[]
            {
                new StringEnumConverter(),
                new ValueTupleJsonConverter()
            }
        };

        #endregion

        public int GlobalSeed { get; set; }
        public OpenApiSettings OpenApiSettings { get; set; }
        public ICollection<GenerationOption> GenerationOptions { get; set; }

        #region Common methods

        //public GenerationOption<T> GetGenerationOption<T>() where T : Entity
        //{
        //    return GenerationOptions.OfType<GenerationOption<T>>().First();
        //}

        public void SaveConfig(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, _jsonSettings));
        }

        public static GeneratorConfig ReadConfig(string path)
        {
            return JsonConvert.DeserializeObject<GeneratorConfig>(File.ReadAllText(path), _jsonSettings);
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

            JsonConvert.PopulateObject(File.ReadAllText(ConfigCredsFileName), config, _jsonSettings);
            return config;
        }

        #endregion
    }
}
