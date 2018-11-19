using DataGeneration.Common;
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
        public ICollection<JsonInjection<IGenerationSettings>> SettingsInjections { get; set; }
        // multuplies launch and apply to settings
        public ICollection<ICollection<JsonInjection<LaunchSettings>>> LaunchMultiplier { get; set; }

        public IEnumerable<LaunchSettings> GetAllLaunchSettings()
        {
            Validate();
            var settings = Files
                .Select(f =>
                {
                    var s = JsonConvert.DeserializeObject<IGenerationSettings>(File.ReadAllText(f), GeneratorConfig.ConfigJsonSettings);
                    if (SettingsInjections != null)
                    {
                        JsonInjection.Inject(s, SettingsInjections);
                    }
                    return s;
                }).ToList();
            var launch = new LaunchSettings
            {
                GenerationSettings = settings
            };

            if (LaunchMultiplier != null)
            {
                foreach (var item in LaunchMultiplier)
                {
                    if (item == null || item.Count == 0)
                        continue;
                    var copy = launch.Copy();
                    JsonInjection.Inject(copy, item);
                    yield return copy;
                }
            }
            else
                yield return launch;
        }

        public void Validate()
        {
            ValidateHelper.ValidateObject(this);
        }
    }
}
