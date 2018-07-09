using CrmDataGeneration.Common;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace CrmDataGeneration.OpenApi
{
    public class OpenApiState : IDisposable
    {
        private HttpClient _httpClient;

        public OpenApiState(ApiSessionConfig settings)
        {
            SessionConfig = settings ?? throw new ArgumentNullException(nameof(ApiSessionConfig));
        }

        public ApiSessionConfig SessionConfig { get; }

        public HttpClient HttpClient
        {
            get
            {
                if (_httpClient != null) return _httpClient;

                return _httpClient = new HttpClient(new HttpClientHandler
                {
                    UseCookies = true,
                    CookieContainer = new CookieContainer(),
                })
                {
                    BaseAddress = SessionConfig.EndpointSettings.EndpointUrl,
                    DefaultRequestHeaders =
                    {
                        Accept = {MediaTypeWithQualityHeaderValue.Parse("text/json")}
                    }
                };
            }
        }

        void IDisposable.Dispose()
        {
            HttpClient?.Dispose();
        }
    }
}
