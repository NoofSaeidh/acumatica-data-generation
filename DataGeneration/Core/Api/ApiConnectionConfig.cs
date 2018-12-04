using System;
using Newtonsoft.Json;

namespace DataGeneration.Core.Api
{
    public class ApiConnectionConfig
    {
        [JsonConstructor]
        public ApiConnectionConfig(EndpointSettings endpointSettings, LoginInfo loginInfo)
        {
            EndpointSettings = endpointSettings ?? throw new ArgumentNullException(nameof(endpointSettings));
            LoginInfo = loginInfo ?? throw new ArgumentNullException(nameof(loginInfo));
        }

        public EndpointSettings EndpointSettings { get; }
        public LoginInfo LoginInfo { get; }
    }
}