using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Formatting;
using Newtonsoft.Json;

namespace CrmDataGeneration.OpenApi
{
    public class OpenApiBaseClient
    {
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

        public async Task Logout()
        {
            var client = await CreateHttpClientAsync();
            var result = await client.PostAsync(State.Settings.LogoutUrl, new ByteArrayContent(new byte[0]));
        }
    }
}
