using System;
using System.Threading;
using Platform.Helpers.Disposables;

namespace Platform.Memory
{
    public abstract unsafe class ResizableDirectMemoryBase : DisposableBase, IResizableDirectMemory
    {
        public const long MinimumCapacity = 4096;

        private IntPtr _pointer;
        private long _reservedCapacity;
        private long _usedCapacity;

        /// <summary>
        /// Gets the size (bytes) of this memory block.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The memory block is disposed.</exception>
        public long Size => UsedCapacity;

        /// <summary>
        /// Gets the pointer to the beginning of this memory block.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The memory block is disposed.</exception>
        public void* Pointer
        {
            get
            {
                EnsureNotDisposed();
                return _pointer.ToPointer();
            }
            protected set
            {
                _pointer = new IntPtr(value);
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
                if (value < 0)
                    value = 0;
                if (value < _usedCapacity)
                {
                    var message = $"The reserved capacity {value} cannot be less than used capacity {_usedCapacity}.";
                    throw new ArgumentOutOfRangeException(message);
                }
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
                if (value < 0)
                    value = 0;
                if (value > _reservedCapacity)
                {
                    // TODO: Use internationalization (aka Resources for message format).
                    var message = $"The used capacity {value} cannot be greater than the reserved capacity {_reservedCapacity}.";
                    throw new ArgumentOutOfRangeException(message);
                }
                _usedCapacity = value;
            }
        }

        protected abstract void OnReservedCapacityChanged(long oldReservedCapacity, long newReservedCapacity);

        protected override bool AllowMultipleDisposeCalls => true;

        protected override void DisposeCore(bool manual)
        {
            var pointer = Interlocked.Exchange(ref _pointer, IntPtr.Zero).ToPointer();
            if (pointer != null)
                DisposePointer(pointer, _usedCapacity);
        }

        protected abstract void DisposePointer(void* pointer, long size);
    }
}
