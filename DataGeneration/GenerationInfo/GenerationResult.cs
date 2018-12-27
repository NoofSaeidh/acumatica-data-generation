using DataGeneration.Core;
using DataGeneration.Core.Settings;
using System;

namespace DataGeneration.GenerationInfo
{
    public class GenerationResult
    {
        internal GenerationResult(IGenerationSettings settings, Exception exception = null)
        {
            Id = settings.Id;
            GenerationType = settings.GenerationType;
            Exception = exception;
        }

        public int Id { get; }
        public string GenerationType { get; }
        public Exception Exception { get; }
        public bool Success => Exception == null;
    }
}