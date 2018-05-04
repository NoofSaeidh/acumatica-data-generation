using CrmDataGeneration.Core;
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
            var config = GeneratorConfig.ReadConfigDefault();

            using (var generatorClient = new GeneratorClient(config))
            {
                try
                {
                    await generatorClient.Login();


                    await generatorClient.Logout();
                }
                catch(Exception e)
                {

                }
            }
        }
    }
}
