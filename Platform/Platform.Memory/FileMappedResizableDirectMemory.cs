using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using Platform.Helpers;
using Platform.Helpers.Disposal;

namespace Platform.Memory
{
    /// <summary>
    /// Represents a memory block stored as a file on disk.
    /// </summary>
    public unsafe class FileMappedResizableDirectMemory : ResizableDirectMemoryBase
    {
        #region Structure

        private readonly string _address;
        private MemoryMappedFile _file;
        private MemoryMappedViewAccessor _accessor;

        #endregion

        #region Logic

        public FileMappedResizableDirectMemory(string address, long minimumReservedCapacity)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentOutOfRangeException("address");
            if (minimumReservedCapacity < 0)
                throw new ArgumentOutOfRangeException("minimumReservedCapacity");

            _address = address;

            var size = FileHelpers.GetSize(_address);

            ReservedCapacity = size > minimumReservedCapacity ? ((size / minimumReservedCapacity + 1) * minimumReservedCapacity) : minimumReservedCapacity;
            UsedCapacity = size;
        }

        /// <remarks>TODO: Проверить насколько корректно будет работать данный случай.</remarks>
        public FileMappedResizableDirectMemory(string address)
            : this(address, 0)
        {
        }

        protected override void OnReservedCapacityChanged(long oldReservedCapacity, long newReservedCapacity)
        {
            UnmapFile();

            FileHelpers.SetSize(_address, newReservedCapacity);

            MapFile(newReservedCapacity);
        }

        private void MapFile(long capacity)
        {
            if (Pointer != null) return;

            _file = MemoryMappedFile.CreateFromFile(_address, FileMode.Open, null, capacity, MemoryMappedFileAccess.ReadWrite);
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
            if (pointer != null)
            {
                if (_accessor != null)
                {
                    _accessor.SafeMemoryMappedViewHandle.ReleasePointer();
                    DisposalHelpers.TryDispose(ref _accessor);
                }

                DisposalHelpers.TryDispose(ref _file);

                return true;
            }
            return false;
        }

        #endregion

        #region DisposalBase

        protected override void DisposePointer(void* pointer, long size)
        {
            if (UnmapFile(pointer))
                FileHelpers.SetSize(_address, size);
        }

        protected override void EnsureNotDisposed()
        {
            var objectName = string.Format("File stored memory block '{0}'.", _address);
            EnsureNotDisposed(objectName);
        }

        #endregion
    }
}