using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using Platform.Helpers.Disposables;
using Platform.Helpers.IO;

namespace Platform.Memory
{
    /// <summary>
    /// Represents a memory block stored as a file on disk.
    /// </summary>
    public unsafe class FileMappedResizableDirectMemory : ResizableDirectMemoryBase
    {
        #region Structure

        private MemoryMappedFile _file;
        private MemoryMappedViewAccessor _accessor;

        protected readonly string Address;

        #endregion

        #region Logic

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

        protected override void OnReservedCapacityChanged(long oldReservedCapacity, long newReservedCapacity)
        {
            UnmapFile();

            FileHelpers.SetSize(Address, newReservedCapacity);

            MapFile(newReservedCapacity);
        }

        private void MapFile(long capacity)
        {
            if (Pointer != null)
                return;

            _file = MemoryMappedFile.CreateFromFile(Address, FileMode.Open, null, capacity, MemoryMappedFileAccess.ReadWrite);
            _accessor = _file.CreateViewAccessor();
            byte* pointer = null;
            _accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref pointer);

            Pointer = pointer;
        }

        private void UnmapFile()
        {
            if (UnmapFile(Pointer))
                Pointer = null;
        }

        private bool UnmapFile(void* pointer)
        {
            if (pointer == null)
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

        #region DisposalBase

        protected override void DisposePointer(void* pointer, long size)
        {
            if (UnmapFile(pointer))
                FileHelpers.SetSize(Address, size);
        }

        protected override void EnsureNotDisposed() => EnsureNotDisposed($"File stored memory block '{Address}'.");

        #endregion
    }
}