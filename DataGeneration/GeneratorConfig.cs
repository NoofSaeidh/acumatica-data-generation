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
        #region Fields

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
        private GenerationSubscriptionSettings _generationSubscriptionSettings;
        private GenerationSubscriptionManager _subscriptionManager;

        #endregion Fields

        [Required]
        public ApiConnectionConfig ApiConnectionConfig { get; set; }
        public ServicePointSettings ServicePointSettings { get; set; }
        public SettingsFilesConfig SettingsFiles { get; set; }
        public LaunchSettings NestedSettings { get; set; }
        public GenerationSubscriptionSettings SubscriptionSettings
        {
            get => _generationSubscriptionSettings;
            set
            {
                _generationSubscriptionSettings = value;
                _subscriptionManager = null;
            }
        }

        [JsonIgnore]
        public GenerationSubscriptionManager SubscriptionManager => _subscriptionManager 
            ?? (_subscriptionManager = SubscriptionSettings?.GetSubscriptionManager(this));

        public ICollection<LaunchSettings> GetAllLaunches(out int uniqueLaunchesCount)
        {
            Validate();
            uniqueLaunchesCount = 0;
            var result = new List<LaunchSettings>();
            if (NestedSettings != null)
            {
                uniqueLaunchesCount++;
                result.Add(NestedSettings);
            }
            if (SettingsFiles != null)
            {
                if (SettingsFiles.Multiplier.HasValue(out var multiplier))
                {
                    uniqueLaunchesCount += multiplier;
                }
                result.AddRange(SettingsFiles.GetAllLaunchSettings());
            }
            return result;
        }

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