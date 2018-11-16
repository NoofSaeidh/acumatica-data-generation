using DataGeneration.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataGeneration.GenerationInfo
{
    public class GenerationSettingsInjection
    {
        public ExecutionTypeSettings? ExecutionTypeSettings { get; set; }
        public int? Count { get; set; }
        public int? Seed { get; set; }

        public IGenerationSettings Inject(IGenerationSettings generationSettings)
        {
            if (generationSettings == null)
                throw new ArgumentNullException(nameof(generationSettings));

            var res = generationSettings.Copy();

            if (Count.HasValue(out var count))
                res.Count = count;
            if (ExecutionTypeSettings.HasValue(out var execution))
                res.ExecutionTypeSettings = execution;
            if (Seed.HasValue(out var seed))
                res.Seed = seed;

            return res;
        }

        // it produces copy. original object not changed
        public IEnumerable<IGenerationSettings> Inject(IEnumerable<IGenerationSettings> generationSettings)
        {
            if (generationSettings == null)
                throw new ArgumentNullException(nameof(generationSettings));

            return generationSettings.Select(g => Inject(g));
        }

        public static IEnumerable<IGenerationSettings> Inject(
            IEnumerable<GenerationSettingsInjection> injections, 
            IEnumerable<IGenerationSettings> generationSettings)
        {
            if (injections == null)
                throw new ArgumentNullException(nameof(injections));
            if (generationSettings == null)
                throw new ArgumentNullException(nameof(generationSettings));

            return injections.SelectMany(i => i.Inject(generationSettings));
        }
    }
}