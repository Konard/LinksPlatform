using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Platform.Sandbox
{
    public class NewMemoryManagerExperiments
    {
        [DllImport("Platform.Links.DataBase.Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void InitPersistentMemoryManager();

        [DllImport("Platform.Links.DataBase.Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong OpenStorageFile(string filename);

        [DllImport("Platform.Links.DataBase.Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong CloseStorageFile();

        [DllImport("Platform.Links.DataBase.Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong EnlargeStorageFile();

        [DllImport("Platform.Links.DataBase.Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong ShrinkStorageFile();

        [DllImport("Platform.Links.DataBase.Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong SetStorageFileMemoryMapping();

        [DllImport("Platform.Links.DataBase.Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong ResetStorageFileMemoryMapping();

        [DllImport("Platform.Links.DataBase.Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void ReadTest();

        [DllImport("Platform.Links.DataBase.Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void WriteTest();

        public static void RunExperiment2()
        {
            {
                Stopwatch sw = Stopwatch.StartNew();

                ReadTest();

                Console.WriteLine("Elapsed time: {0}.", sw.Elapsed);
                Console.WriteLine();
            }
        }

        public static void RunExperiment()
        {
            InitPersistentMemoryManager();

            while (true)
            {
                ulong result;

                Console.Write("Enter file number: ");
                string number = Console.ReadLine();

                if ((result = OpenStorageFile(@"C:\data_" + number + ".dat")) != 0)
                    return;

                if ((result = SetStorageFileMemoryMapping()) != 0)
                    return;

                Console.ReadKey();

                {
                    Stopwatch sw = Stopwatch.StartNew();

                    ReadTest();

                    Console.WriteLine("Elapsed time: {0}.", sw.Elapsed);
                    Console.WriteLine();
                }

                //Console.ReadKey();

                //ShrinkStorageFile();

                //Console.ReadKey();

                //{
                //    Stopwatch sw = Stopwatch.StartNew();

                //    ReadTest();

                //    Console.WriteLine("Elapsed time: {0}.", sw.Elapsed);
                //    Console.WriteLine();
                //}

                Console.ReadKey();

                if ((result = ResetStorageFileMemoryMapping()) != 0)
                    return;

                if ((result = CloseStorageFile()) != 0)
                    return;
            }
        }
    }
}
