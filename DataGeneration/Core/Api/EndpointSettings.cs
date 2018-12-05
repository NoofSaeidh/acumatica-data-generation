using Newtonsoft.Json;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace DataGeneration.Core.Api
{
    public class EndpointSettings
    {
        public const string EndpointPart = "entity";
        public const string LoginEndpointPart = "auth/login";
        public const string LogoutEndpointPart = "auth/logout";
        public const string MaintPart = "maintenance/5.31";
        public const string TelemetryMarkerPart = "testmarker?command=mark";

        [JsonConstructor]
        public EndpointSettings(string acumaticaBaseUrl, string endpointName, string endpointVersion)
        {
            if (acumaticaBaseUrl == null)
                throw new ArgumentNullException(nameof(acumaticaBaseUrl));
            AcumaticaBaseUrl = acumaticaBaseUrl.TrimEnd('/');
            EndpointName = endpointName ?? throw new ArgumentNullException(nameof(endpointName));
            EndpointVersion = endpointVersion ?? throw new ArgumentNullException(nameof(endpointVersion));

            EndpointUrl = new Uri(AcumaticaBaseUrl + $"/{EndpointPart}/{EndpointName}/{EndpointVersion}/");
            LoginUrl = new Uri(AcumaticaBaseUrl + $"/{EndpointPart}/{LoginEndpointPart}");
            LogoutUrl = new Uri(AcumaticaBaseUrl + $"/{EndpointPart}/{LogoutEndpointPart}");
            MaintananceUrl = new Uri(AcumaticaBaseUrl + $"/{EndpointPart}/{MaintPart}");
            TelemetryMarkerUrl = AcumaticaBaseUrl + $"/{TelemetryMarkerPart}";
        }

        public string AcumaticaBaseUrl { get; }
        public string EndpointName { get; }
        public string EndpointVersion { get; }
        public TimeSpan? Timeout { get; set; }

        [JsonIgnore]
        public Uri EndpointUrl { get; }

        [JsonIgnore]
        public Uri LoginUrl { get; } 

        [JsonIgnore]
        public Uri LogoutUrl { get; }

        [JsonIgnore]
        public Uri MaintananceUrl { get; }

        [JsonIgnore]
        public string TelemetryMarkerUrl { get; }

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
                        SendTimeout = Timeout ?? TimeSpan.FromMinutes(1),
                        CloseTimeout = Timeout ?? TimeSpan.FromMinutes(1),
                        OpenTimeout = Timeout ?? TimeSpan.FromMinutes(1),
                        ReceiveTimeout = Timeout ?? TimeSpan.FromMinutes(1),
                    };
                }
                case "https":
                {
                    return new BasicHttpsBinding
                    {
                        AllowCookies = true,
                        MaxReceivedMessageSize = int.MaxValue,
                        SendTimeout = Timeout ?? TimeSpan.FromMinutes(1),
                        CloseTimeout = Timeout ?? TimeSpan.FromMinutes(1),
                        OpenTimeout = Timeout ?? TimeSpan.FromMinutes(1),
                        ReceiveTimeout = Timeout ?? TimeSpan.FromMinutes(1),
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