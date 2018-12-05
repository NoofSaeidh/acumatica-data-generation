using System;
using System.Collections.Generic;
using System.Linq;
using DataGeneration.Soap;

namespace DataGeneration.Core.Queueing
{
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
}
