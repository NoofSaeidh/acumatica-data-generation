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

        #endregion Static fields

        [Required]
        public ApiConnectionConfig ApiConnectionConfig { get; set; }

        //[RequiredCollection(AllowEmpty = false)]
        public ICollection<GenerationLaunchSettings> GetAllLaunches()
        {
            throw new NotImplementedException();
        }

        public GenerationLaunchSettings NestedSettings { get; set; }

        #region Common methods

        public void SaveConfig(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, _jsonSettings));
        }

        //public void AddGenerationSettingsFromFile(string path)
        //{
        //    var settings = JsonConvert.DeserializeObject<IGenerationSettings>(File.ReadAllText(path), _jsonSettings);

        //    if (GenerationSettingsCollection == null)
        //        GenerationSettingsCollection = new List<IGenerationSettings>();
        //    GenerationSettingsCollection.Add(settings);
        //}

        public static GeneratorConfig ReadConfig(string path)
        {
            return JsonConvert.DeserializeObject<GeneratorConfig>(File.ReadAllText(path), _jsonSettings);
        }

        public void Validate()
        {
            ValidateHelper.ValidateObject(this);
        }

        #endregion Common methods

    }
}