using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Cosmos.TestRunner;

using Sys = Cosmos.System;

namespace Cosmos.Kernel.Tests.System.IO
{
    public delegate int TestDelegate(Kernel k);

    public class Kernel
        : Sys.Kernel
    {
        // Invoke them manually, as reflection is not (yet) provided
        internal static readonly TestDelegate[] functions = new TestDelegate[] {

        };

        protected override void BeforeRun()
        {
            Console.WriteLine("Testing Operating System for the namespace '[mscorlib.dll]global::System.IO.*'");
        }

        protected override void Run()
        {
            try
            {
                for (int i = 0, l = functions.Length; i < l; i++)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"Testing method {i + 1}/{l + 1} ...");
                    Console.ForegroundColor = ConsoleColor.White;

                    int hresult = functions[i].Invoke(this);

                    if (hresult != 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Method Test {i + 1}/{l + 1} failed with the return code {hresult}.");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Method Test {i + 1}/{l + 1} succesful.");
                    }
                }
            }
            catch (Exception ex)
            {
                mDebugger.Send("Exception occurred: " + ex.Message);

                TestController.Failed();
            }
        }
    }

    public static class TestMethods
    {

    }
}
