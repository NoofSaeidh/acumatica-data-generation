using System;

namespace DataGeneration.Entities.Leads
{
    [Flags]
    public enum ConvertLeadFlags
    {
        DontConvert = 0,
        ToOpportunity = 1
    }
}