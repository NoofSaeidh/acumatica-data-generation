using CrmDataGeneration.OpenApi;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmDataGeneration.Core
{
    public class GeneratorClient : IDisposable
    {
        private readonly OpenApiState _openApiState;
        private readonly OpenApiBaseClient _loginClient;
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public GeneratorClient(GeneratorConfig config)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            if (Config.OpenApiSettings == null) throw new ArgumentException($"Property {nameof(Config.OpenApiSettings)}" +
                 $" of argument {nameof(config)} must not be null.");

            _openApiState = new OpenApiState(Config.OpenApiSettings);
            _loginClient = new OpenApiBaseClient(_openApiState);
        }

        public GeneratorConfig Config { get; }

        public T GetApiClient<T>() where T : OpenApiBaseClient
        {
            try
            {
                var result = (T)Activator.CreateInstance(typeof(T), _openApiState);
                logger.Debug($"Api client of type {typeof(T).Name} created.", result);
                return result;
            }
            catch (Exception e)
            {
                logger.Error(e, $"Cannot create Api client of type {typeof(T).Name}.");
                throw;
            }
        }

        public async Task Login() => await _loginClient.Login();

        public async Task Logout() => await _loginClient.Logout();

        void IDisposable.Dispose()
        {
            ((IDisposable)_openApiState).Dispose();
        }
    }
}
