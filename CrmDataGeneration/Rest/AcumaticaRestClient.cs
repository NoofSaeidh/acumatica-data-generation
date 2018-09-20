using CrmDataGeneration.Common;
using CrmDataGeneration.Soap;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VoidTask = System.Threading.Tasks.Task;


namespace CrmDataGeneration.Rest
{
    [Obsolete("Not fully implemented.")]
    public class AcumaticaRestClient : IApiClient
    {
        private static ILogger _logger => LogConfiguration.DefaultLogger;

        private readonly HttpClient _httpClient;

        private readonly ApiConnectionConfig _apiConnectionConfig;

        private readonly string _endpointAddress;

        public AcumaticaRestClient(ApiConnectionConfig apiConnectionConfig)
        {
            _apiConnectionConfig = apiConnectionConfig ?? throw new ArgumentNullException(nameof(apiConnectionConfig));
            _httpClient = new HttpClient(
            new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = new CookieContainer()
            })
            {
                BaseAddress = apiConnectionConfig.EndpointSettings.EndpointUrl,
                DefaultRequestHeaders =
                {
                    Accept = { MediaTypeWithQualityHeaderValue.Parse("text/json") }
                }
            };
            _endpointAddress = _apiConnectionConfig.EndpointSettings.EndpointUrl.ToString();
        }

        public static async Task<ILoginLogoutApiClient> GetLoginLogoutClientAsync(ApiConnectionConfig apiConnectionConfig)
        {
            if (apiConnectionConfig == null)
                throw new ArgumentNullException(nameof(apiConnectionConfig));

            var client = new LogoutClientImpl(apiConnectionConfig.EndpointSettings);
            await client.LoginAsync(apiConnectionConfig.LoginInfo);
            return client;
        }


        public void Delete<T>(T entity) where T : Entity
        {
            throw new NotImplementedException();
        }

        public VoidTask DeleteAsync<T>(T entity) where T : Entity
        {
            throw new NotImplementedException();
        }

        public VoidTask DeleteAsync<T>(T entity, CancellationToken cancellationToken) where T : Entity
        {
            throw new NotImplementedException();
        }

        public T Get<T>(T whereEntity) where T : Entity
        {
            throw new NotImplementedException();
        }

        public Task<T> GetAsync<T>(T whereEntity) where T : Entity
        {
            throw new NotImplementedException();
        }

        public Task<T> GetAsync<T>(T whereEntity, CancellationToken cancellationToken) where T : Entity
        {
            throw new NotImplementedException();
        }

        public IList<T> GetList<T>(T whereEntity) where T : Entity
        {
            throw new NotImplementedException();
        }

        public Task<IList<T>> GetListAsync<T>(T whereEntity) where T : Entity
        {
            throw new NotImplementedException();
        }

        public Task<IList<T>> GetListAsync<T>(T whereEntity, CancellationToken cancellationToken) where T : Entity
        {
            throw new NotImplementedException();
        }

        public void Invoke<TEntity, TAction>(TEntity entity, TAction action)
            where TEntity : Entity
            where TAction : Soap.Action
        {
            InvokeAsync(entity, action).Wait();
        }

        public VoidTask InvokeAsync<TEntity, TAction>(TEntity entity, TAction action)
            where TEntity : Entity
            where TAction : Soap.Action
        {
            return InvokeAsync(entity, action, default);
        }

