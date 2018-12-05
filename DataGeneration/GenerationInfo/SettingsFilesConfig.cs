using DataGeneration.Core;
using DataGeneration.Core.Common;
using DataGeneration.Core.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.GenerationInfo
{
    public class SettingsFilesConfig : IValidatable
    {
        [Required]
        public ICollection<string> Files { get; set; }
        public ICollection<JsonInjection<LaunchSettings>> LaunchInjections { get; set; }
        public ICollection<JsonInjection<IGenerationSettings>> SettingsInjections { get; set; }
        // multuplies launches
        public int? Multiplier { get; set; }

        public IEnumerable<LaunchSettings> GetAllLaunchSettings()
        {
            Validate();
            var settings = Files
                .Select(f => JsonConvert.DeserializeObject<IGenerationSettings>(
                                    File.ReadAllText(f),
                                    GeneratorConfig.ConfigJsonSettings))
                .ToList();

            var launch = new LaunchSettings
            {
                GenerationSettings = settings,
                Injections = SettingsInjections
            };

            if (LaunchInjections != null)
                JsonInjection.Inject(launch, LaunchInjections);

            if (Multiplier.HasValue(out var multipl))
            {
                if (multipl <= 0)
                    throw new InvalidOperationException($"{nameof(Multiplier)} must be null or positive integer.");
                // copy method ignores Id field, so need to adjust this
                return Enumerable
                    .Range(0, multipl)
                    .Select(i =>
                    {
                        var newLaunch = launch.Copy();
                        newLaunch.Id = i++;
                        return newLaunch;
                    });
            }
            else
                return launch.AsArray();
        }

        public void Validate()
        {
            ValidateHelper.ValidateObject(this);
        }
    }
}
