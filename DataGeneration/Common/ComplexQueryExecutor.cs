using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataGeneration.Soap;
using Task = System.Threading.Tasks.Task;

namespace DataGeneration.Common
{
    public class ComplexQueryExecutor
    {
        private readonly Func<Entity, CancellationToken, Task<IEnumerable<Entity>>> _getListFactory;

        public ComplexQueryExecutor(Func<Entity, CancellationToken, Task<IEnumerable<Entity>>> getListFactory)
        {
            _getListFactory = getListFactory ?? throw new ArgumentNullException(nameof(getListFactory));
        }

        public async Task Execute(IEnumerable<IComplexQueryEntity> entities, CancellationToken ct)
        {
            if(entities == null)
                throw new ArgumentNullException(nameof(entities));

            var grouping = entities.Where(e => !(e is null)).ToList().GroupBy(e => e.QueryRequestor);

            foreach (var group in grouping)
            {
                // arrange
                var query = new ComplexQuery(group.Key);

                if(group.Key.RequestType == RequestType.PerType)
                {
                    group.First().AdjustComplexQuery(query);
                }
                else
                {
                    foreach (var entity in group)
                    {
                        entity.AdjustComplexQuery(query);
                    }
                }

                // act
                var result = new ComplexQueryResult(query);
                foreach (var queryEntity in query)
                {
                    result.AddRange(await _getListFactory(queryEntity, ct));
                }

                // utilize
                foreach (var entity in group)
                {
                    entity.UtilizeComplexQueryResult(result);
                }
            }
        }
    }

    public class ComplexQuery : List<Entity>
    {
        public ComplexQuery(QueryRequestor requestor) : this(requestor, Enumerable.Empty<Entity>())
        {
        }

        public ComplexQuery(QueryRequestor requestor, params Entity[] entities) : this(requestor, (IEnumerable<Entity>)entities)
        {
        }

        public ComplexQuery(QueryRequestor requestor, IEnumerable<Entity> entities) : base(entities)
        {
            Requestor = requestor ?? throw new ArgumentNullException(nameof(requestor));
        }

        public QueryRequestor Requestor { get; }
    }

    public class ComplexQueryResult : List<Entity>
    {
        public ComplexQueryResult(ComplexQuery query) : this(query, Enumerable.Empty<Entity>())
        {
        }

        public ComplexQueryResult(ComplexQuery query, params Entity[] entities) : this(query, (IEnumerable<Entity>)entities)
        {
        }

        public ComplexQueryResult(ComplexQuery query, IEnumerable<Entity> entities) : base(entities)
        {
            Query = query ?? throw new ArgumentNullException(nameof(query));
        }

        public ComplexQuery Query { get; }
        public QueryRequestor Requestor => Query.Requestor;
    }

    public class QueryRequestor : IEquatable<QueryRequestor>
    {
        public QueryRequestor(Type type, Guid guid, RequestType requestType = RequestType.PerType)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Guid = guid;
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

    public enum RequestType : byte
    {
        // call AdjustComplexQuery only one time per type
        PerType = 0,
        // call AdjustComplexQuery per every instance
        PerInstance = 1
    }
}
