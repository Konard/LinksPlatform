using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using Platform.Disposables;
using Platform.Helpers.IO;

namespace Platform.Memory
{
    /// <summary>
    /// Represents a memory block stored as a file on disk.
    /// Представляет блок памяти, хранящийся в виде файла на диске.
    /// </summary>
    public unsafe class FileMappedResizableDirectMemory : ResizableDirectMemoryBase
    {
        #region Fields

        private MemoryMappedFile _file;
        private MemoryMappedViewAccessor _accessor;

        protected readonly string Address;

        #endregion

        #region DisposableBase Properties

        protected override string ObjectName => $"File stored memory block at '{Address}' path.";

        #endregion

        #region Constructors

        public FileMappedResizableDirectMemory(string address, long minimumReservedCapacity)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentOutOfRangeException(nameof(address));
            if (minimumReservedCapacity < MinimumCapacity)
                minimumReservedCapacity = MinimumCapacity;

            Address = address;

            var size = FileHelpers.GetSize(Address);

            ReservedCapacity = size > minimumReservedCapacity ? (size / minimumReservedCapacity + 1) * minimumReservedCapacity : minimumReservedCapacity;
            UsedCapacity = size;
        }

        public FileMappedResizableDirectMemory(string address)
            : this(address, MinimumCapacity)
        {
        }

        #endregion

        #region Methods

        private void MapFile(long capacity)
        {
            if (Pointer != IntPtr.Zero)
                return;

            _file = MemoryMappedFile.CreateFromFile(Address, FileMode.Open, null, capacity, MemoryMappedFileAccess.ReadWrite);
            _accessor = _file.CreateViewAccessor();
            byte* pointer = null;
            _accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref pointer);

            Pointer = new IntPtr(pointer);
        }

        private void UnmapFile()
        {
            if (UnmapFile(Pointer))
                Pointer = IntPtr.Zero;
        }

        private bool UnmapFile(IntPtr pointer)
        {
            if (pointer == IntPtr.Zero)
                return false;

            if (_accessor != null)
            {
                _accessor.SafeMemoryMappedViewHandle.ReleasePointer();
                Disposable.TryDispose(ref _accessor);
            }

            Disposable.TryDispose(ref _file);
            return true;
        }

        #endregion

        #region ResizableDirectMemoryBase Methods

        protected override void OnReservedCapacityChanged(long oldReservedCapacity, long newReservedCapacity)
        {
            UnmapFile();

            FileHelpers.SetSize(Address, newReservedCapacity);

            MapFile(newReservedCapacity);
        }

        protected override void DisposePointer(IntPtr pointer, long size)
        {
            if (UnmapFile(pointer))
                FileHelpers.SetSize(Address, size);
        }

        #endregion
    }
}