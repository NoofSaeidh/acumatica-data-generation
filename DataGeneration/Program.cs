namespace DataGeneration
{
    internal class Program
    {
        private static void Main()
        {
            new GeneratorClient(GeneratorConfig.ReadConfig("config.json")).GenerateAllOptions().Wait();
        }
    }
}