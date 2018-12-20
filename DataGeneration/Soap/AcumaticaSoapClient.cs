using DataGeneration.Core;
using DataGeneration.Core.Api;
using DataGeneration.Core.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using DataGeneration.Core.Common;
using VoidTask = System.Threading.Tasks.Task;

namespace DataGeneration.Soap
{
    public class AcumaticaSoapClient : IApiClient, IDisposable
    {
        #region Initialization

        private static readonly ILogger _logger = LogHelper.GetLogger(LogHelper.LoggerNames.ApiClient);

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

        public static ILoginLogoutApiClient LoginLogoutClient(ApiConnectionConfig connectionConfig)
        {
            if (connectionConfig == null)
            {
                throw new ArgumentNullException(nameof(connectionConfig));
            }

            var client = LogoutClient(connectionConfig.EndpointSettings);

            client.Login(connectionConfig.LoginInfo);

            return (ILoginLogoutApiClient)client;
        }

        public static async Task<ILoginLogoutApiClient> LoginLogoutClientAsync(ApiConnectionConfig connectionConfig, CancellationToken cancellationToken = default)
        {
            if (connectionConfig == null)
            {
                throw new ArgumentNullException(nameof(connectionConfig));
            }

            var client = LogoutClient(connectionConfig.EndpointSettings);

            await client.LoginAsync(connectionConfig.LoginInfo, cancellationToken);

            return (ILoginLogoutApiClient)client;
        }

        public static ILogoutApiClient LogoutClient(EndpointSettings endpointSettings)
        {
            if (endpointSettings == null)
            {
                throw new ArgumentNullException(nameof(endpointSettings));
            }

            return new LogoutClientImpl(endpointSettings);
        }

        public int RetryCount { get; set; }

        #endregion

        #region Login Logout

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
            TryCatch(
                () => _client.Login(name, password, company, branch, locale),
                LoginArgs()
            );
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
            await TryCatchAsync(
                () => _client.LoginAsync(name, password, company, branch, locale),
                cancellationToken,
                LoginArgs()
            );
        }

        public void Logout()
        {
            TryCatch(
                () => _client.Logout(),
                LogoutArgs()
            );
        }

        public async VoidTask LogoutAsync()
        {
            await TryCatchAsync(
                () => _client.LogoutAsync(),
                LogoutArgs()
            );
        }

        #endregion

        #region Crud

        public T Get<T>(T whereEntity) where T : Entity
        {
            return TryCatch(
                () => _client.Get(whereEntity),
                CrudLogArgs<T>("Get")
            );
        }

        public async Task<T> GetAsync<T>(T whereEntity) where T : Entity
        {
            return await TryCatchAsync(
               () => _client.GetAsync(whereEntity),
               CrudLogArgs<T>("Get")
            );
        }

        public async Task<T> GetAsync<T>(T whereEntity, CancellationToken cancellationToken) where T : Entity
        {
            return await TryCatchAsync(
               () => _client.GetAsync(whereEntity),
               cancellationToken,
               CrudLogArgs<T>("Get")
            );
        }

        public IList<T> GetList<T>(T whereEntity) where T : Entity
        {
            return TryCatch(
                () => _client.GetList(whereEntity),
                CrudLogArgs<T>("Get List")
            );
        }

        public async Task<IList<T>> GetListAsync<T>(T whereEntity) where T : Entity
        {
            return await TryCatchAsync(
               () => _client.GetListAsync(whereEntity),
               CrudLogArgs<T>("Get List")
            );
        }

        public async Task<IList<T>> GetListAsync<T>(T whereEntity, CancellationToken cancellationToken) where T : Entity
        {
            return await TryCatchAsync(
               () => _client.GetListAsync(whereEntity),
               cancellationToken,
               CrudLogArgs<T>("Get List")
            );
        }

        public T Put<T>(T entity) where T : Entity
        {
            return TryCatch(
                () => _client.Put(entity),
                CrudLogArgs<T>("Put")
            );
        }

        public async Task<T> PutAsync<T>(T entity) where T : Entity
        {
            return await TryCatchAsync(
               () => _client.PutAsync(entity),
               CrudLogArgs<T>("Put")
            );
        }

        public async Task<T> PutAsync<T>(T entity, CancellationToken cancellationToken) where T : Entity
        {
            return await TryCatchAsync(
               () => _client.PutAsync(entity),
               cancellationToken,
               CrudLogArgs<T>("Put")
            );
        }

