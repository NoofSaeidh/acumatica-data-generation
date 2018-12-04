using System;

namespace DataGeneration.Core
{
    public class GenerationException : Exception
    {
        public GenerationException()
        {
        }

        public GenerationException(string message) : base(message)
        {
        }

        public GenerationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public static GenerationException NewFromEntityType<TEntity>(Exception innerException)
        {
            return new GenerationException($"Generation {typeof(TEntity)} failed.", innerException);
        }
    }
}