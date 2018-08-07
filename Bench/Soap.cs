using BenchmarkDotNet.Attributes;
using CrmDataGeneration;
using CrmDataGeneration.Common;
using CrmDataGeneration.Entities.Leads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bench
{
    public class Soap
    {
        private GeneratorConfig _generatorConfig;


        [Params(100, 500, 1000, 2000)]
        public int Count;
        [Params(0, 1, 2, 3, 4, 8, 12)]
        public int Threads;

        [GlobalSetup]
        public void Setup()
        {
            _generatorConfig = GeneratorConfig.ReadConfigDefault();
        }

        [IterationSetup]
        public void IterationSetup()
        {
            var settings = _generatorConfig.GenerationSettingsCollection.First() as LeadGenerationSettings;
            settings.Count = Count;
            settings.ExecutionTypeSettings = new ExecutionTypeSettings(
                Threads == 0 ? ExecutionType.Sequent : ExecutionType.Parallel,
                false,
                Threads);
        }

        [Benchmark]
        public void Generate()
        {
            var generator = new GeneratorClient(_generatorConfig);
            generator.GenerateAllOptions().Wait();
        }
    }
}
