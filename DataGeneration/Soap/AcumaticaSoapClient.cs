using DataGeneration.Common;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using VoidTask = System.Threading.Tasks.Task;

namespace DataGeneration.Soap
{
    public class AcumaticaSoapClient : IApiClient, IDisposable
    {
        private const string _loggerName = Common.LogManager.LoggerNames.ApiClient;

        private static ILogger _logger => Common.LogManager.GetLogger(_loggerName);

        private readonly DefaultSoapClient _client;

        static AcumaticaSoapClient()
        {
            // otherwise all soap clients will use same settings
            ClientBase<DefaultSoap>.CacheSetting = CacheSetting.AlwaysOff;
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

            _client = new DefaultSoapClient(endpointSettings.GetBinding(), endpointSettings.GetEndpointAddress());
        }

        public static AcumaticaSoapClient LoginLogoutClient(ApiConnectionConfig connectionConfig)
        {
            if (connectionConfig == null)
            {
                throw new ArgumentNullException(nameof(connectionConfig));
            }

            var client = LogoutClient(connectionConfig.EndpointSettings);

            client.Login(connectionConfig.LoginInfo);

            return client;
        }

        public static async Task<AcumaticaSoapClient> LoginLogoutClientAsync(ApiConnectionConfig connectionConfig, CancellationToken cancellationToken = default)
        {
            if (connectionConfig == null)
            {
                throw new ArgumentNullException(nameof(connectionConfig));
            }

            var client = LogoutClient(connectionConfig.EndpointSettings);

            await client.LoginAsync(connectionConfig.LoginInfo, cancellationToken);

            return client;
        }

        public static AcumaticaSoapClient LogoutClient(EndpointSettings endpointSettings)
        {
            if (endpointSettings == null)
            {
                throw new ArgumentNullException(nameof(endpointSettings));
            }

            return new LogoutClientImpl(endpointSettings);
        }

        public void Login(LoginInfo loginInfo)
        {
            if (loginInfo == null)
            {
                throw new ArgumentNullException(nameof(loginInfo));
            }

            Login(loginInfo.Username, loginInfo.Password, loginInfo.Company, loginInfo.Branch, loginInfo.Locale);
        }

        public void Login(string name, string password, string company = null, string branch = null, string locale = null)
        {
            _logger.Debug("Login to {acumatica}", _client.Endpoint.Address.Uri);
            using (Log("Login"))
            {
                TryCatch("Login", () => _client.Login(name, password, company, branch, locale));
            }
        }

        public VoidTask LoginAsync(LoginInfo loginInfo)
        {
            return LoginAsync(loginInfo, default);
        }

        public VoidTask LoginAsync(LoginInfo loginInfo, CancellationToken cancellationToken)
        {
            if (loginInfo == null)
            {
                throw new ArgumentNullException(nameof(loginInfo));
            }

            return LoginAsync(loginInfo.Username, loginInfo.Password, loginInfo.Company, loginInfo.Branch, loginInfo.Locale, cancellationToken);
        }

        public async VoidTask LoginAsync(string name, string password, string company = null, string branch = null, string locale = null, CancellationToken cancellationToken = default)
        {
            _logger.Debug("Login to {acumatica}", _client.Endpoint.Address.Uri);

            using (Log("Login"))
            {
                await TryCatchAsync("Login", _client.LoginAsync(name, password, company, branch, locale), cancellationToken);
            }
        }

        public void Logout()
        {
            _logger.Debug("Logout from {acumatica}", _client.Endpoint.Address.Uri);

            using (Log("Logout"))
            {
                TryCatch("Logout", () => _client.Logout());
            }
        }

        public async VoidTask LogoutAsync()
        {
            _logger.Debug("Logout from {acumatica}", _client.Endpoint.Address.Uri);

            using (Log("Logout"))
            {
                await TryCatchAsync("Logout", _client.LogoutAsync());
            }
        }

        public T Get<T>(T whereEntity) where T : Entity
        {
            using (Log<T>("Get"))
            {
                return TryCatch("Get {whereEntity}", () => _client.Get(whereEntity), whereEntity);
            }
        }

