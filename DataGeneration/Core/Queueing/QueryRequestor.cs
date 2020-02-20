using System;
using System.Collections.Generic;

namespace DataGeneration.Core.Queueing
{
    public class QueryRequestor : IEquatable<QueryRequestor>
    {
        public QueryRequestor(Type type, Guid guid, RequestType requestType = RequestType.PerType)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Guid = guid;
            RequestType = requestType;
        }

        public Type Type { get; }
        public Guid Guid { get; }

        // doesn't involved in grouping (in equals) so must be the same for all instances
        // otherwise only the first be used in querying
        public RequestType RequestType { get; }

        public override bool Equals(object obj)
        {
            return Equals(obj as QueryRequestor);
        }

        public bool Equals(QueryRequestor other)
        {
            return other != null &&
                   EqualityComparer<Type>.Default.Equals(Type, other.Type) &&
                   Guid.Equals(other.Guid);
        }

        public override int GetHashCode()
        {
            var hashCode = 652959137;
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(Type);
            hashCode = hashCode * -1521134295 + EqualityComparer<Guid>.Default.GetHashCode(Guid);
            return hashCode;
        }

        public static bool operator ==(QueryRequestor requestor1, QueryRequestor requestor2) => EqualityComparer<QueryRequestor>.Default.Equals(requestor1, requestor2);
        public static bool operator !=(QueryRequestor requestor1, QueryRequestor requestor2) => !(requestor1 == requestor2);
    }
}
