﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CrmDataGeneration.Common;
using CrmDataGeneration.Entities.Leads;
using Newtonsoft.Json.Converters;

namespace CrmDataGeneration
{
    public class GeneratorConfig : IValidatable
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
        public ApiConnectionConfig ApiConnectionConfig { get; set; }
        [RequiredCollection]
        public ICollection<IGenerationSettings> GenerationSettingsCollection { get; set; }
        // if true processing will be stopped if any generation option will fail
        public bool StopProccesingOnExeception { get; set; }
        // provide ability to run EACH Generations Settings with different Execution Settings
        public ICollection<ExecutionTypeSettings> InjectExecutionSettings { get; set; }

        // inject all injected props to GenerationSettingsCollection (now only InjectExecutionSettings)
        // work only if items in GenerationSettingsCollection inherited from GenerationSettingsBase
        public IEnumerable<IGenerationSettings> GetInjectedGenerationSettingsCollection()
        {
            if (InjectExecutionSettings.IsNullOrEmpty())
                return GenerationSettingsCollection.ToList();

            return GenerationSettingsCollection
                ?.SelectMany(s =>
                    InjectExecutionSettings
                    .Select(es =>
                    {
                        if (s is GenerationSettingsBase gsb)
                        {
                            var res = gsb.Copy();
                            res.ExecutionTypeSettings = es;
                            return res;
                        }
                        return s;
                    })
                );
        }

        #region Common methods

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

        public void Validate()
        {
            ValidateHelper.ValidateObject(this);
        }

        #endregion
    }
}
