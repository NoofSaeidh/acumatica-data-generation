using DataGeneration.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.GenerationInfo
{
    public class GenerationSubscriptionManager
    {
        public EventHandler<RunBeforeGenerationStartedEventArgs> RunBeforeGenerationStarted { get; set; }
        public EventHandler<RunGenerationStartedEventArgs> RunGenerationStarted { get; set; }
        public EventHandler<RunGenerationCompletedEventArgs> RunGenerationCompleted { get; set; }

        public void SubscribeGenerationRunner(GenerationRunner runner)
        {
            if (runner == null)
                throw new ArgumentNullException(nameof(runner));

            runner.RunBeforeGenerationStarted += RunBeforeGenerationStarted;
            runner.RunGenerationStarted += RunGenerationStarted;
            runner.RunGenerationCompleted += RunGenerationCompleted;
        }
    }
}
