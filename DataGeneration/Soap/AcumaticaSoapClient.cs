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
                new LogArgs("Login to {acumatica}", LogLevel.Debug, _client.Endpoint.Address.Uri)
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
                new LogArgs("Login to {acumatica}", LogLevel.Debug, _client.Endpoint.Address.Uri)
            );
        }

        public void Logout()
        {
            TryCatch(
                () => _client.Logout(),
                new LogArgs("Logout from {acumatica}", LogLevel.Debug, _client.Endpoint.Address.Uri)
            );
        }

        public async VoidTask LogoutAsync()
        {
            await TryCatchAsync(
                () => _client.LogoutAsync(),
                new LogArgs("Logout from {acumatica}", LogLevel.Debug, _client.Endpoint.Address.Uri)
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

        private IDisposable Logger(LogArgs args) => StopwatchLoggerFactory.LogDispose(_loggerName, args.Description, args.Args);

        private LogArgs CrudLogArgs<T>(string action, T whereEntity) => new LogArgs(
            $"{action} {{entityType}}", typeof(T).AsArray(),
            $"{action} {{entityType}}: {{@entity}}", new object[] { typeof(T), whereEntity });

        private LogArgs InvokeArgs<TEntity, TAction>(TEntity entity, TAction action) => new LogArgs(
            "Invoke {actionType} on {entityType}", new object[] { typeof(TAction), typeof(TEntity) },
            "Invoke {actionType} on {entityType}: {@action}, {@entity}", new object[] { typeof(TAction), typeof(TEntity), action, entity });

        private LogArgs GetFilesArgs<T>(T entity) => new LogArgs(
            "Get Files for {entityType}", typeof(T).AsArray(),
            "Get Files for {entityType}: {@entity}", new object[] { typeof(T), entity });

        private LogArgs PutFilesArgs<T>(T entity, IEnumerable<File> files) => new LogArgs(
            "Put Files for {entityType}", typeof(T).AsArray(),
            "Put Files for {entityType}: {@entity}, {@files}", new object[] { typeof(T), entity, files });


        private T TryCatchPure<T>(Func<T> action, LogArgs logArgs)
        {
            try
            {
                if (logArgs.LogTrace())
                    _logger.Log(logArgs.TraceLogLevel, logArgs.TraceDescription, logArgs.TraceArgs);

                // move stopwatchlogger to overrides, to use logdispose with await
                return action();
            }
            catch (OperationCanceledException oce)
            {
                _logger.Error(oce, $"{logArgs.Description} canceled.", logArgs.Args);
                throw;
            }
            catch (Exception e)
            {
                var text = $"{logArgs.Description} failed.";
                _logger.Error(e, text, logArgs.Args);
                throw new ApiException(text, e);
            }
        }

        private T TryCatch<T>(Func<T> action, LogArgs logArgs)
        {
            return TryCatchPure(() => { using (Logger(logArgs)) return action(); }, logArgs);
        }

        private void TryCatch(System.Action action, LogArgs logArgs)
        {
            TryCatchPure((Func<object>)(() => { using (Logger(logArgs)) action(); return null; }), logArgs);
        }

        private async Task<T> TryCatchAsync<T>(Func<Task<T>> task, LogArgs logArgs)
        {
            return await TryCatchPure(async () => { using (Logger(logArgs)) return await task(); }, logArgs);
        }

        private async VoidTask TryCatchAsync(Func<VoidTask> task, LogArgs logArgs)
        {
            await TryCatchPure(async () => { using (Logger(logArgs)) await task(); }, logArgs);
        }

        private async Task<T> TryCatchAsync<T>(Func<Task<T>> task, CancellationToken cancellationToken, LogArgs logArgs)
        {
#if DISABLE_API_CANCELLATION
            return await TryCatchAsync(task, logArgs);
#else
            return await TryCatchPure(async () => { using (Logger(logArgs)) return await task().WithCancellation(cancellationToken); }, logArgs);
#endif
        }

        private async VoidTask TryCatchAsync(Func<VoidTask> task, CancellationToken cancellationToken, LogArgs logArgs)
        {
#if DISABLE_API_CANCELLATION
            await TryCatchAsync(task, logArgs);
#else
            await TryCatchPure(async () => { using (Logger(logArgs)) await task().WithCancellation(cancellationToken); }, logArgs);
#endif
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

        private class LogArgs
        {
            public LogArgs(string description, params object[] args)
            {
                Description = description;
                Args = args;
                TraceDescription = null;
                TraceArgs = null;
                TraceLogLevel = null;
            }

            public LogArgs(string description, object[] args, string traceDescription, object[] traceArgs)
                : this(description, args, traceDescription, traceArgs, LogLevel.Trace)
            {

            }

            public LogArgs(string description, object[] args, string traceDescription, object[] traceArgs, LogLevel traceLogLevel)
            {
                Description = description;
                Args = args;
                TraceDescription = traceDescription;
                TraceArgs = traceArgs;
                TraceLogLevel = traceLogLevel;
            }

            public LogArgs(string description, LogLevel traceLogLevel, params object[] args)
            {
                Description = description;
                Args = args;
                TraceDescription = description;
                TraceArgs = args;
                TraceLogLevel = traceLogLevel;
            }

            public readonly string Description;
            public readonly object[] Args;
            public readonly string TraceDescription;
            public readonly object[] TraceArgs;
            public readonly LogLevel TraceLogLevel;

            public bool LogTrace() => TraceLogLevel != null;
        }

        #endregion
    }
}