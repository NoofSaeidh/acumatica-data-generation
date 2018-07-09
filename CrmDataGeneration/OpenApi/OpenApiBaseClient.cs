using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Formatting;
using Newtonsoft.Json;
using NLog;
using CrmDataGeneration.Common;

namespace CrmDataGeneration.OpenApi
{
    public class OpenApiBaseClient
    {
        private static ILogger _logger => LogConfiguration.DefaultLogger;

        public OpenApiBaseClient(OpenApiState state)
        {
            State = state ?? throw new ArgumentNullException(nameof(state));
        }

        protected OpenApiState State { get; }

        // signature for generator
        protected Task<HttpClient> CreateHttpClientAsync(CancellationToken? cancellationToken = null)
        {
            return Task.FromResult(State.HttpClient);
        }

        public async Task Login()
        {
            try
            {
                var client = await CreateHttpClientAsync();
                var result = await client.PostAsJsonAsync(State.SessionConfig.EndpointSettings.LoginUrl, new
                {
                    name = State.SessionConfig.LoginInfo.Username,
                    password = State.SessionConfig.LoginInfo.Password,
                    company = State.SessionConfig.LoginInfo.Company,
                    branch = State.SessionConfig.LoginInfo.Branch,
                    locale = State.SessionConfig.LoginInfo.Locale
                });
                result.EnsureSuccessStatusCode();
                _logger.Info("Log id to Acumatica REST. {url} {@settings}.", State.SessionConfig.EndpointSettings.LoginUrl, State.SessionConfig);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Couldn't log in to Acumatica REST. {url} {@settings}.", State.SessionConfig.EndpointSettings.LoginUrl, State.SessionConfig);
                throw;
            }
        }

        public async Task Logout()
        {
            try
            {
                var client = await CreateHttpClientAsync();
                var result = await client.PostAsync(State.SessionConfig.EndpointSettings.LogoutUrl, new ByteArrayContent(new byte[0]));
                _logger.Info("Log out from Acumatica REST. {url}.", State.SessionConfig.EndpointSettings.LogoutUrl);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Couldn't log out to Acumatica REST. {url}.", State.SessionConfig.EndpointSettings.LogoutUrl);
                throw;
            }
        }
    }
}
