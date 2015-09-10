using System.IO;
using Platform.Links.System.Helpers.Disposal;

namespace Platform.Links.System.Memory
{
    public unsafe class SyncedFileMemory : DisposalBase
    {
        private readonly FileStream _file;

        public SyncedFileMemory(string filename)
        {
            _file = File.Open(filename, FileMode.Append, FileAccess.Write);
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
            var bytes = BitConverterExtensions.GetBytes(value);
            _file.Write(bytes, 0, bytes.Length);
        }*/

        protected override void DisposeCore(bool manual)
        {
            _file.Dispose();
        }
    }
}
