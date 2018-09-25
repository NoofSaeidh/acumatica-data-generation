using DataGeneration.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataGeneration
{
    public class GeneratorConfig : IValidatable
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

        public ApiConnectionConfig ApiConnectionConfig { get; set; }

        [RequiredCollection]
        public ICollection<IGenerationSettings> GenerationSettingsCollection { get; set; }

        // provide ability to multiply GenerationSettings for each injection
        public ICollection<Injection> GenerationSettingsInjections { get; set; }

        // if true processing will be stopped if any generation option will fail
        public bool StopProccesingAtExeception { get; set; }

        // inject all injected props to GenerationSettingsCollection (now only InjectExecutionSettings)
        // work only if items in GenerationSettingsCollection inherited from GenerationSettingsBase
        public IEnumerable<IGenerationSettings> GetInjectedGenerationSettingsCollection()
        {
            if (GenerationSettingsCollection.IsNullOrEmpty())
                return GenerationSettingsCollection;

            if (GenerationSettingsInjections.IsNullOrEmpty())
                return GenerationSettingsCollection.ToList();

            return GenerationSettingsCollection.SelectMany(s => Injection.Inject(GenerationSettingsInjections, s));
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

        public void Validate()
        {
            ValidateHelper.ValidateObject(this);
        }

        public class Injection
        {
            public ExecutionTypeSettings? ExecutionTypeSettings { get; set; }
            public int? Count { get; set; }
            public int? Seed { get; set; }

            public static IEnumerable<IGenerationSettings> Inject(IEnumerable<Injection> injections, IGenerationSettings generationSettings)
            {
                if (injections == null)
                    throw new ArgumentNullException(nameof(injections));
                if (generationSettings == null)
                    throw new ArgumentNullException(nameof(generationSettings));

                // check if there are no injections to return single setting
                bool empty = false;

                foreach (var injection in injections)
                {
                    if (injection.Count == 0
                        && injection.ExecutionTypeSettings == null
                        && injection.Seed == null)
                    {
                        empty = true;
                        continue;
                    }

                    var res = generationSettings.Copy();
                    if (injection.Count != null)
                        res.Count = injection.Count.Value;
                    if (injection.ExecutionTypeSettings != null)
                        res.ExecutionTypeSettings = injection.ExecutionTypeSettings.Value;
                    if (injection.Seed != null)
                        res.Seed = injection.Seed;

                    yield return res;
                }

                if (empty)
                    yield return generationSettings;
            }
        }

        #endregion Common methods
    }
}