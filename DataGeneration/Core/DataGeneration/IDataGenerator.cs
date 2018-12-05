using System.Collections.Generic;

namespace DataGeneration.Core.DataGeneration
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
}