        public async Task<T> GetAsync<T>(T whereEntity) where T : Entity
        {
            using (Log<T>("Get"))
            {
                return await TryCatchAsync("Get {whereEntity}", _client.GetAsync(whereEntity), whereEntity);
            }
        }

        public async Task<T> GetAsync<T>(T whereEntity, CancellationToken cancellationToken) where T : Entity
        {
            using (Log<T>("Get"))
            {
                return await TryCatchAsync("Get {whereEntity}", _client.GetAsync(whereEntity), cancellationToken, whereEntity);
            }
        }

        public IList<T> GetList<T>(T whereEntity) where T : Entity
        {
            using (Log<T>("Get List"))
            {
                return TryCatch("Get list {whereEntity}", () => _client.GetList(whereEntity), whereEntity);
            }
        }

        public async Task<IList<T>> GetListAsync<T>(T whereEntity) where T : Entity
        {
            using (Log<T>("Get List"))
            {
                return await TryCatchAsync("Get list {whereEntity}", _client.GetListAsync(whereEntity), whereEntity);
            }
        }

        public async Task<IList<T>> GetListAsync<T>(T whereEntity, CancellationToken cancellationToken) where T : Entity
        {
            using (Log<T>("Get List"))
            {
                return await TryCatchAsync("Get list {whereEntity}", _client.GetListAsync(whereEntity), cancellationToken, whereEntity);
            }
        }

        public T Put<T>(T entity) where T : Entity
        {
            using (Log<T>("Put"))
            {
                return TryCatch("Put {entity}", () => _client.Put(entity), entity);
            }
        }

        public async Task<T> PutAsync<T>(T entity) where T : Entity
        {
            using (Log<T>("Put"))
            {
                return await TryCatchAsync("Put {entity}", _client.PutAsync(entity), entity);
            }
        }

        public async Task<T> PutAsync<T>(T entity, CancellationToken cancellationToken) where T : Entity
        {
            using (Log<T>("Put"))
            {
                return await TryCatchAsync("Put {entity}", _client.PutAsync(entity), cancellationToken, entity);
            }
        }

        public void Delete<T>(T whereEntity) where T : Entity
        {
            using (Log<T>("Delete"))
            {
                TryCatch("Delete {whereEntity}", () => _client.Delete(whereEntity), whereEntity);
            }
        }

        public async VoidTask DeleteAsync<T>(T whereEntity) where T : Entity
        {
            using (Log<T>("Delete"))
            {
                await TryCatchAsync("Delete {whereEntity}", _client.DeleteAsync(whereEntity), whereEntity);
            }
        }

        public async VoidTask DeleteAsync<T>(T whereEntity, CancellationToken cancellationToken) where T : Entity
        {
            using (Log<T>("Delete"))
            {
                await TryCatchAsync("Delete {whereEntity}", _client.DeleteAsync(whereEntity), cancellationToken, whereEntity);
            }
        }

        //todo: add wait for all invoke
        public void Invoke<TEntity, TAction>(TEntity entity, TAction action) where TEntity : Entity where TAction : Action
        {
            using (Log($"Invoke {typeof(TAction).Name} for {typeof(TEntity).Name}"))
            {
                TryCatch("Invoke {action} for {entity}", () => _client.Invoke(entity, action), action, entity);
            }
        }

        public async VoidTask InvokeAsync<TEntity, TAction>(TEntity entity, TAction action) where TEntity : Entity where TAction : Action
        {
            using (Log($"Invoke {typeof(TAction).Name} for {typeof(TEntity).Name}"))
            {
                await TryCatchAsync("Invoke {action} for {entity}", _client.InvokeAsync(entity, action), action, entity);
            }
        }

        public async VoidTask InvokeAsync<TEntity, TAction>(TEntity entity, TAction action, CancellationToken cancellationToken) where TEntity : Entity where TAction : Action
        {
            using (Log($"Invoke {typeof(TAction).Name} for {typeof(TEntity).Name}"))
            {
                await TryCatchAsync("Invoke {action} for {entity}", _client.InvokeAsync(entity, action), cancellationToken, action, entity);
            }
        }

