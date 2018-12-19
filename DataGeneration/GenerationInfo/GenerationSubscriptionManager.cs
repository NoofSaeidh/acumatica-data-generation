using DataGeneration.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.GenerationInfo
{
    public class GenerationSubscriptionManager
    {
        public GenerationSubscriptionManager()
        {
            RunBeforeGenerationStarted = new List<EventHandler<RunBeforeGenerationStartedEventArgs>>();
            RunGenerationStarted = new List<EventHandler<RunGenerationStartedEventArgs>>();
            RunGenerationCompleted = new List<EventHandler<RunGenerationCompletedEventArgs>>();
        }

        public List<EventHandler<RunBeforeGenerationStartedEventArgs>> RunBeforeGenerationStarted { get; }
        public List<EventHandler<RunGenerationStartedEventArgs>> RunGenerationStarted { get; }
        public List<EventHandler<RunGenerationCompletedEventArgs>> RunGenerationCompleted { get; }

        public void SubscribeGenerationRunner(GenerationRunner runner)
        {
            if (runner == null)
                throw new ArgumentNullException(nameof(runner));

            RunBeforeGenerationStarted.ForEach(e => runner.RunBeforeGenerationStarted += e);
            RunGenerationStarted.ForEach(e => runner.RunGenerationStarted += e);
            RunGenerationCompleted.ForEach(e => runner.RunGenerationCompleted += e);
        }

        public void Add(
            EventHandler<RunBeforeGenerationStartedEventArgs> beforeStarted = null,
            EventHandler<RunGenerationStartedEventArgs> started = null,
            EventHandler<RunGenerationCompletedEventArgs> completed = null)
        {
            if (beforeStarted != null) RunBeforeGenerationStarted.Add(beforeStarted);
            if (started != null) RunGenerationStarted.Add(started);
            if (completed != null) RunGenerationCompleted.Add(completed);
        }
    }
}
