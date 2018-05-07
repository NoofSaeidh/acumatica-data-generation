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

            var config = GeneratorConfig.ReadConfigDefault();
            config.LeadRandomizerSettings = new LeadGenerationSettings
            {
                Count = 10,
                GenerateInParallel = true,
                MaxExecutionThreads = 5,
            };
            using (var generatorClient = new GeneratorClient(config))
            {
                try
                {
                    await generatorClient.Login();

                    var sw = new Stopwatch();
                    sw.Start();

                    config.LeadRandomizerSettings.GenerateInParallel = false;
                    await generatorClient.GenerateAll<Lead>();

                    sw.Stop();
                    LogConfiguration.DefaultLogger.Warn("Sequent execution took {time}", sw.Elapsed);
                    sw.Restart();

                    config.LeadRandomizerSettings.GenerateInParallel = true;
                    await generatorClient.GenerateAll<Lead>();

                    sw.Stop();
                    LogConfiguration.DefaultLogger.Warn("Parallel execution took {time}", sw.Elapsed);

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
