using CrmDataGeneration;
using CrmDataGeneration.Common;
using CrmDataGeneration.Generation.Leads;
using CrmDataGeneration.OpenApi.Reference;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using VoidTask = System.Threading.Tasks.Task; // Task in generated Api Client also exists

namespace AC_81769
{
    class Program
    {
        static async VoidTask Main(string[] args)
        {
            //CrmDataGeneration.OpenApi.SwaggerGenerator.GenerateClient("http://msk-ws-89.int.acumatica.com/r103/entity/Default/17.200.001/swagger.json");
            GeneratorConfig config;
            try
            {
                config = GeneratorConfig.ReadConfigDefault();
                //config.SaveConfig(GeneratorConfig.ConfigCredsFileName);
            }
            catch(Exception e)
            {
                throw;
            }

            //var leadOption = new LeadGenerationOption
            //{
            //    Count = 10,
            //    GenerateInParallel = true,
            //    MaxExecutionThreadsParallel = 10,
            //    RandomizerSettings = new LeadRandomizerSettings
            //    {

            //    }
            //};

            //config.GenerationOptions = new List<GenerationOption> { leadOption };
            //config.SaveConfig(GeneratorConfig.ConfigCredsFileName);

            using (var generatorClient = new GeneratorClient(config))
            {
                try
                {
                    await generatorClient.Login();

                    await generatorClient.GenerateAllOptions();

                    await generatorClient.Logout();
                }
                catch(Exception e)
                {
                    throw;
                }
            }
        }
    }
}
