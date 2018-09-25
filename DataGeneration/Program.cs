namespace DataGeneration
{
    class Program
    {
        static void Main()
        {
            new GeneratorClient(GeneratorConfig.ReadConfig("config.json")).GenerateAllOptions().Wait();
        }
    }
}

