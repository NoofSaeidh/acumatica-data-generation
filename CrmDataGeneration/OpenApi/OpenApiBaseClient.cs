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
                var result = await client.PostAsJsonAsync(State.Settings.LoginUrl, new
                {
                    name = State.Settings.Username,
                    password = State.Settings.Password,
                    company = State.Settings.Company,
                    branch = State.Settings.Branch,
                    locale = State.Settings.Locale
                });
                result.EnsureSuccessStatusCode();
                _logger.Info("Log id to Acumatica REST. {url} {@settings}.", State.Settings.LoginUrl, State.Settings);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Couldn't log in to Acumatica REST. {url} {@settings}.", State.Settings.LoginUrl, State.Settings);
                throw;
            }
        }

        public async Task Logout()
        {
            try
            {
                var client = await CreateHttpClientAsync();
                var result = await client.PostAsync(State.Settings.LogoutUrl, new ByteArrayContent(new byte[0]));
                _logger.Info("Log out from Acumatica REST. {url}.", State.Settings.LogoutUrl);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Couldn't log out to Acumatica REST. {url}.", State.Settings.LogoutUrl);
                throw;
            }
        }
    }
}
