using Newtonsoft.Json;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace DataGeneration.Common
{
    public class EndpointSettings
    {
        public const string EndpointPart = "entity";
        public const string LoginEndpointPart = "auth/login";
        public const string LogoutEndpointPart = "auth/logout";
        public const string MaintPart = "maintenance/5.31";

        [JsonConstructor]
        public EndpointSettings(string acumaticaBaseUrl, string endpointName, string endpointVersion)
        {
            AcumaticaBaseUrl = acumaticaBaseUrl ?? throw new ArgumentNullException(nameof(acumaticaBaseUrl));
            EndpointName = endpointName ?? throw new ArgumentNullException(nameof(endpointName));
            EndpointVersion = endpointVersion ?? throw new ArgumentNullException(nameof(endpointVersion));
        }

        public string AcumaticaBaseUrl { get; }
        public string EndpointName { get; }
        public string EndpointVersion { get; }

        [JsonIgnore]
        public Uri EndpointUrl => new Uri(AcumaticaBaseUrl.TrimEnd('/') + $"/{EndpointPart}/{EndpointName}/{EndpointVersion}/");

        [JsonIgnore]
        public Uri LoginUrl => new Uri(AcumaticaBaseUrl.TrimEnd('/') + $"/{EndpointPart}/{LoginEndpointPart}");

        [JsonIgnore]
        public Uri LogoutUrl => new Uri(AcumaticaBaseUrl.TrimEnd('/') + $"/{EndpointPart}/{LoginEndpointPart}");

        [JsonIgnore]
        public Uri MaintananceUrl => new Uri(AcumaticaBaseUrl.TrimEnd('/') + $"/{EndpointPart}/{MaintPart}");


        public Binding GetBinding()
        {
            switch (EndpointUrl.Scheme)
            {
                case "http":
                {
                    return new BasicHttpBinding
                    {
                        AllowCookies = true,
                        MaxReceivedMessageSize = int.MaxValue,
                        SendTimeout = TimeSpan.FromHours(1)
                    };
                }
                case "https":
                {
                    return new BasicHttpsBinding
                    {
                        AllowCookies = true,
                        MaxReceivedMessageSize = int.MaxValue,
                        SendTimeout = TimeSpan.FromHours(1)
                    };
                }
                default:
                    throw new InvalidOperationException();
            }
        }

        public EndpointAddress GetEndpointAddress()
        {
            return new EndpointAddress(EndpointUrl);
        }

        public EndpointAddress GetMaintenanceEndpointAddress()
        {
            return new EndpointAddress(MaintananceUrl);
        }
    }
}