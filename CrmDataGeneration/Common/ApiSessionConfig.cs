using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmDataGeneration.Common
{
    public class ApiSessionConfig
    {
        [JsonConstructor]
        public ApiSessionConfig(EndpointSettings endpointSettings, LoginInfo loginInfo)
        {
            EndpointSettings = endpointSettings;
            LoginInfo = loginInfo;
        }

        public EndpointSettings EndpointSettings { get; }
        public LoginInfo LoginInfo { get; }
    }
}
