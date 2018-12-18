using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using DataGeneration.Core.Common;
using DataGeneration.Core.DataGeneration;
using DataGeneration.Core.Settings;
using DataGeneration.Soap;
using Newtonsoft.Json;

namespace DataGeneration.Entities.Leads
{
    public class LeadConvertRandomizerSettings : RandomizerSettings<EntityWrapper<int>>, IAvailableCountLimit
    {
        [Required]
        public ProbabilityCollection<string> ConvertProbabilitiesByStatus { get; set; }

        public int? AvailableCount => Leads == null ? null : (GetStatefullDataGenerator() as IAvailableCountLimit)?.AvailableCount;

        [JsonIgnore]
        public ICollection<(int key, string status)> Leads { get; set; }

        public override IDataGenerator<EntityWrapper<int>> GetStatelessDataGenerator()
        {
            Validate();

            if(Leads == null)
                throw new InvalidOperationException("Leads must be initialized.");

            var rand = GetRandomizer();

            var collection = Leads
                .Select(l =>
                {
                    if (!ConvertProbabilitiesByStatus.TryGetValue(l.status, out var probability)
                        || !rand.Bool((float) probability))
                        return null;
                    return new EntityWrapper<int>(l.key);
                })
                .Where(l => l != null);

            return new ConsumerCollectionDataGenerator<EntityWrapper<int>>(
                new ConcurrentQueue<EntityWrapper<int>>(collection));
        }
    }
}
