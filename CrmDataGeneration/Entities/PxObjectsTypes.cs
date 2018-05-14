using CrmDataGeneration.OpenApi.Reference;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmDataGeneration.Entities
{
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

        public static string GetEntityPxTypeName(this Entity entity)
        {
            if (PxTypes.TryGetValue(entity.GetType(), out var result))
                return result;
            throw new NotSupportedException();
        }

        public static string GetEntityPxTypeName<T>() where T : Entity
        {
            if (PxTypes.TryGetValue(typeof(T), out var result))
                return result;
            throw new NotSupportedException();
        }
    }
}
