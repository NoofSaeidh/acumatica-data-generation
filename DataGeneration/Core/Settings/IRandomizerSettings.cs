using DataGeneration.Core.Common;
using DataGeneration.Core.DataGeneration;

namespace DataGeneration.Core.Settings
{
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
}