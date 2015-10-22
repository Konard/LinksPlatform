using System.IO;
using Platform.Helpers.Disposal;

namespace Platform.Memory
{
    public unsafe class SyncedFileMemory : DisposalBase
    {
        private readonly FileStream _file;

        public SyncedFileMemory(string filename)
        {
            _file = File.Open(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }

        public void* Read(long size)
        {
            return null;
        }

        public void Write(void* data, long size)
        {
        }

        /*public void Push<T>(T value)
            where T : struct
        {
            var bytes = BitConverterHelpers.GetBytes(value);
            _file.Write(bytes, 0, bytes.Length);
        }*/

        protected override void DisposeCore(bool manual)
        {
            _file.Dispose();
        }
    }
}