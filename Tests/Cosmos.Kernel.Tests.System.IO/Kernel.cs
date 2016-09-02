using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Cosmos.TestRunner;

using static System.ConsoleColor;

using Sys = Cosmos.System;

namespace Cosmos.Kernel.Tests.System.IO
{
    public unsafe delegate void TestDelegate(Kernel k, long* hresult);

    public unsafe class Kernel
        : Sys.Kernel
    {
        // Invoke them manually, as reflection is not (yet) provided
        internal static readonly TestDelegate[] functions = new TestDelegate[] {
            TestMethods.BinaryStreamTest
        };

        
        protected override void BeforeRun()
        {
            Console.WriteLine("Testing Operating System for the namespace '[mscorlib.dll]global::System.IO.*'");
        }

        protected override void Run()
        {   
            Console.ForegroundColor = White;
            Console.WriteLine("\nPress (T) to start the tests or any other key to shut down the machine.");

            char c = Console.ReadKey(true).KeyChar;

            if (c == 't' || c == 'T')
                try
                {
                    long hresult = 0;
                    int total_err = 0;
                    int l = functions.Length;

                    for (int i = 0; i < l; i++)
                    {
                        Console.ForegroundColor = Cyan;
                        Console.WriteLine($"Testing method {i + 1}/{l} ...");
                        Console.ForegroundColor = White;

                        hresult = 0;
                        functions[i].Invoke(this, &hresult);

                        if (hresult != 0)
                        {
                            Console.ForegroundColor = Red;
                            Console.WriteLine($"\nMethod Test {i + 1}/{l} failed with the return code {hresult} (0x{hresult:x16}).");

                            total_err++;
                        }
                        else
                        {
                            Console.ForegroundColor = Green;
                            Console.WriteLine($"\nMethod Test {i + 1}/{l} succesful.");
                        }
                    }

                    Console.ForegroundColor = total_err == 0 ? Green : Red;
                    Console.WriteLine($"{l - total_err}/{l} Tests have been successfull, {total_err} Errors occured.");

                    if (total_err > 0)
                        TestController.Failed();
                }
                catch (Exception ex)
                {
                    mDebugger.Send("Exception occurred: " + ex.Message);

                    Console.ForegroundColor = Red;
                    Console.WriteLine("\nFatal error occured.");

                    TestController.Failed();
                }
            else
                this.Stop();
        }
    }

    public static unsafe class TestMethods
    {
        /// <summary>
        /// Tests the classes "System::IO::BinaryReader" and "System::IO::BinaryWriter"
        /// </summary>
        public static void BinaryStreamTest(Kernel k, long* hresult)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryReader rd = new BinaryReader(ms))
            using (BinaryWriter rw = new BinaryWriter(ms))
            {
                // write methods
                rw.Write(true);
                rw.Write((byte)0x42);
                rw.Write('U');
                rw.Write("Unknown6656");
                rw.Write(4.2m);
                rw.Write(4.2f);
                rw.Write(4.2d);
                rw.Write(42);
                rw.Write(42U);
                rw.Write(42L);
                rw.Write(42UL);
                rw.Write(new byte[3] { 0x42, 0x31, 0x56 });
                
                long flag = 1L;
                Action<bool> writeresult = new Action<bool>(_ => *hresult |= _ ? 0 : (flag <<= 1));

                // read methods
                writeresult(rd.ReadBoolean());
                writeresult(rd.ReadByte() == 0x42);
                writeresult(rd.ReadChar() == 'U');
                writeresult(rd.ReadString() == "Unknown6656");
                writeresult(rd.ReadDecimal() == 4.2m);
                writeresult(rd.ReadSingle() == 4.2f);
                writeresult(rd.ReadDouble() == 4.2d);
                writeresult(rd.ReadInt32() == 42);
                writeresult(rd.ReadUInt32() == 42U);
                writeresult(rd.ReadInt64() == 42L);
                writeresult(rd.ReadUInt64() == 42UL);

                byte[] arr = rd.ReadBytes(3);

                writeresult((arr[0] == 0x42) &&
                            (arr[1] == 0x31) &&
                            (arr[2] == 0x56));
            }
        }
    }
}
