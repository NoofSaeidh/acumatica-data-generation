using Newtonsoft.Json;

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