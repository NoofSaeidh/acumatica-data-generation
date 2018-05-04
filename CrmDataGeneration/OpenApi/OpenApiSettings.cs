using Newtonsoft.Json;
using System;

namespace CrmDataGeneration.OpenApi
{
    public class OpenApiSettings
    {
        public const string OpenApiEnpointPart = "entity";
        public const string LoginEndpointPart = "auth/login";
        public const string LogoutEndpointPart = "auth/logout";

        public string AcumaticaBaseUrl { get; set; }
        public string EndpointName { get; set; }
        public string EndpointVersion { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Branch { get; set; }
        public string Locale { get; set; }
        public string Company { get; set; }

        [JsonIgnore]
        public Uri EndpointUrl => new Uri(AcumaticaBaseUrl.TrimEnd('/') + $"/{OpenApiEnpointPart}/{EndpointName}/{EndpointVersion}/");
        [JsonIgnore]
        public Uri LoginUrl => new Uri(AcumaticaBaseUrl.TrimEnd('/') + $"/{OpenApiEnpointPart}/{LoginEndpointPart}");
        [JsonIgnore]
        public Uri LogoutUrl => new Uri(AcumaticaBaseUrl.TrimEnd('/') + $"/{OpenApiEnpointPart}/{LoginEndpointPart}");
    }
}
