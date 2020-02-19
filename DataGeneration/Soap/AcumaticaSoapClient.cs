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
using Newtonsoft.Json;

namespace DataGeneration.Soap
{
    public class AcumaticaSoapClient : IApiClient, ILoggerInjectable, IDisposable
    {
        #region Initialization

        private static readonly ILogger _logger = LogHelper.GetLogger(LogHelper.LoggerNames.ApiClient);

        private (object, object)[] _eventParams;
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

        public static ILogoutApiClient LoginLogoutClient(ApiConnectionConfig connectionConfig)
        {
            if (connectionConfig == null)
            {
                throw new ArgumentNullException(nameof(connectionConfig));
            }

            var client = LogoutClient(connectionConfig.EndpointSettings);

            client.Login(connectionConfig.LoginInfo);

            return client;
        }

        public static async Task<ILogoutApiClient> LoginLogoutClientAsync(ApiConnectionConfig connectionConfig, CancellationToken cancellationToken = default)
        {
            if (connectionConfig == null)
            {
                throw new ArgumentNullException(nameof(connectionConfig));
            }

            var client = LogoutClient(connectionConfig.EndpointSettings);

            await client.LoginAsync(connectionConfig.LoginInfo, cancellationToken);

            return client;
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
                CrudLogArgs<T>("Get", whereEntity)
            );
        }

        public async Task<T> GetAsync<T>(T whereEntity) where T : Entity
        {
            return await TryCatchAsync(
               () => _client.GetAsync(whereEntity),
               CrudLogArgs<T>("Get", whereEntity)
            );
        }

        public async Task<T> GetAsync<T>(T whereEntity, CancellationToken cancellationToken) where T : Entity
        {
            return await TryCatchAsync(
               () => _client.GetAsync(whereEntity),
               cancellationToken,
               CrudLogArgs<T>("Get", whereEntity)
            );
        }

        public IList<T> GetList<T>(T whereEntity) where T : Entity
        {
            return TryCatch(
                () => _client.GetList(whereEntity),
                CrudLogArgs<T>("Get List", whereEntity)
            );
        }

        public async Task<IList<T>> GetListAsync<T>(T whereEntity) where T : Entity
        {
            return await TryCatchAsync(
               () => _client.GetListAsync(whereEntity),
               CrudLogArgs<T>("Get List", whereEntity)
            );
        }

        public async Task<IList<T>> GetListAsync<T>(T whereEntity, CancellationToken cancellationToken) where T : Entity
        {
            return await TryCatchAsync(
               () => _client.GetListAsync(whereEntity),
               cancellationToken,
               CrudLogArgs<T>("Get List", whereEntity)
            );
        }

        public T Put<T>(T entity) where T : Entity
        {
            return TryCatch(
                () => _client.Put(entity),
                CrudLogArgs<T>("Put", entity)
            );
        }

        public async Task<T> PutAsync<T>(T entity) where T : Entity
        {
            return await TryCatchAsync(
               () => _client.PutAsync(entity),
               CrudLogArgs<T>("Put", entity)
            );
        }

        public async Task<T> PutAsync<T>(T entity, CancellationToken cancellationToken) where T : Entity
        {
            return await TryCatchAsync(
               () => _client.PutAsync(entity),
               cancellationToken,
               CrudLogArgs<T>("Put", entity)
            );
        }

        public void Delete<T>(T whereEntity) where T : Entity
        {
            TryCatch(
                () => _client.Delete(whereEntity),
                CrudLogArgs<T>("Delete", whereEntity)
            );
        }

        public async VoidTask DeleteAsync<T>(T whereEntity) where T : Entity
        {
            await TryCatchAsync(
               () => _client.DeleteAsync(whereEntity),
               CrudLogArgs<T>("Delete", whereEntity)
            );
        }

        public async VoidTask DeleteAsync<T>(T whereEntity, CancellationToken cancellationToken) where T : Entity
        {
            await TryCatchAsync(
               () => _client.DeleteAsync(whereEntity),
               cancellationToken,
               CrudLogArgs<T>("Delete", whereEntity)
            );
        }

        #endregion

        #region Actions

        //todo: add wait for all invoke
        public void Invoke<TEntity, TAction>(TEntity entity, TAction action) where TEntity : Entity where TAction : Action
        {
            TryCatch(
                () => AwaitInvokationResult(_client.Invoke(entity, action)).GetAwaiter().GetResult(),
                InvokeArgs<TEntity, TAction>(entity, action)
            );
        }

