using DataGeneration.Common;
using DataGeneration.Soap;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DataGeneration.Entities.Leads
{
    public class LeadToOpportunityRandomizerSettings : RandomizerSettings<Lead>
    {
        [RequiredCollection(AllowEmpty = false)]
        public ProbabilityCollection<bool> ConvertProbability { get; set; }


        private IEnumerable<Lead> _leads;
        private Lead[] _newLeads;
        // injected
        [Required]
        [JsonIgnore]
        public IEnumerable<Lead> Leads
        {
            get => _leads;
            set
            {
                _leads = value;
                _newLeads = null;
            }
        }

        [JsonIgnore]
        public IList<Lead> NewLeads
        {
            get
            {
                if (_newLeads != null)
                    return _newLeads;

                Validate();
                var rand = GetRandomizer();
                return _newLeads = Leads.Where(l => rand.ProbabilityRandom(ConvertProbability)).ToArray();
            }
        }

        public override IDataGenerator<Lead> GetStatelessDataGenerator()
        {
            return new SourceCollectionDataGenerator<Lead>(NewLeads);
        }
    }
}
