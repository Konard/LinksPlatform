using System.IO;
using System.Runtime.InteropServices;
using Platform.Helpers.Disposables;
using Platform.Helpers.IO;

namespace Platform.Memory
{
    /// <remarks>
    /// Ideas for immediate memory (pushed to disk on each operation)
    /// </remarks>
    internal unsafe class SyncedFileMemory<TElement> : DisposableBase, IMemory
        where TElement : struct
    {
        private static readonly long ElementSize = Marshal.SizeOf(typeof(TElement));
        private readonly FileStream _file;

        public long Size => _file.Length;

        public SyncedFileMemory(string filename)
        {
            _file = File.Open(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }

        #region Direct Memory Transformations Wrappers

        public void* Read(long size)
        {
            return null;
        }

        public void Write(void* data, long size)
        {
            
        }

        // Copy
        public void Move(void* source, void* target, long size)
        {
            // It is possible to check if pointer inside this memory block:
            //if (source > Pointer && (Pointer + Size) <= (source + size))
            //{
            //    ...
            //}
            //if (target > Pointer && (Pointer + Size) <= (target + size))
            //{
            //    ...
            //}

            // Such method can be used to trigger events:
            // TriggerRead();
            // TriggerWrite();
        }

        #endregion

        #region Access Through Structure (TElement)

        // Append
        public void Push(TElement value)
        {
            _file.Seek(0, SeekOrigin.End);
            _file.Write(value);
        }

        public TElement this[int index]
        {
            get
            {
                _file.Seek(ElementSize * index, SeekOrigin.Begin);
                return _file.ReadOrDefault<TElement>();
            }
            set
            {
                _file.Seek(ElementSize * index, SeekOrigin.Begin);
                _file.Write(value);
            }
        }

        #endregion

        protected override void DisposeCore(bool manual)
        {
            if (manual)
                Disposable.TryDispose(_file);
        }
    }
}