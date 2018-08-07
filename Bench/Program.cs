using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bench
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<Soap>();
        }
    }
}
