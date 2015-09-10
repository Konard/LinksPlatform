using System;
using System.IO;

namespace Platform.Sandbox
{
    unsafe class Program
    {
        static void Main(string[] args)
        {
            //Sequences.TestSimplify();
            //WrongPointerTest();
            //UDPTest();
            //Links();
        }

        private static void WrongPointerTest()
        {
            try
            {
                var array = new byte[256];

                fixed (byte* pointer = array)
                {
                    var value = *(pointer - 259);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private static void Links()
        {
            var filename = (new Random(2)).Next(1, 99999).ToString();

            //if (File.Exists(filename))
            //    File.Delete(filename);

            //ConceptTest.TestGexf(filename);

            if (File.Exists(filename))
                File.Delete(filename);

            Console.ReadKey();
        }
    }
}