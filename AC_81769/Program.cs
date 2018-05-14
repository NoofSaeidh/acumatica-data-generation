using CrmDataGeneration;
using CrmDataGeneration.Common;
using CrmDataGeneration.Entities.Leads;
using CrmDataGeneration.OpenApi.Reference;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            catch (Exception e)
            {
                throw;
            }
            using (var generatorClient = new GeneratorClient(config))
            {
                try
                {
                    await generatorClient.Login();

                    var lc = generatorClient.GetRawApiClient<LeadClient>();
                    var ac = generatorClient.GetRawApiClient<InvokeActionClient>();
                    var ec = generatorClient.GetRawApiClient<EmailClient>();
                    var lead = (await lc.GetListAsync()).First();
                    var newEmail = new Email
                    {
                        Incoming = true,
                        From = lead.Email,
                        To = "acumatica@acumatica.com",
                        Parent = lead.Id,
                        Subject = "hihihi",
                        Description = "uhuhu"
                    };
                    var resEmail = await ec.PutEntityAsync(newEmail);
                    var invocation = new SelectRelatedEntityEmail
                    {
                        Entity = resEmail,
                        Parameters = new Parameters4
                        {
                            RelatedEntity = lead.LeadDisplayName,
                            Type = "PX.Objects.CR.Contact"
                        }
                    };
                    await ac.SelectRelatedEntityEmailAsync(invocation);
                    await generatorClient.GenerateAllOptions();

                    await generatorClient.Logout();
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }
        static async VoidTask LogActionTime(string actionName, VoidTask action)
        {
            using (var sw = new StreamWriter("log_common\\main.log"))
            {
                var watch = new Stopwatch();
                watch.Start();
                await action;
                watch.Stop();
                sw.WriteLine(actionName + " Time Elapsed: " + watch.Elapsed);
            }
        }
    }
}

