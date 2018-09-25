using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.Common
{
    public class ApiConnectionConfig
    {
        [JsonConstructor]
        public ApiConnectionConfig(EndpointSettings endpointSettings, LoginInfo loginInfo)
        {
            EndpointSettings = endpointSettings;
            LoginInfo = loginInfo;
        }

        public EndpointSettings EndpointSettings { get; }
        public LoginInfo LoginInfo { get; }
    }
}
