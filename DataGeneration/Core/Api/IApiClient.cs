﻿using DataGeneration.Core.Api;
using DataGeneration.Soap;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VoidTask = System.Threading.Tasks.Task;

namespace DataGeneration.Core.Api
{
    public interface IApiClient
    {
        int RetryCount { get; set; }
        void Delete<T>(T whereEntity) where T : Entity;
        VoidTask DeleteAsync<T>(T whereEntity) where T : Entity;
        VoidTask DeleteAsync<T>(T whereEntity, CancellationToken cancellationToken) where T : Entity;
        T Get<T>(T whereEntity) where T : Entity;
        Task<T> GetAsync<T>(T whereEntity) where T : Entity;
        Task<T> GetAsync<T>(T whereEntity, CancellationToken cancellationToken) where T : Entity;
        IList<T> GetList<T>(T whereEntity) where T : Entity;
        Task<IList<T>> GetListAsync<T>(T whereEntity) where T : Entity;
        Task<IList<T>> GetListAsync<T>(T whereEntity, CancellationToken cancellationToken) where T : Entity;
        void Invoke<TEntity, TAction>(TEntity entity, TAction action)
            where TEntity : Entity
            where TAction : Soap.Action;
        VoidTask InvokeAsync<TEntity, TAction>(TEntity entity, TAction action)
            where TEntity : Entity
            where TAction : Soap.Action;
        VoidTask InvokeAsync<TEntity, TAction>(TEntity entity, TAction action, CancellationToken cancellationToken)
            where TEntity : Entity
            where TAction : Soap.Action;
        void Login(LoginInfo loginInfo);
        void Login(string name, string password, string company = null, string branch = null, string locale = null);
        VoidTask LoginAsync(LoginInfo loginInfo);
        VoidTask LoginAsync(LoginInfo loginInfo, CancellationToken cancellationToken);
        // no sense to do 2 methods with cancellation token and optional arguments
        //VoidTask LoginAsync(string name, string password, string company = null, string branch = null, string locale = null);
        VoidTask LoginAsync(string name, string password, string company = null, string branch = null, string locale = null, CancellationToken cancellationToken = default);
        void Logout();
        VoidTask LogoutAsync();
        T Put<T>(T entity) where T : Entity;
        Task<T> PutAsync<T>(T entity) where T : Entity;
        Task<T> PutAsync<T>(T entity, CancellationToken cancellationToken) where T : Entity;
        IList<File> GetFiles<T>(T entity) where T : Entity;
        Task<IList<File>> GetFilesAsync<T>(T entity) where T : Entity;
        Task<IList<File>> GetFilesAsync<T>(T entity, CancellationToken cancellationToken) where T : Entity;
        void PutFiles<T>(T entity, IEnumerable<File> files) where T : Entity;
        VoidTask PutFilesAsync<T>(T entity, IEnumerable<File> files) where T : Entity;
        VoidTask PutFilesAsync<T>(T entity, IEnumerable<File> files, CancellationToken cancellationToken) where T : Entity;
    }


    // just indicates that client will autologout in dispose
    public interface ILogoutApiClient : IApiClient, IDisposable
    {
    }
}