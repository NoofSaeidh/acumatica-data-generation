using System;

namespace DataGeneration.Core.Cache
{
    [Flags]
    public enum CacheSettingsType : byte
    {
        None    = 0b0000,
        Save    = 0b0001,
        Read    = 0b0010,
        Delete  = 0b0100,
    }
}