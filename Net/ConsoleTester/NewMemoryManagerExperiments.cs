using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace ConsoleTester
{
	public class NewMemoryManagerExperiments
	{
		[DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
		static private extern void InitPersistentMemoryManager();

		[DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
		static private extern ulong OpenStorageFile(string filename);

		[DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
		static private extern ulong CloseStorageFile();

		[DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
		static private extern ulong EnlargeStorageFile();

		[DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
		static private extern ulong ShrinkStorageFile();

		[DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
		static private extern ulong SetStorageFileMemoryMapping();

		[DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
		static private extern ulong ResetStorageFileMemoryMapping();

		[DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
		static private extern void ReadTest();

		[DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
		static private extern void WriteTest();

		static public void RunExperiment2()
		{
			{
				Stopwatch sw = Stopwatch.StartNew();

				ReadTest();

				Console.WriteLine("Elapsed time: {0}.", sw.Elapsed);
				Console.WriteLine();
			}
		}

		static public void RunExperiment()
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
