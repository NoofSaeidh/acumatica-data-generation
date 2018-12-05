using DataGeneration.Core;
using DataGeneration.Core.Settings;
using System;

namespace DataGeneration.GenerationInfo
{
    public class GenerationResult
    {
        internal GenerationResult(IGenerationSettings generationSettings, Exception exception = null)
        {
            GenerationSettings = generationSettings;
            Exception = exception;
        }

        public IGenerationSettings GenerationSettings { get; }
        public Exception Exception { get; }
        public bool Success => Exception == null;
    }
}