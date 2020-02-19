using Bogus;
using DataGeneration.Core;
using DataGeneration.Core.Api;
using DataGeneration.Core.Extensions;
using DataGeneration.Core.Settings;
using DataGeneration.GenerationInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using static DataGeneration.Soap.SoapExtensions;


namespace DataGeneration.Scripts
{
    public class GenerateSalesDemoScriptBase
    {
        protected GeneratorConfig GetConfig(IGenerationSettings settings)
        {
            return new GeneratorConfig
            {
                ApiConnectionConfig = new ApiConnectionConfig
                (
                    new EndpointSettings("http://localhost:201", "datagen", "18.200.001"),
                    new LoginInfo { Username = "admin", Password = "123" }
                ),
                ServicePointSettings = new ServicePointSettings
                {
                    DefaultConnectionLimit = 6,
                },
                NestedSettings = new BatchSettings
                {
                    StopProcessingAtException = true,
                    GenerationSettings = new[] { settings },
                },
            };
        }
    }
}
