using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataGeneration.Core.Queueing;
using DataGeneration.GenerationInfo;
using DataGeneration.Soap;

namespace DataGeneration.Core.Settings
{
    public abstract class SearchGenerationSettings<T, TRandomizerSettings> :
        GenerationSettings<T, TRandomizerSettings>,
        ISearchUtilizer,
        IBatchDependent
            where T : class
            where TRandomizerSettings : IRandomizerSettings<T>
    {
        [ConditionRequired(nameof(SearchPatternRequired))]
        public SearchPattern SearchPattern { get; set; }

        public virtual bool SearchPatternRequired => true;

        public virtual void Inject(Batch batch)
        {
            if (batch == null)
                throw new ArgumentNullException(nameof(batch));

            if (SearchPattern?.CreatedDate == null) return;

            // by default Inject prop in this class is false,
            // so no need to create, if it is null
            SearchPattern.CreatedDate.Injected = new DateTimeSearch
            {
                Condition = DateTimeCondition.IsBetween,
                Value = batch.StartTime,
                Value2 = batch.HasCompletedIterations ? batch.IterationEndTime : DateTime.Now
            };
        }


        public override void Validate()
        {
            base.Validate();
            if(SearchPattern != null) ValidateHelper.ValidateObject(SearchPattern);
        }
    }
}
