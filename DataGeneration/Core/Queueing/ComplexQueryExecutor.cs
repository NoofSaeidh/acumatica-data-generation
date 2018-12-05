using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataGeneration.Soap;
using Task = System.Threading.Tasks.Task;

namespace DataGeneration.Core.Queueing
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
}
