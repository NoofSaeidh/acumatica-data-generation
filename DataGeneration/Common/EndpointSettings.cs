using Newtonsoft.Json;
using System;

namespace DataGeneration.Common
{
    public class EndpointSettings
    {
        public const string OpenApiEnpointPart = "entity";
        public const string LoginEndpointPart = "auth/login";
        public const string LogoutEndpointPart = "auth/logout";

        public string AcumaticaBaseUrl { get; set; }
        public string EndpointName { get; set; }
        public string EndpointVersion { get; set; }

        [JsonIgnore]
        public Uri EndpointUrl => new Uri(AcumaticaBaseUrl.TrimEnd('/') + $"/{OpenApiEnpointPart}/{EndpointName}/{EndpointVersion}/");

        [JsonIgnore]
        public Uri LoginUrl => new Uri(AcumaticaBaseUrl.TrimEnd('/') + $"/{OpenApiEnpointPart}/{LoginEndpointPart}");

        [JsonIgnore]
        public Uri LogoutUrl => new Uri(AcumaticaBaseUrl.TrimEnd('/') + $"/{OpenApiEnpointPart}/{LoginEndpointPart}");
    }
}