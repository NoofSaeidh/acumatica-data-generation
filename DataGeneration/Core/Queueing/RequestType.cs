namespace DataGeneration.Core.Queueing
{
    public enum RequestType : byte
    {
        // call AdjustComplexQuery only one time per type
        PerType = 0,
        // call AdjustComplexQuery per every instance
        PerInstance = 1
    }
}
