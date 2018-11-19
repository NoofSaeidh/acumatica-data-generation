using DataGeneration.Common;
using DataGeneration.GenerationInfo;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace DataGeneration
{
    public partial class GeneratorConfig : IValidatable
    {
        #region Static fields

        internal static readonly JsonSerializerSettings ConfigJsonSettings = new JsonSerializerSettings
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

        #endregion Static fields

        [Required]
        public ApiConnectionConfig ApiConnectionConfig { get; set; }

        // hack: set it not much bigger that count of used threads
        public int? DefaultConnectionLimit { get; set; }

        public ICollection<LaunchSettings> GetAllLaunches()
        {
            Validate();
            var result = new List<LaunchSettings>();
            if (NestedSettings != null)
                result.Add(NestedSettings);
            if (SettingsFiles != null)
                result.AddRange(SettingsFiles.GetAllLaunchSettings());
            return result;
        }

        public SettingsFilesConfig SettingsFiles { get; set; }
        public LaunchSettings NestedSettings { get; set; }

        #region Common methods

        public void SaveConfig(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, ConfigJsonSettings));
        }

        public static GeneratorConfig ReadConfig(string path)
        {
            return JsonConvert.DeserializeObject<GeneratorConfig>(File.ReadAllText(path), ConfigJsonSettings);
        }

        public void Validate()
        {
            ValidateHelper.ValidateObject(this);
            if (SettingsFiles == null && NestedSettings == null)
                throw new ValidationException($"One of properties must be specified: " +
                    $"{nameof(SettingsFiles)}, {nameof(NestedSettings)}.");
        }

        #endregion Common methods

    }
}