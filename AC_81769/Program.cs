using CrmDataGeneration;
using CrmDataGeneration.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AC_81769
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //CrmDataGeneration.OpenApi.SwaggerGenerator.GenerateClient("http://msk-ws-89.int.acumatica.com/r103/entity/Default/17.200.001/swagger.json");

            var config = GeneratorConfig.ReadConfigDefault();

            using (var generatorClient = new GeneratorClient(config))
            {
                try
                {
                    await generatorClient.Login();

                    await generatorClient.GenerateSingle<CrmDataGeneration.OpenApi.Reference.Lead>();

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
