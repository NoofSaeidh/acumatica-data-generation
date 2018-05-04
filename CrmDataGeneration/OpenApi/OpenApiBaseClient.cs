using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Formatting;
using Newtonsoft.Json;
using NLog;

namespace CrmDataGeneration.OpenApi
{
    public class OpenApiBaseClient
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

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
                logger.Info($"Log id. Url: {State.Settings.LoginUrl}.");
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
            }
            catch (Exception e)
            {
                logger.Error(e, $"Couldn't log out. Url: {State.Settings.LoginUrl}");
                throw;
            }
        }

        public async Task Logout()
        {
            try
            {
                logger.Info($"Log out. Url: {State.Settings.LoginUrl}.");
                var client = await CreateHttpClientAsync();
                var result = await client.PostAsync(State.Settings.LogoutUrl, new ByteArrayContent(new byte[0]));
            }
            catch (Exception e)
            {
                logger.Error(e, $"Couldn't log out.  Url: {State.Settings.LoginUrl}");
                throw;
            }
        }
    }
}
