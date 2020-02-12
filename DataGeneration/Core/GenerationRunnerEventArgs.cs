using DataGeneration.Core.Settings;
using System;

namespace DataGeneration.Core
{
    public abstract class GenerationRunnerEventArgs : EventArgs
    {
        public GenerationRunnerEventArgs(IGenerationSettings generationSettings, string message = null)
        {
            GenerationSettings = generationSettings;
            Message = message;
        }

        public IGenerationSettings GenerationSettings { get; }
        public string Message { get; }
    }

    public class RunBeforeGenerationStartedEventArgs : GenerationRunnerEventArgs
    {
        public RunBeforeGenerationStartedEventArgs(IGenerationSettings generationSettings, string message = null) : base(generationSettings, message)
        {
        }
    }

    public class RunGenerationStartedEventArgs : GenerationRunnerEventArgs
    {
        public RunGenerationStartedEventArgs(IGenerationSettings generationSettings, string message = null) : base(generationSettings, message)
        {
        }
    }

    public class RunGenerationCompletedEventArgs : GenerationRunnerEventArgs
    {
        public RunGenerationCompletedEventArgs(IGenerationSettings generationSettings, string message = null) : base(generationSettings, message)
        {
        }
    }

    public class RunAfterGenerationStartedEventArgs : GenerationRunnerEventArgs
    {
        public RunAfterGenerationStartedEventArgs(IGenerationSettings generationSettings, string message = null) : base(generationSettings, message)
        {
        }
    }

    public class GenerationCompletedEventArgs : GenerationRunnerEventArgs
    {
        public GenerationCompletedEventArgs(IGenerationSettings generationSettings, string message = null) : base(generationSettings, message)
        {
        }
    }
}