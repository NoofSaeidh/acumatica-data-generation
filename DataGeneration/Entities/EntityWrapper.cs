using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.Entities
{
    // to save memory
    public class EntityWrapper<T>
    {
        public EntityWrapper(T key)
        {
            Key = key;
        }

        public T Key { get; }
    }
}
