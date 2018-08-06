using CrmDataGeneration.Common;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VoidTask = System.Threading.Tasks.Task;

namespace CrmDataGeneration.Soap
{
    public class AcumaticaSoapClient : IApiClient ,IDisposable
    {
        private static ILogger _logger => LogConfiguration.DefaultLogger;

        private readonly DefaultSoapClient _client;

        /// <summary>
        ///     Throw execeptions on errors happened during execution API requests.
        /// Otherwise only logger log exception and execution will continue.
        /// Such exceptions as Argument exception will be thrown anyway.
        /// Default is true.
        /// </summary>
        public bool ThrowOnErrors { get; set; } = true;

        static AcumaticaSoapClient()
        {
            // otherwise all soap clients will use same settings
            ClientBase<DefaultSoapClient>.CacheSetting = CacheSetting.AlwaysOff;
        }

        public AcumaticaSoapClient(DefaultSoapClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public AcumaticaSoapClient(EndpointSettings endpointSettings)
        {
            if (endpointSettings == null)
            {
                throw new ArgumentNullException(nameof(endpointSettings));
            }

            _client = new DefaultSoapClient(
                new BasicHttpBinding
                {
                    AllowCookies = true,
                    MaxReceivedMessageSize = int.MaxValue,
                    SendTimeout = TimeSpan.FromHours(1)
                },
                new EndpointAddress(endpointSettings.EndpointUrl)
            );
        }

        public static ILoginLogoutApiClient LoginLogoutClient(ApiConnectionConfig connectionConfig, bool throwOnErrors = true)
        {
            if (connectionConfig == null)
            {
                throw new ArgumentNullException(nameof(connectionConfig));
            }

            var client = LogoutClient(connectionConfig.EndpointSettings, throwOnErrors);

            client.Login(connectionConfig.LoginInfo);

            return (ILoginLogoutApiClient)client;
        }

        public static async Task<ILoginLogoutApiClient> LoginLogoutClientAsync(ApiConnectionConfig connectionConfig, bool throwOnErrors = true)
        {
            if (connectionConfig == null)
            {
                throw new ArgumentNullException(nameof(connectionConfig));
            }


            var client = LogoutClient(connectionConfig.EndpointSettings, throwOnErrors);

            await client.LoginAsync(connectionConfig.LoginInfo);

            return (ILoginLogoutApiClient)client;
        }

        public static ILogoutApiClient LogoutClient(EndpointSettings endpointSettings, bool throwOnErrors = true)
        {
            if (endpointSettings == null)
            {
                throw new ArgumentNullException(nameof(endpointSettings));
            }

            return new LogoutClientImpl(endpointSettings)
            {
                ThrowOnErrors = throwOnErrors
            };
        }

        public void Login(LoginInfo loginInfo)
        {
            if (loginInfo == null)
            {
                throw new ArgumentNullException(nameof(loginInfo));
            }

            _logger.Info("Login to {acumatica}.", _client.Endpoint.Address.Uri);

            TryCatch("Login", () => _client.Login(loginInfo.Username, loginInfo.Password, loginInfo.Company, loginInfo.Branch, loginInfo.Locale));
        }

        public void Login(string name, string password, string company = null, string branch = null, string locale = null)
        {
            _logger.Info("Login to {acumatica}.", _client.Endpoint.Address.Uri);

            TryCatch("Login", () => _client.Login(name, password, company, branch, locale));
        }

        public async VoidTask LoginAsync(LoginInfo loginInfo)
        {
            if (loginInfo == null)
            {
                throw new ArgumentNullException(nameof(loginInfo));
            }

            _logger.Info("Login to {acumatica}.", _client.Endpoint.Address.Uri);

            await TryCatchAsync("Login", _client.LoginAsync(loginInfo.Username, loginInfo.Password, loginInfo.Company, loginInfo.Branch, loginInfo.Locale));
        }

        public async VoidTask LoginAsync(string name, string password, string company = null, string branch = null, string locale = null)
        {
            _logger.Info("Login to {acumatica}.", _client.Endpoint.Address.Uri);

            await TryCatchAsync("Login", _client.LoginAsync(name, password, company, branch, locale));
        }

        public void Logout()
        {
            _logger.Info("Logout from {acumatica}.", _client.Endpoint.Address.Uri);

            TryCatch("Logout", () => _client.Logout());
        }

        public async VoidTask LogoutAsync()
        {
            _logger.Info("Logout from {acumatica}.", _client.Endpoint.Address.Uri);

            await TryCatchAsync("Logout", _client.LogoutAsync());
        }

        public T Get<T>(T whereEntity) where T : Entity
        {
            return TryCatch($"Get {typeof(T).Name}", () => _client.Get(whereEntity));
        }

        public async Task<T> GetAsync<T>(T whereEntity) where T : Entity
        {
            return await TryCatchAsync($"Get {typeof(T).Name}", _client.GetAsync(whereEntity));
        }

        public async Task<T> GetAsync<T>(T whereEntity, CancellationToken cancellationToken) where T : Entity
        {
            return await TryCatchAsync($"Get {typeof(T).Name}", _client.GetAsync(whereEntity), cancellationToken);
        }

        public IList<T> GetList<T>(T whereEntity) where T : Entity
        {
            return TryCatch($"Get list {typeof(T).Name}", () => _client.GetList(whereEntity));
        }

        public async Task<IList<T>> GetListAsync<T>(T whereEntity) where T : Entity
        {
            return await TryCatchAsync($"Get list {typeof(T).Name}", _client.GetListAsync(whereEntity));
        }

        public async Task<IList<T>> GetListAsync<T>(T whereEntity, CancellationToken cancellationToken) where T : Entity
        {
            return await TryCatchAsync($"Get list {typeof(T).Name}", _client.GetListAsync(whereEntity), cancellationToken);
        }

        public T Put<T>(T entity) where T : Entity
        {
            return TryCatch($"Put {typeof(T).Name}", () => _client.Put(entity));
        }

        public async Task<T> PutAsync<T>(T entity) where T : Entity
        {
            return await TryCatchAsync($"Put {typeof(T).Name}", _client.PutAsync(entity));
        }

        public async Task<T> PutAsync<T>(T entity, CancellationToken cancellationToken) where T : Entity
        {
            return await TryCatchAsync($"Put {typeof(T).Name}", _client.PutAsync(entity), cancellationToken);
        }

        public void Delete<T>(T entity) where T : Entity
        {
            TryCatch($"Delete {typeof(T).Name}", () => _client.Delete(entity));
        }

        public async VoidTask DeleteAsync<T>(T entity) where T : Entity
        {
            await TryCatchAsync($"Delete {typeof(T).Name}", _client.DeleteAsync(entity));
        }

        public async VoidTask DeleteAsync<T>(T entity, CancellationToken cancellationToken) where T : Entity
        {
            await TryCatchAsync($"Delete {typeof(T).Name}", _client.DeleteAsync(entity), cancellationToken);
        }

        public void Invoke<TEntity, TAction>(TEntity entity, TAction action) where TEntity : Entity where TAction : Action
        {
            TryCatch($"Invoke {typeof(TAction).Name} on {typeof(TEntity)}", () => _client.Invoke(entity, action));
        }

        public async VoidTask InvokeAsync<TEntity, TAction>(TEntity entity, TAction action) where TEntity : Entity where TAction : Action
        {
            await TryCatchAsync($"Invoke {typeof(TAction).Name} on {typeof(TEntity)}", _client.InvokeAsync(entity, action));
        }

        public async VoidTask InvokeAsync<TEntity, TAction>(TEntity entity, TAction action, CancellationToken cancellationToken) where TEntity : Entity where TAction : Action
        {
            await TryCatchAsync($"Invoke {typeof(TAction).Name} on {typeof(TEntity)}", _client.InvokeAsync(entity, action), cancellationToken);
        }

        public void Dispose()
        {
            ((IDisposable)_client).Dispose();
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
                if(ThrowOnErrors)
                    throw new AcumaticaException(text, e);
                return default;
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
                if (ThrowOnErrors)
                    throw new AcumaticaException(text, e);
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
                if (ThrowOnErrors)
                    throw new AcumaticaException(text, e);
                return default;
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
                if (ThrowOnErrors)
                    throw new AcumaticaException(text, e);
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
                if (ThrowOnErrors)
                    throw new AcumaticaException(text, e);
                return default;
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
                if (ThrowOnErrors)
                    throw new AcumaticaException(text, e);
            }
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