        public async VoidTask InvokeAsync<TEntity, TAction>(TEntity entity, TAction action) where TEntity : Entity where TAction : Action
        {
            await TryCatchAsync(
                async () => await AwaitInvokationResult(await _client.InvokeAsync(entity, action)),
                InvokeArgs<TEntity, TAction>(entity, action)
            );
        }

        public async VoidTask InvokeAsync<TEntity, TAction>(TEntity entity, TAction action, CancellationToken cancellationToken) where TEntity : Entity where TAction : Action
        {
            await TryCatchAsync(
                async () => await AwaitInvokationResult(await _client.InvokeAsync(entity, action), cancellationToken),
                cancellationToken,
                InvokeArgs<TEntity, TAction>(entity, action)
            );
        }

        private async VoidTask AwaitInvokationResult(InvokeResult invokeResult, CancellationToken cancellationToken = default)
        {
            while(true)
            {
                var status = await _client.GetProcessStatusAsync(invokeResult);
                switch (status.Status)
                {
                    case ProcessStatus.NotExists:
                    case ProcessStatus.Completed:
                        return;
                    case ProcessStatus.InProcess:
                        // todo: add global timeout
                        cancellationToken.ThrowIfCancellationRequested();
                        await VoidTask.Delay(1000);
                        continue;

                    case ProcessStatus.Aborted:
                        throw new SkipHandleException(
                            new ApiInvokationException($"Invokation was aborted. Message: {status.Message}"));
                    default:
                        throw new SkipHandleException(
                            new ApiInvokationException($"Invokation returned unexpected status code: {status.Status}, Message: {status.Message}"));
                }
            }

        }

        public IList<File> GetFiles<T>(T entity) where T : Entity
        {
            return TryCatch(
                () => _client.GetFiles(entity),
                GetFilesArgs<T>(entity)
            );
        }

        public async Task<IList<File>> GetFilesAsync<T>(T entity) where T : Entity
        {
            return await TryCatchAsync(
                () => _client.GetFilesAsync(entity),
                GetFilesArgs<T>(entity)
            );
        }

        public async Task<IList<File>> GetFilesAsync<T>(T entity, CancellationToken cancellationToken) where T : Entity
        {
            return await TryCatchAsync(
                () => _client.GetFilesAsync(entity),
                cancellationToken,
                GetFilesArgs<T>(entity)
            );
        }

        public void PutFiles<T>(T entity, IEnumerable<File> files) where T : Entity
        {
            TryCatch(
                () => _client.PutFiles(entity, files.ToArray()),
                PutFilesArgs<T>(entity)
            );
        }

        public async VoidTask PutFilesAsync<T>(T entity, IEnumerable<File> files) where T : Entity
        {
            await TryCatchAsync(
                () => _client.PutFilesAsync(entity, files.ToArray()),
                PutFilesArgs<T>(entity)
            );
        }

