using CrmDataGeneration.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using VoidTask = System.Threading.Tasks.Task;

namespace CrmDataGeneration.Wsdl
{
    public class AcumaticaClient : IDisposable
    {
        private readonly DefaultSoapClient _client;

        static AcumaticaClient()
        {
            // otherwise all soap clients will use same settings
            ClientBase<DefaultSoapClient>.CacheSetting = CacheSetting.AlwaysOff;
        }

        public AcumaticaClient(DefaultSoapClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public AcumaticaClient(EndpointSettings endpointSettings)
        {
            // todo: intiialization through EndpointSettings
            _client = new DefaultSoapClient();
        }

        public static AcumaticaClient LoginLogoutClient(ApiSessionConfig sessionConfig)
        {
            if (sessionConfig == null)
            {
                throw new ArgumentNullException(nameof(sessionConfig));
            }

            return new LoginLogoutClientImpl(sessionConfig);
        }

        public void Login(LoginInfo loginInfo)
        {
            if (loginInfo == null)
            {
                throw new ArgumentNullException(nameof(loginInfo));
            }

            TryCatch(() => _client.Login(loginInfo.Username, loginInfo.Password, loginInfo.Company, loginInfo.Branch, loginInfo.Locale));
        }

        public async VoidTask LoginAsync(string name, string password, string company = null, string branch = null, string locale = null)
        {
            await TryCatchAsync(_client.LoginAsync(name, password, company, branch, locale));
        }

        public void Logout()
        {
            TryCatch(() => _client.Logout());
        }

        public async VoidTask LogoutAsync()
        {
            await TryCatchAsync(_client.LogoutAsync());
        }

        public T Get<T>(T whereEntity) where T : Entity
        {
            return TryCatch(() => _client.Get(whereEntity));
        }

        public async Task<T> GetAsync<T>(T whereEntity) where T : Entity
        {
            return await TryCatchAsync(_client.GetAsync(whereEntity));
        }

        public IList<T> GetList<T>(T whereEntity) where T : Entity
        {
            return TryCatch(() => _client.GetList(whereEntity));
        }

        public async Task<IList<T>> GetListAsync<T>(T whereEntity) where T : Entity
        {
            return await TryCatchAsync(_client.GetListAsync(whereEntity));
        }

        public T Put<T>(T entity) where T : Entity
        {
            return TryCatch(() => _client.Put(entity));
        }

        public async Task<T> PutAsync<T>(T entity) where T : Entity
        {
            return await TryCatchAsync(_client.PutAsync(entity));
        }

        public void Delete(Entity entity)
        {
            TryCatch(() => _client.Delete(entity));
        }

        public async VoidTask DeleteAsync<T>(T entity) where T : Entity
        {
            await TryCatchAsync(_client.DeleteAsync(entity));
        }

        public void Invoke(Entity entity, Action action)
        {
            TryCatch(() => _client.Invoke(entity, action));
        }

        public async VoidTask InvokeAsync(Entity entity, Action action)
        {
            await TryCatchAsync(_client.InvokeAsync(entity, action));
        }

        public void Dispose()
        {
            ((IDisposable)_client).Dispose();
        }

        private T TryCatch<T>(Func<T> action)
        {
            try
            {
                return action();
            }
            catch (Exception e)
            {
                throw new AcumaticaException(null, e);
            }
        }
        private void TryCatch(System.Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                throw new AcumaticaException(null, e);
            }
        }
        private async Task<T> TryCatchAsync<T>(Task<T> task)
        {
            try
            {
                return await task;
            }
            catch (Exception e)
            {
                throw new AcumaticaException(null, e);
            }
        }
        private async VoidTask TryCatchAsync(VoidTask task)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                throw new AcumaticaException(null, e);
            }
        }

        private class LoginLogoutClientImpl : AcumaticaClient, IDisposable
        {
            public LoginLogoutClientImpl(ApiSessionConfig info) : base(info.EndpointSettings)
            {
                Login(info.LoginInfo);
            }
            void IDisposable.Dispose()
            {
                Logout();
                base.Dispose();
            }
        }
    }
}
