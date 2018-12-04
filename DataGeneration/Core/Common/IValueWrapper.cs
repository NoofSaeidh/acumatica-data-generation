namespace DataGeneration.Core.Common
{
    public interface IValueWrapper<T>
    {
        T Value { get; set; }
    }
}