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
        #region Initialization

        private const string _loggerName = Common.LogManager.LoggerNames.ApiClient;

        private static ILogger _logger { get; } = Common.LogManager.GetLogger(_loggerName);

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
                CrudLogArgs("Get", whereEntity)
            );
        }

        public async Task<T> GetAsync<T>(T whereEntity) where T : Entity
        {
            return await TryCatchAsync(
               () => _client.GetAsync(whereEntity),
               CrudLogArgs("Get", whereEntity)
            );
        }

        public async Task<T> GetAsync<T>(T whereEntity, CancellationToken cancellationToken) where T : Entity
        {
            return await TryCatchAsync(
               () => _client.GetAsync(whereEntity),
               cancellationToken,
               CrudLogArgs("Get", whereEntity)
            );
        }

        public IList<T> GetList<T>(T whereEntity) where T : Entity
        {
            return TryCatch(
                () => _client.GetList(whereEntity),
                CrudLogArgs("Get List", whereEntity)
            );
        }

        public async Task<IList<T>> GetListAsync<T>(T whereEntity) where T : Entity
        {
            return await TryCatchAsync(
               () => _client.GetListAsync(whereEntity),
               CrudLogArgs("Get List", whereEntity)
            );
        }

        public async Task<IList<T>> GetListAsync<T>(T whereEntity, CancellationToken cancellationToken) where T : Entity
        {
            return await TryCatchAsync(
               () => _client.GetListAsync(whereEntity),
               cancellationToken,
               CrudLogArgs("Get List", whereEntity)
            );
        }

        public T Put<T>(T entity) where T : Entity
        {
            return TryCatch(
                () => _client.Put(entity),
                CrudLogArgs("Put", entity)
            );
        }

        public async Task<T> PutAsync<T>(T entity) where T : Entity
        {
            return await TryCatchAsync(
               () => _client.PutAsync(entity),
               CrudLogArgs("Put", entity)
            );
        }

        public async Task<T> PutAsync<T>(T entity, CancellationToken cancellationToken) where T : Entity
        {
            return await TryCatchAsync(
               () => _client.PutAsync(entity),
               cancellationToken,
               CrudLogArgs("Put", entity)
            );
        }

        public void Delete<T>(T whereEntity) where T : Entity
        {
            TryCatch(
                () => _client.Delete(whereEntity),
                CrudLogArgs("Delete", whereEntity)
            );
        }

        public async VoidTask DeleteAsync<T>(T whereEntity) where T : Entity
        {
            await TryCatchAsync(
               () => _client.DeleteAsync(whereEntity),
               CrudLogArgs("Delete", whereEntity)
            );
        }

        public async VoidTask DeleteAsync<T>(T whereEntity, CancellationToken cancellationToken) where T : Entity
        {
            await TryCatchAsync(
               () => _client.DeleteAsync(whereEntity),
               cancellationToken,
               CrudLogArgs("Delete", whereEntity)
            );
        }

        #endregion

        #region Actions

        //todo: add wait for all invoke
        public void Invoke<TEntity, TAction>(TEntity entity, TAction action) where TEntity : Entity where TAction : Action
        {
            TryCatch(
                () => _client.Invoke(entity, action),
                InvokeArgs(entity, action)
            );
        }

        public async VoidTask InvokeAsync<TEntity, TAction>(TEntity entity, TAction action) where TEntity : Entity where TAction : Action
        {
            await TryCatchAsync(
                () => _client.InvokeAsync(entity, action),
                InvokeArgs(entity, action)
            );
        }

        public async VoidTask InvokeAsync<TEntity, TAction>(TEntity entity, TAction action, CancellationToken cancellationToken) where TEntity : Entity where TAction : Action
        {
            await TryCatchAsync(
                () => _client.InvokeAsync(entity, action),
                cancellationToken,
                InvokeArgs(entity, action)
            );
        }

        public IList<File> GetFiles<T>(T entity) where T : Entity
        {
            return TryCatch(
                () => _client.GetFiles(entity),
                GetFilesArgs(entity)
            );
        }

        public async Task<IList<File>> GetFilesAsync<T>(T entity) where T : Entity
        {
            return await TryCatchAsync(
                () => _client.GetFilesAsync(entity),
                GetFilesArgs(entity)
            );
        }

        public async Task<IList<File>> GetFilesAsync<T>(T entity, CancellationToken cancellationToken) where T : Entity
        {
            return await TryCatchAsync(
                () => _client.GetFilesAsync(entity),
                cancellationToken,
                GetFilesArgs(entity)
            );
        }

        public void PutFiles<T>(T entity, IEnumerable<File> files) where T : Entity
        {
            TryCatch(
                () => _client.PutFiles(entity, files.ToArray()),
                PutFilesArgs(entity, files)
            );
        }

        public async VoidTask PutFilesAsync<T>(T entity, IEnumerable<File> files) where T : Entity
        {
            await TryCatchAsync(
                () => _client.PutFilesAsync(entity, files.ToArray()),
                PutFilesArgs(entity, files)
            );
        }

        public async VoidTask PutFilesAsync<T>(T entity, IEnumerable<File> files, CancellationToken cancellationToken) where T : Entity
        {
            await TryCatchAsync(
                () => _client.PutFilesAsync(entity, files.ToArray()),
                cancellationToken,
                PutFilesArgs(entity, files)
            );
        }

        #endregion

        #region Try Catch

        private void TryCatch(System.Action action, LogArgs logArgs)
        {
            try
            {
                logArgs.StartInfo.Log();
                using (logArgs.CompleteInfo.StopwatchLog())
                {
                    action();
                }
            }
            catch (OperationCanceledException oce)
            {
                logArgs.CancelInfo.Log(oce);
                throw;
            }
            catch (Exception e)
            {
                throw logArgs.FailInfo.LogAndGetException(e);
            }
        }

        private T TryCatch<T>(Func<T> action, LogArgs logArgs)
        {
            try
            {
                logArgs.StartInfo.Log();
                using (logArgs.CompleteInfo.StopwatchLog())
                {
                    return action();
                }
            }
            catch (OperationCanceledException oce)
            {
                logArgs.CancelInfo.Log(oce);
                throw;
            }
            catch (Exception e)
            {
                throw logArgs.FailInfo.LogAndGetException(e);
            }
        }

        private async VoidTask TryCatchAsync(Func<VoidTask> task, LogArgs logArgs)
        {
            try
            {
                logArgs.StartInfo.Log();
                using (logArgs.CompleteInfo.StopwatchLog())
                {
                    await task();
                }
            }
            catch (OperationCanceledException oce)
            {
                logArgs.CancelInfo.Log(oce);
                throw;
            }
            catch (Exception e)
            {
                throw logArgs.FailInfo.LogAndGetException(e);
            }
        }

        private async Task<T> TryCatchAsync<T>(Func<Task<T>> task, LogArgs logArgs)
        {
            try
            {
                logArgs.StartInfo.Log();
                using (logArgs.CompleteInfo.StopwatchLog())
                {
                    return await task();
                }
            }
            catch (OperationCanceledException oce)
            {
                logArgs.CancelInfo.Log(oce);
                throw;
            }
            catch (Exception e)
            {
                throw logArgs.FailInfo.LogAndGetException(e);
            }
        }

        private async Task<T> TryCatchAsync<T>(Func<Task<T>> task, CancellationToken cancellationToken, LogArgs logArgs)
        {
#if DISABLE_API_CANCELLATION
            return await TryCatchAsync(task, logArgs);
#else
            return await TryCatchPure(async () => { using (logArgs.CompleteInfo.StopwatchLog()) return await task().WithCancellation(cancellationToken); }, logArgs);
#endif
        }

        private async VoidTask TryCatchAsync(Func<VoidTask> task, CancellationToken cancellationToken, LogArgs logArgs)
        {
#if DISABLE_API_CANCELLATION
            await TryCatchAsync(task, logArgs);
#else
            await TryCatchPure(async () => { using (logArgs.CompleteInfo.StopwatchLog()) await task().WithCancellation(cancellationToken); }, logArgs);
#endif
        }

        #endregion

        #region Log

        private LogArgs CrudLogArgs<T>(string action, T whereEntity)
            => new LogArgs($"{action} {typeof(T)}", "{entity}", whereEntity);

        private LogArgs InvokeArgs<TEntity, TAction>(TEntity entity, TAction action)
            => new LogArgs($"Invoke {typeof(TAction)} on {typeof(TEntity)}", "{entity}, {action}", entity, action);

        private LogArgs GetFilesArgs<T>(T entity)
            => new LogArgs($"Get Files for {typeof(T)}", "{entity}", entity);

        private LogArgs PutFilesArgs<T>(T entity, IEnumerable<File> files)
            => new LogArgs($"Put Files for {typeof(T)}", "{entity}", entity);

        private LogArgs LoginArgs()
            => new LogArgs("Login", "Url = {url}", _client.Endpoint.Address.Uri);

        private LogArgs LogoutArgs()
            => new LogArgs("Logout", "Url = {url}", _client.Endpoint.Address.Uri);


        private class LogArgs
        {
            private readonly string _operation;
            private readonly string _argsLayout;
            private readonly object[] _args;

            private SingleLogArg _startInfo;
            private SingleLogArg _completeInfo;
            private SingleLogArg _failInfo;
            private SingleLogArg _cancelInfo;

            public LogLevel StartInfoLogLevel;
            public LogLevel CompleteInfoLogLevel;
            public LogLevel FailInfoLogLevel;
            public LogLevel CancelInfoLogLevel;

            public LogArgs(string operation, string argsLayout, params object[] args)
            {
                _operation = operation;
                _argsLayout = argsLayout;
                _args = args;

                StartInfoLogLevel = LogLevel.Trace;
                CompleteInfoLogLevel = LogLevel.Debug;
                FailInfoLogLevel = LogLevel.Error;
                CancelInfoLogLevel = LogLevel.Error;
            }

            public LogArgs(string operation, string argsLayout, object[] args,
                SingleLogArg startInfo = null,
                SingleLogArg completInfo = null,
                SingleLogArg failInfo = null,
                SingleLogArg cancelInfo = null)
                : this(operation, argsLayout, args)
            {
                _startInfo = startInfo;
                _completeInfo = completInfo;
                _failInfo = failInfo;
                _cancelInfo = cancelInfo;
            }

            public SingleLogArg StartInfo => _startInfo
                ?? (_startInfo = new SingleLogArg(
                    $"Operation {_operation} started. " + _argsLayout, StartInfoLogLevel, _args)
                );

            // tood: perhaps need to handle log args somehow (but not force, because it will affect performance
            public SingleLogArg CompleteInfo => _completeInfo
                ?? (_completeInfo = new SingleLogArg(
                    $"Operation {_operation} completed.", CompleteInfoLogLevel)
                );

            public SingleLogArg FailInfo => _failInfo
                ?? (_failInfo = new SingleLogArg(
                    $"Operation {_operation} failed.", FailInfoLogLevel)
                );

            public SingleLogArg CancelInfo => _cancelInfo
                ?? (_cancelInfo = new SingleLogArg(
                    $"Operation {_operation} canceled.", CompleteInfoLogLevel)
                );
        }

        private class SingleLogArg
        {
            public readonly string Text;
            public readonly object[] Args;
            public readonly LogLevel LogLevel;

            public SingleLogArg(string text, LogLevel logLevel, params object[] args)
            {
                Text = text;
                Args = args;
                LogLevel = logLevel;
            }

            public SingleLogArg(string text, params object[] args) : this(text, null, args)
            {
            }

            public bool IsEnabled => LogLevel != null;

            public void Log()
            {
                if (IsEnabled)
                    _logger.Log(LogLevel, Text, Args);
            }

            public void Log(Exception e)
            {
                if (IsEnabled)
                    _logger.Log(LogLevel, e, Text, Args);
            }

            public ApiException LogAndGetException(Exception e)
            {
                if (IsEnabled)
                    _logger.Log(LogLevel, e, Text, Args);
                return new ApiException(Text.FormatWith(Args), e);
            }

            public IDisposable StopwatchLog()
            {
                return StopwatchLoggerFactory.LogDispose(_loggerName, Text, Args);
            }



            public static implicit operator SingleLogArg(string text) => new SingleLogArg(text);
            public static implicit operator SingleLogArg((string text, object arg) log) => new SingleLogArg(log.text, log.arg);
            public static implicit operator SingleLogArg((string text, object arg1, object arg2) log) => new SingleLogArg(log.text, log.arg1, log.arg2);
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