        public void Dispose()
        {
            _client.Close();
        }

        private T TryCatch<T>(string descr, Func<T> action, params object[] logDebugArgs)
        {
            try
            {
                _logger.Trace(descr, logDebugArgs);
                return action();
            }
            catch (OperationCanceledException oce)
            {
                _logger.Error(oce, $"Action \"{descr}\" canceled.");
                throw;
            }
            catch (Exception e)
            {
                var text = $"Action \"{descr}\" failed.";
                _logger.Error(e, text, logDebugArgs);
                throw new ApiException(text, e);
            }
        }

        private void TryCatch(string descr, System.Action action, params object[] logDebugArgs)
        {
            try
            {
                _logger.Trace(descr, logDebugArgs);
                action();
            }
            catch (OperationCanceledException oce)
            {
                _logger.Error(oce, $"Action \"{descr}\" canceled.");
                throw;
            }
            catch (Exception e)
            {
                var text = $"Action \"{descr}\" failed.";
                _logger.Error(e, text, logDebugArgs);
                throw new ApiException(text, e);
            }
        }

        private async Task<T> TryCatchAsync<T>(string descr, Task<T> task, params object[] logDebugArgs)
        {
            try
            {
                _logger.Trace(descr, logDebugArgs);
                return await task;
            }
            catch (OperationCanceledException oce)
            {
                _logger.Error(oce, $"Action \"{descr}\" canceled.");
                throw;
            }
            catch (Exception e)
            {
                var text = $"Action \"{descr}\" failed.";
                _logger.Error(e, text, logDebugArgs);
                throw new ApiException(text, e);
            }
        }

        private async VoidTask TryCatchAsync(string descr, VoidTask task, params object[] logDebugArgs)
        {
            try
            {
                _logger.Trace(descr, logDebugArgs);
                await task;
            }
            catch (OperationCanceledException oce)
            {
                _logger.Error(oce, $"Action \"{descr}\" canceled.");
                throw;
            }
            catch (Exception e)
            {
                var text = $"Action \"{descr}\" failed.";
                _logger.Error(e, text, logDebugArgs);
                throw new ApiException(text, e);
            }
        }

        private async Task<T> TryCatchAsync<T>(string descr, Task<T> task, CancellationToken cancellationToken, params object[] logDebugArgs)
        {
            try
            {
                _logger.Trace(descr, logDebugArgs);
                return await task.WithCancellation(cancellationToken);
            }
            catch (OperationCanceledException oce)
            {
                _logger.Error(oce, $"Action \"{descr}\" canceled.");
                throw;
            }
            catch (Exception e)
            {
                var text = $"Action \"{descr}\" failed.";
                _logger.Error(e, text, logDebugArgs);
                throw new ApiException(text, e);
            }
        }

        private async VoidTask TryCatchAsync(string descr, VoidTask task, CancellationToken cancellationToken, params object[] logDebugArgs)
        {
            try
            {
                _logger.Trace(descr, logDebugArgs);
                await task.WithCancellation(cancellationToken);
            }
            catch (OperationCanceledException oce)
            {
                _logger.Error(oce, $"Action \"{descr}\" canceled.");
                throw;
            }
            catch (Exception e)
            {
                var text = $"Action \"{descr}\" failed.";
                _logger.Error(e, text, logDebugArgs);
                throw new ApiException(text, e);
            }
        }

        private IDisposable Log(string description, params object[] args)
        {
            return StopwatchLoggerFactory.Log(_loggerName, description, args);
        }

        private IDisposable Log<T>(string description, params object[] args)
        {
            return StopwatchLoggerFactory.Log(_loggerName, description + ' ' + typeof(T).Name, args);
        }

        private class LogoutClientImpl : AcumaticaSoapClient, ILoginLogoutApiClient, IDisposable
        {
            public LogoutClientImpl(EndpointSettings endpointSettings) : base(endpointSettings)
            {
            }

            void IDisposable.Dispose()
            {
                try
                {
                    Logout();
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Could not logout while disposing");
                }
                base.Dispose();
            }
        }
    }
}