        public async VoidTask InvokeAsync<TEntity, TAction>(TEntity entity, TAction action, CancellationToken cancellationToken)
            where TEntity : Entity
            where TAction : Soap.Action
        {
            await TryCatchAsync("Invoke", async () =>
            {
                var result = await _httpClient.PostAsync(_endpointAddress + '/' + typeof(TEntity).Name + '/' + typeof(TAction).Name,
                    ToStringContent(new { Entity = entity, Parameters = action }));

                result.EnsureSuccessStatusCode();

                var dt = DateTime.Now;
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    switch (result.StatusCode)
                    {
                        case HttpStatusCode.NoContent:
                            return "No content";
                        case HttpStatusCode.Accepted:
                            if ((DateTime.Now - dt).Seconds > 30)
                                throw new TimeoutException();
                            Thread.Sleep(500);
                            result = await _httpClient.GetAsync(result.Headers.Location);
                            result.EnsureSuccessStatusCode();
                            continue;
                        default:
                            throw new InvalidOperationException(
                              "Invalid process result: " + result.StatusCode);
                    }
                }
            });
        }

        public void Login(LoginInfo loginInfo)
        {
            LoginAsync(loginInfo).Wait();
        }

        public void Login(string name, string password, string company = null, string branch = null, string locale = null)
        {
            LoginAsync(name, password, company, branch, locale).Wait();
        }

        public async VoidTask LoginAsync(LoginInfo loginInfo)
        {
            await LoginAsync(loginInfo.Username, loginInfo.Password, loginInfo.Company, loginInfo.Branch, loginInfo.Locale);
        }

        public async VoidTask LoginAsync(string name, string password, string company = null, string branch = null, string locale = null)
        {
            throw new NotImplementedException();
            //await TryCatchAsync("Login", async () =>
            //(await _httpClient.PostAsJsonAsync(_apiConnectionConfig.EndpointSettings.LoginUrl,
            //    new
            //    {
            //        name,
            //        password,
            //        company,
            //        branch,
            //        locale
            //    })).EnsureSuccessStatusCode());
        }

        public void Logout()
        {
            LogoutAsync().Wait();
        }

        public VoidTask LogoutAsync()
        {
            return TryCatchAsync("Logout", _httpClient.PostAsync(_apiConnectionConfig.EndpointSettings.LogoutUrl, new ByteArrayContent(new byte[0])));
        }

        public T Put<T>(T entity) where T : Entity
        {
            return PutAsync(entity).Result;
        }

        public async Task<T> PutAsync<T>(T entity) where T : Entity
        {
            return await TryCatchAsync("Put " + typeof(T).Name, async () =>
            {
                var res = await _httpClient
                    .PutAsync(_endpointAddress + typeof(T).Name, ToStringContent(entity));

                res.EnsureSuccessStatusCode();

                var obj = await res.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<T>(obj);
            });
        }

        public Task<T> PutAsync<T>(T entity, CancellationToken cancellationToken) where T : Entity
        {
            cancellationToken.ThrowIfCancellationRequested();
            return PutAsync(entity);
        }

        public void Dispose()
        {
            ((IDisposable)_httpClient).Dispose();
        }

        private T TryCatch<T>(string descr, Func<T> action)
        {
            try
            {
                return action();
            }
            catch (Exception e)
            {
                var text = $"Action \"{descr}\" failed.";
                _logger.Error(e, text);
                throw new ApiException(text, e);
            }
        }

        private void TryCatch(string descr, System.Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                var text = $"Action \"{descr}\" failed.";
                _logger.Error(e, text);
                throw new ApiException(text, e);
            }
        }

        private async Task<T> TryCatchAsync<T>(string descr, Task<T> task)
        {
            try
            {
                return await task;
            }
            catch (Exception e)
            {
                var text = $"Action \"{descr}\" failed.";
                _logger.Error(e, text);
                throw new ApiException(text, e);
            }
        }

        private async Task<T> TryCatchAsync<T>(string descr, Func<Task<T>> task)
        {
            try
            {
                return await task();
            }
            catch (Exception e)
            {
                var text = $"Action \"{descr}\" failed.";
                _logger.Error(e, text);
                throw new ApiException(text, e);
            }
        }

        private async VoidTask TryCatchAsync(string descr, VoidTask task)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                var text = $"Action \"{descr}\" failed.";
                _logger.Error(e, text);
                throw new ApiException(text, e);
            }
        }

        private async Task<T> TryCatchAsync<T>(string descr, Task<T> task, CancellationToken cancellationToken)
        {
            try
            {
                return await task.WithCancellation(cancellationToken);
            }
            catch (Exception e)
            {
                var text = $"Action \"{descr}\" failed.";
                _logger.Error(e, text);
                throw new ApiException(text, e);
            }
        }

        private async VoidTask TryCatchAsync(string descr, VoidTask task, CancellationToken cancellationToken)
        {
            try
            {
                await task.WithCancellation(cancellationToken);
            }
            catch (Exception e)
            {
                var text = $"Action \"{descr}\" failed.";
                _logger.Error(e, text);
                throw new ApiException(text, e);
            }
        }

        private StringContent ToStringContent(object obj)
        {
            return new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
        }

        private class LogoutClientImpl : AcumaticaSoapClient, ILoginLogoutApiClient, IDisposable
        {
            public LogoutClientImpl(EndpointSettings endpointSettings) : base(endpointSettings)
            {
            }
            void IDisposable.Dispose()
            {
                Logout();
                base.Dispose();
            }
        }
    }
}
