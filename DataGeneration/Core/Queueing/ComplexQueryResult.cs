using System;
using System.Collections.Generic;
using System.Linq;
using DataGeneration.Soap;

namespace DataGeneration.Core.Queueing
{
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
}
