using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CrmDataGeneration.Common
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
