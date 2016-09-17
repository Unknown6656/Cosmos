using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;

namespace Cosmos.Kernel.Tests.System
{
    public class Kernel
        : Sys.Kernel
    {
        protected Random rand;

        protected override void BeforeRun()
        {
            rand = new Random();
        }

        protected override void Run()
        {
            int next = rand.Next();

            Console.WriteLine(next);
            Console.ReadKey(true);
        }
    }
}
