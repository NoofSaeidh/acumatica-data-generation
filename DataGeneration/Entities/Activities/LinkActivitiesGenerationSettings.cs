using DataGeneration.Core;
using DataGeneration.Core.Api;
using DataGeneration.Core.Cache;
using DataGeneration.Core.Common;
using DataGeneration.Core.Queueing;
using DataGeneration.Core.Settings;
using DataGeneration.Soap;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.Entities.Activities
{
    public class LinkActivitiesGenerationSettings :
        GenerationSettings<OneToManyRelation<LinkEntityToActivity, Activity>, LinkActivitiesRandomizerSettings>,
        ISearchUtilizer,
        IHasCacheAdapter
    {
        public override GenerationRunner GetGenerationRunner(ApiConnectionConfig apiConnectionConfig) => new LinkActivitiesGenerationRunner(apiConnectionConfig, this);

        [Required]
        public SearchPattern SearchPattern { get; set; }
        public CacheAdapter CacheAdapter => new CacheAdapter();

        public override void Validate()
        {
            base.Validate();
            ValidateHelper.ValidateObject(SearchPattern);
        }
    }
}
