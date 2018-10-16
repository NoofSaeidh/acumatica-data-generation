using DataGeneration.Soap;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VoidTask = System.Threading.Tasks.Task;

namespace DataGeneration.Common
{
    /// <summary>
    ///     Randomizer that can generate entities.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDataGenerator<T>
    {
        T Generate();
        IList<T> GenerateList(int count);
        // lazy endless enumeration. you need to call .Take(count) method
        IEnumerable<T> GenerateEnumeration();
    }

    /// <summary>
    ///     Settings for randomization.
    /// Implement this interface, define all needed properties and write <see cref="GetDataGenerator()"/>
    /// to return configured randomizer that generate entities depending on class properties.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRandomizerSettings<T> : IValidatable
    {
        int Seed { get; set; }
        IDataGenerator<T> GetDataGenerator();
    }

    public interface IValueWrapper<T>
    {
        T Value { get; set; }
    }

    public interface IEntity
    {
        Guid? Id { get; set; }
    }

    public interface IGenerationSettings
    {
        int Count { get; set; }
        string GenerationEntity { get; }

        // get, set seed for randomizer settings
        int? Seed { get; set; }

        ExecutionTypeSettings ExecutionTypeSettings { get; set; }

        GenerationRunner GetGenerationRunner(ApiConnectionConfig apiConnectionConfig);
    }

    public interface IGenerationSettings<T> : IGenerationSettings, IValidatable
    {
        IRandomizerSettings<T> RandomizerSettings { get; }
    }

    public interface IValidatable
    {
        void Validate();
    }

    public interface IStopwatchLogger
    {
        IStopwatchLogger Log(string description, params object[] args);
        IStopwatchLogger Start();
        IStopwatchLogger Stop();
        IStopwatchLogger Reset();
        IStopwatchLogger Restart();
    }

    public interface IProbabilityObject
    {
        decimal? Probability { get; }
    }

    public interface IApiClient
    {
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
    }

    // just indicates that client will autologout in dispose

    public interface ILogoutApiClient : IApiClient, IDisposable
    {
    }

    // just indicates that client will autologin in ctor and autologout in dispose
    public interface ILoginLogoutApiClient : ILogoutApiClient, IDisposable
    {
    }
}