        public async VoidTask PutFilesAsync<T>(T entity, IEnumerable<File> files, CancellationToken cancellationToken) where T : Entity
        {
            await TryCatchAsync(
                () => _client.PutFilesAsync(entity, files.ToArray()),
                cancellationToken,
                PutFilesArgs<T>(entity)
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
            catch (SkipHandleException she)
            {
                throw logArgs.LogFailedAndGetException(she.InnerException, attempt);
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
                throw logArgs.LogFailedAndGetException(te, attempt);
            }
            catch (SkipHandleException she)
            {
                throw logArgs.LogFailedAndGetException(she.InnerException, attempt);
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
                throw logArgs.LogFailedAndGetException(te, attempt);
            }
            catch (SkipHandleException she)
            {
                throw logArgs.LogFailedAndGetException(she.InnerException, attempt);
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
                throw logArgs.LogFailedAndGetException(te, attempt);
            }
            catch (SkipHandleException she)
            {
                throw logArgs.LogFailedAndGetException(she.InnerException, attempt);
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

        private LogArgs CrudLogArgs<T>(string action, T entity) => new LogArgs($"{action} {typeof(T).Name}", _eventParams, entity);
        private LogArgs InvokeArgs<TEntity, TAction>(TEntity entity, TAction action) 
            => new LogArgs($"Invoke {typeof(TAction).Name} for {typeof(TEntity).Name}", _eventParams, _logger.IsTraceEnabled ? new { entity, action } : null);
        private LogArgs GetFilesArgs<T>(T entity) => new LogArgs($"Get Files for {typeof(T).Name}", _eventParams, entity);
        private LogArgs PutFilesArgs<T>(T entity) => new LogArgs($"Put Files for {typeof(T).Name}", _eventParams, entity);
        private LogArgs LoginArgs() => new LogArgs($"Login to {_client.Endpoint.Address.Uri}", _eventParams);
        private LogArgs LogoutArgs() => new LogArgs($"Logout from {_client.Endpoint.Address.Uri}", _eventParams);

        private class LogArgs
        {
            public readonly string Text;
            public readonly (object, object)[] EventParams;

            public const string OperationStarted = "Operation \"{0}\" started";
            public const string OperationCompleted = "Operation \"{0}\" completed";
            public const string OperationFailed = "Operation \"{0}\" failed";
            public const string OperationCanceled = "Operation \"{0}\" canceled";
            public const string RetryAddition = ", Retry";
            public const string AttemptAddition = ", Attempt: {1}";


            public void LogStart()
            {
                if (_logger.IsTraceEnabled)
                    LogHelper.LogWithEventParams(
                        _logger,
                        LogLevel.Trace,
                        OperationStarted.FormatWith(Text),
                        eventParams: EventParams
                    );
            }

            public IDisposable LogCompleted()
            {
                if (_logger.IsDebugEnabled)
                    return StopwatchLoggerFactory.LogDispose(
                        _logger,
                        LogLevel.Debug,
                        OperationCompleted.FormatWith(Text),
                        eventParams: EventParams);
                return DisposeHelper.NullDisposable;
            }

            public void LogFailed(Exception e, int attempt = 1)
            {
                if (_logger.IsErrorEnabled)
                    LogHelper.LogWithEventParams(
                        _logger,
                        LogLevel.Error,
                        (OperationFailed + AttemptAddition).FormatWith(Text, attempt),
                        exception: e,
                        eventParams: EventParams
                    );
            }

            public ApiException LogFailedAndGetException(Exception e, int attempt = 1)
            {
                var text = (OperationFailed + AttemptAddition).FormatWith(Text, attempt);
                if (_logger.IsErrorEnabled)
                    LogHelper.LogWithEventParams(
                        _logger,
                        LogLevel.Error,
                        text,
                        exception: e,
                        eventParams: EventParams
                    );
                return new ApiException(text, e);
            }

            public void LogCanceled(Exception e)
            {
                if (_logger.IsErrorEnabled)
                    LogHelper.LogWithEventParams(
                        _logger,
                        LogLevel.Error,
                        OperationCanceled.FormatWith(Text),
                        exception: e,
                        eventParams: EventParams
                    );
            }

            public void LogRetry(Exception e, int attempt = 1)
            {
                if (_logger.IsWarnEnabled)
                    LogHelper.LogWithEventParams(
                        _logger,
                        LogLevel.Warn,
                        (OperationFailed + AttemptAddition + RetryAddition).FormatWith(Text, attempt),
                        exception: e,
                        eventParams: EventParams
                    );
            }

            public LogArgs(string text, (object, object)[] parameters, object entity = null)
            {
                Text = text;

                if (_logger.IsTraceEnabled)
                {
                    (object, object) entityParam = ("entity", JsonConvert.SerializeObject(entity));
                    if (parameters == null)
                    {
                        EventParams = new[] { entityParam };
                    }
                    else
                    {
                        EventParams = new (object, object)[parameters.Length + 1];
                        parameters.CopyTo(EventParams, 0);
                        EventParams[parameters.Length] = entityParam;
                    }
                }
                else
                {
                    EventParams = parameters;
                }
            }
        }

        #endregion

        #region Common

        public void Dispose()
        {
            _client.Close();
        }

        public void InjectEventParameters(params (object name, object value)[] events)
        {
            _eventParams = events ?? throw new ArgumentNullException(nameof(events));
        }

        private class LogoutClientImpl : AcumaticaSoapClient, ILogoutApiClient, IDisposable
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

        private class SkipHandleException : Exception
        {
            public SkipHandleException(Exception e) : base(null, e) { }
        }

        private class ApiInvokationException : ApiException
        {
            public ApiInvokationException(string message) : base(message)
            {
            }
        }

        #endregion
    }
}