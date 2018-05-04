//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using CrmDataGeneration.Core;
//using CrmDataGeneration.OpenApi;
//using NLog;

//namespace CrmDataGeneration
//{
//    // just an example!

//    class Program
//    {
//        // if endpoint changed you may need to regenerate client
//        // SwaggerGenerator.GenerateClient("http://localhost/dbdata/(W(20))/entity/Default/17.200.001/swagger.json");

//        // you may initialize it like that:
//        //var settings = new OpenApiSettings
//        //{
//        //    AcumaticaBaseUrl = "http://localhost/dbdata",
//        //    Username = "admin",
//        //    Password = "123",
//        //    EndpointName = "default",
//        //    EndpointVersion = "17.200.001"
//        //};
//        //var config = new GeneratorConfig { OpenApiSettings = settings };

//        static async Task Main(string[] args)
//        {
//            // default settings liked to Solution as:
//            // * config.json - default config, added to source control
//            // * config.creds.json - user's config removed from source control,
//            // you may add any properties of config.json to here for your environment
//            var config = GeneratorConfig.ReadConfigDefault();

//            using (var generatorClient = new GeneratorClient(config))
//            {
//                await generatorClient.Login();

//                //var baClient = new OpenApi.Reference.BusinessAccountClient(settings);
//                //var b = baClient.GetListAsync().Result;

//                var leadClient = generatorClient.GetApiClient<OpenApi.Reference.LeadClient>();
//                var result = await leadClient.GetListAsync();

//                var lead = new OpenApi.Reference.Lead
//                {
//                    FirstName = "Xxxxx",
//                    LastName = "Lead",
//                    CompanyName = "Xxxxx",
//                    Email = "ahah@no.com",
//                    LeadClass = "LEADBUS"
//                };

//                lead = await leadClient.PutEntityAsync(lead);

//                await generatorClient.Logout();

//            }
//        }
//    }
//}
