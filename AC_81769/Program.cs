using CrmDataGeneration;

namespace AC_81769
{
    class Program
    {
        static void Main()
        {
            new GeneratorClient(GeneratorConfig.ReadConfig("config.json")).GenerateAllOptions().Wait();
        }
    }
}