        public void Delete<T>(T whereEntity) where T : Entity
        {
            TryCatch(
                () => _client.Delete(whereEntity),
                CrudLogArgs<T>("Delete")
            );
        }

        public async VoidTask DeleteAsync<T>(T whereEntity) where T : Entity
        {
            await TryCatchAsync(
               () => _client.DeleteAsync(whereEntity),
               CrudLogArgs<T>("Delete")
            );
        }

        public async VoidTask DeleteAsync<T>(T whereEntity, CancellationToken cancellationToken) where T : Entity
        {
            await TryCatchAsync(
               () => _client.DeleteAsync(whereEntity),
               cancellationToken,
               CrudLogArgs<T>("Delete")
            );
        }

        #endregion

        #region Actions

        //todo: add wait for all invoke
        public void Invoke<TEntity, TAction>(TEntity entity, TAction action) where TEntity : Entity where TAction : Action
        {
            TryCatch(
                () => _client.Invoke(entity, action),
                InvokeArgs<TEntity, TAction>()
            );
        }

        public async VoidTask InvokeAsync<TEntity, TAction>(TEntity entity, TAction action) where TEntity : Entity where TAction : Action
        {
            await TryCatchAsync(
                () => _client.InvokeAsync(entity, action),
                InvokeArgs<TEntity, TAction>()
            );
        }

        public async VoidTask InvokeAsync<TEntity, TAction>(TEntity entity, TAction action, CancellationToken cancellationToken) where TEntity : Entity where TAction : Action
        {
            await TryCatchAsync(
                () => _client.InvokeAsync(entity, action),
                cancellationToken,
                InvokeArgs<TEntity, TAction>()
            );
        }

        public IList<File> GetFiles<T>(T entity) where T : Entity
        {
            return TryCatch(
                () => _client.GetFiles(entity),
                GetFilesArgs<T>()
            );
        }

        public async Task<IList<File>> GetFilesAsync<T>(T entity) where T : Entity
        {
            return await TryCatchAsync(
                () => _client.GetFilesAsync(entity),
                GetFilesArgs<T>()
            );
        }

        public async Task<IList<File>> GetFilesAsync<T>(T entity, CancellationToken cancellationToken) where T : Entity
        {
            return await TryCatchAsync(
                () => _client.GetFilesAsync(entity),
                cancellationToken,
                GetFilesArgs<T>()
            );
        }

        public void PutFiles<T>(T entity, IEnumerable<File> files) where T : Entity
        {
            TryCatch(
                () => _client.PutFiles(entity, files.ToArray()),
                PutFilesArgs<T>()
            );
        }

        public async VoidTask PutFilesAsync<T>(T entity, IEnumerable<File> files) where T : Entity
        {
            await TryCatchAsync(
                () => _client.PutFilesAsync(entity, files.ToArray()),
                PutFilesArgs<T>()
            );
        }

        public async VoidTask PutFilesAsync<T>(T entity, IEnumerable<File> files, CancellationToken cancellationToken) where T : Entity
        {
            await TryCatchAsync(
                () => _client.PutFilesAsync(entity, files.ToArray()),
                cancellationToken,
                PutFilesArgs<T>()
            );
        }

        #endregion

        #region Try Catch

        private void TryCatch(System.Action action, LogArgs logArgs, int attempt = 1)
        {
            try
            {
                logArgs.LogStart();
                using (logArgs.LogCompleted())
                {
                    action();
                }
            }
            catch (OperationCanceledException oce)
            {
                logArgs.LogCanceled(oce);
                throw;
            }
            catch (TimeoutException te)
            {
                throw logArgs.LogFailedAndGetException(te);
            }
            catch (Exception e)
            {
                if (attempt > RetryCount)
                    throw logArgs.LogFailedAndGetException(e, attempt);
                logArgs.LogRetry(e, attempt);
                TryCatch(action, logArgs, ++attempt);
            }
        }

        private T TryCatch<T>(Func<T> action, LogArgs logArgs, int attempt = 1)
        {
            try
            {
                logArgs.LogStart();
                using (logArgs.LogCompleted())
                {
                    return action();
                }
            }
            catch (OperationCanceledException oce)
            {
                logArgs.LogCanceled(oce);
                throw;
            }
            catch (TimeoutException te)
            {
                throw logArgs.LogFailedAndGetException(te);
            }
            catch (Exception e)
            {
                if (attempt > RetryCount)
                    throw logArgs.LogFailedAndGetException(e, attempt);
                logArgs.LogRetry(e, attempt);
                return TryCatch(action, logArgs, ++attempt);
            }
        }

