using DataGeneration.Soap;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DataGeneration.Entities
{
    // todo: move it to config
    public static class PxObjectsTypes
    {
        public const string ContactType = "PX.Objects.CR.Contact";
        public const string LeadType = ContactType;

        public static readonly IReadOnlyDictionary<Type, string> PxTypes = new ReadOnlyDictionary<Type, string>(
            new Dictionary<Type, string>
            {
                {typeof(Lead), LeadType },
                {typeof(Contact), ContactType }
            });

        public static string GetPxTypeName(this Entity entity)
        {
            if (PxTypes.TryGetValue(entity.GetType(), out var result))
                return result;
            throw null;
        }

        public static string GetPxTypeName<T>() where T : Entity
        {
            if (PxTypes.TryGetValue(typeof(T), out var result))
                return result;
            return null;
        }
    }
}