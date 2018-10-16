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
        private readonly Func<Task<ILoginLogoutApiClient>> _clientFactory;
        private readonly List<Action<Adjuster<Entity>>> _inputAdjustment;
        private readonly List<Action<EnumerableAdjuster<Entity>>> _outputAdjustment;
        private Func<Entity> _entityFactory;

        public EntitySearcher(Func<Task<ILoginLogoutApiClient>> clientFactory)
        {
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            _inputAdjustment = new List<Action<Adjuster<Entity>>>();
            _outputAdjustment = new List<Action<EnumerableAdjuster<Entity>>>();
        }

        public EntitySearcher EntityFactory(Func<Entity> factory)
        {
            _entityFactory = factory ?? throw new ArgumentNullException(nameof(factory));
            return this;
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
            if (_entityFactory == null)
                throw new InvalidOperationException("Entity factory must not be null. Call EntityFactory method before Execute.");

            var adjEntity = _entityFactory().Adjust();
            _inputAdjustment.ForEach(action => action(adjEntity));
            IEnumerable<Entity> result;
            using (var client = await _clientFactory())
            {
                result = await client.GetListAsync(adjEntity.Value, ct);
            }
            _outputAdjustment.ForEach(action =>
            {
                var adj = result.Adjust();
                action(adj);
                result = adj.Value;
            });
            return result.ToArray();
        }
    }
}
