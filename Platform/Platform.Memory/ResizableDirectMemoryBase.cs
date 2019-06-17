using System;
using System.Threading;
using Platform.Helpers;
using Platform.Helpers.Disposables;

namespace Platform.Memory
{
    public abstract class ResizableDirectMemoryBase : DisposableBase, IResizableDirectMemory
    {
        public const long MinimumCapacity = 4096;

        private IntPtr _pointer;
        private long _reservedCapacity;
        private long _usedCapacity;

        /// <summary>
        /// Gets the size (bytes) of this memory block.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The memory block is disposed.</exception>
        public long Size
        {
            get
            {
                EnsureNotDisposed();
                return UsedCapacity;
            }
        }

        /// <summary>
        /// Gets the pointer to the beginning of this memory block.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The memory block is disposed.</exception>
        public IntPtr Pointer
        {
            get
            {
                EnsureNotDisposed();
                return _pointer;
            }
            protected set
            {
                EnsureNotDisposed();
                _pointer = value;
            }
        }

        /// <summary>
        /// Gets or sets the reserved capacity (bytes) of this memory block.
        /// </summary>
        /// <remarks>
        /// If less then zero the value is replaced with zero.
        /// Cannot be less than the used capacity of thie memory block.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">The memory block is disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Attempted to set the reserved capacity to a value that is less than the used capacity.</exception>
        public long ReservedCapacity
        {
            get
            {
                EnsureNotDisposed();
                return _reservedCapacity;
            }
            set
            {
                EnsureNotDisposed();
                Ensure.ArgumentInRange(value, new Range<long>(_usedCapacity, long.MaxValue));
                if (value != _reservedCapacity)
                {
                    OnReservedCapacityChanged(_reservedCapacity, value);
                    _reservedCapacity = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the used capacity (bytes) of this memory block.
        /// </summary>
        /// <remarks>
        /// If less then zero the value is replaced with zero.
        /// Cannot be greater than the reserved capacity of this memory block.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">The memory block is disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Attempted to set the used capacity to a value that is greater than the reserved capacity.</exception>
        public long UsedCapacity
        {
            get
            {
                EnsureNotDisposed();
                return _usedCapacity;
            }
            set
            {
                EnsureNotDisposed();
                Ensure.ArgumentInRange(value, new Range<long>(0, _reservedCapacity));
                _usedCapacity = value;
            }
        }

        protected abstract void OnReservedCapacityChanged(long oldReservedCapacity, long newReservedCapacity);

        protected override bool AllowMultipleDisposeCalls => true;

        protected override void DisposeCore(bool manual, bool wasDisposed)
        {
            if (!wasDisposed)
            {
                var pointer = Interlocked.Exchange(ref _pointer, IntPtr.Zero);
                if (pointer != IntPtr.Zero)
                    DisposePointer(pointer, _usedCapacity);
            }
        }

        protected abstract void DisposePointer(IntPtr pointer, long size);
    }
}