        private async VoidTask TryCatchAsync(Func<VoidTask> task, LogArgs logArgs, int attempt = 1)
        {
            try
            {
                logArgs.LogStart();
                using (logArgs.LogCompleted())
                {
                    await task();
                }
            }
            catch (OperationCanceledException oce)
            {
                logArgs.LogCanceled(oce);
                throw;
            }
            catch (TimeoutException te)
            {
                throw logArgs.LogFailedAndGetException(te);
            }
            catch (Exception e)
            {
                if (attempt > RetryCount)
                    throw logArgs.LogFailedAndGetException(e, attempt);
                logArgs.LogRetry(e, attempt);
                await TryCatchAsync(task, logArgs, ++attempt);
            }
        }

        private async Task<T> TryCatchAsync<T>(Func<Task<T>> task, LogArgs logArgs, int attempt = 1)
        {
            try
            {
                logArgs.LogStart();
                using (logArgs.LogCompleted())
                {
                    return await task();
                }
            }
            catch (OperationCanceledException oce)
            {
                logArgs.LogCanceled(oce);
                throw;
            }
            catch (TimeoutException te)
            {
                throw logArgs.LogFailedAndGetException(te);
            }
            catch (Exception e)
            {
                if (attempt > RetryCount)
                    throw logArgs.LogFailedAndGetException(e, attempt);
                logArgs.LogRetry(e, attempt);
                return await TryCatchAsync(task, logArgs, ++attempt);
            }
        }

        private async Task<T> TryCatchAsync<T>(Func<Task<T>> task, CancellationToken cancellationToken, LogArgs logArgs, int attempt = 1)
        {
#if DISABLE_API_CANCELLATION
            return await TryCatchAsync(task, logArgs, attempt);
#else
            throw new NotImplemetedException();
#endif
        }

        private async VoidTask TryCatchAsync(Func<VoidTask> task, CancellationToken cancellationToken, LogArgs logArgs, int attempt = 1)
        {
#if DISABLE_API_CANCELLATION
            await TryCatchAsync(task, logArgs, attempt);
#else
            throw new NotImplemetedException();
#endif
        }

        #endregion

        #region Log

        private LogArgs CrudLogArgs<T>(string action) => new LogArgs($"{action} {typeof(T).Name}");
        private LogArgs InvokeArgs<TEntity, TAction>() => new LogArgs($"Invoke {typeof(TAction).Name} for {typeof(TEntity).Name}");
        private LogArgs GetFilesArgs<T>() => new LogArgs($"Get Files for {typeof(T).Name}");
        private LogArgs PutFilesArgs<T>() => new LogArgs($"Put Files for {typeof(T).Name}");
        private LogArgs LoginArgs() => new LogArgs($"Login to {_client.Endpoint.Address.Uri}");
        private LogArgs LogoutArgs() => new LogArgs($"Logout from {_client.Endpoint.Address.Uri}");

        private class LogArgs
        {
            public readonly string Text;

            public const string OperationStarted = "Operation \"{0}\" started";
            public const string OperationCompleted = "Operation \"{0}\" completed";
            public const string OperationFailed = "Operation \"{0}\" failed";
            public const string OperationCanceled = "Operation \"{0}\" canceled";
            public const string RetryAddition = ", Retry";
            public const string AttemptAddition = ", Attempt: {1}";


            public void LogStart()
            {
                _logger.Trace(OperationStarted.FormatWith(Text));
            }

            public IDisposable LogCompleted()
            {
                return StopwatchLoggerFactory.LogDispose(_logger, OperationCompleted.FormatWith(Text));
            }

            public void LogFailed(Exception e, int attempt = 1)
            {
                _logger.Error(e, (OperationFailed + AttemptAddition).FormatWith(Text, attempt));
            }

            public ApiException LogFailedAndGetException(Exception e, int attempt = 1)
            {
                var text = (OperationFailed + AttemptAddition).FormatWith(Text, attempt);
                _logger.Error(e, text);
                return new ApiException(text, e);
            }

            public void LogCanceled(Exception e)
            {
                _logger.Error(e, OperationFailed.FormatWith(Text));
            }

            public void LogRetry(Exception e, int attempt = 1)
            {
                _logger.Warn(e, (OperationFailed + AttemptAddition + RetryAddition).FormatWith(Text, attempt));
            }

            public LogArgs(string text)
            {
                Text = text;
            }
        }

        #endregion

        #region Common

        public void Dispose()
        {
            _client.Close();
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

        #endregion
    }
}