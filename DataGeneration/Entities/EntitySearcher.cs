using DataGeneration.Common;
using DataGeneration.Soap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataGeneration.Entities
{
    // lazy sorting and filtering
    public class EntitySearcher
    {
        private readonly Func<Entity, CancellationToken, Task<IEnumerable<Entity>>> _getListFactory;
        private readonly List<Action<Adjuster<Entity>>> _inputAdjustment;
        private readonly List<Action<EnumerableAdjuster<Entity>>> _outputAdjustment;
        private readonly Func<Entity> _entityFactory;

        public EntitySearcher(Func<Entity, CancellationToken, Task<IEnumerable<Entity>>> getListFactory, Func<Entity> entityFactory)
        {
            _getListFactory = getListFactory ?? throw new ArgumentNullException(nameof(getListFactory));
            _entityFactory = entityFactory ?? throw new ArgumentNullException(nameof(entityFactory));
            _inputAdjustment = new List<Action<Adjuster<Entity>>>();
            _outputAdjustment = new List<Action<EnumerableAdjuster<Entity>>>();
        }

        public EntitySearcher AdjustInput(Action<Adjuster<Entity>> adjustment)
        {
            if (adjustment == null) throw new ArgumentNullException(nameof(adjustment));
            _inputAdjustment.Add(adjustment);
            return this;
        }

        public EntitySearcher AdjustOutput(Action<EnumerableAdjuster<Entity>> adjustment)
        {
            if (adjustment == null) throw new ArgumentNullException(nameof(adjustment));
            _outputAdjustment.Add(adjustment);
            return this;
        }

        public async Task<IList<Entity>> Execute(CancellationToken ct = default)
        {
            var adjEntity = _entityFactory().GetAdjuster();
            _inputAdjustment.ForEach(action => action(adjEntity));
            IEnumerable<Entity> result = await _getListFactory(adjEntity.Value, ct);
            _outputAdjustment.ForEach(action =>
            {
                var adj = result.GetAdjuster();
                action(adj);
                result = adj.Value;
            });
            return result.ToArray();
        }
    }
}
