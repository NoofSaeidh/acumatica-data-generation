using Bogus;
using CrmDataGeneration.Soap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VoidTask = System.Threading.Tasks.Task;

namespace CrmDataGeneration.Common
{
    /// <summary>
    ///     Randomizer that can generate entities.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDataGenerator<T> where T : IEntity
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
    public interface IRandomizerSettings<T>: IValidatable where T : IEntity
    {
        int Seed { get; }
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
        int Count { get; }
        string GenerationEntity { get; }
        ExecutionTypeSettings ExecutionTypeSettings { get; }

        GenerationRunner GetGenerationRunner(ApiConnectionConfig apiConnectionConfig);
    }

    public interface IGenerationSettings<T> : IGenerationSettings, IValidatable where T : IEntity
    {
        IRandomizerSettings<T> RandomizerSettings { get; }
    }

    public interface IValidatable
    {
        void Validate();
    }
}
