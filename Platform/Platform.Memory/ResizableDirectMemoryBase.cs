using System;
using Platform.Helpers.Disposal;

namespace Platform.Memory
{
    public abstract unsafe class ResizableDirectMemoryBase : DisposalBase, IResizableDirectMemory
    {
        private void* _pointer;
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
                return UsedCapacity;
            }
        }

        /// <summary>
        /// Gets the pointer to the beginning of this memory block.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The memory block is disposed.</exception>
        public void* Pointer
        {
            get
            {
                EnsureNotDisposed();
                return _pointer;
            }
            protected set
            {
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
                    var message = string.Format("The reserved capacity {0} cannot be less than used capacity {1}.", value, _usedCapacity);
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
                    var message = string.Format("The used capacity {0} cannot be greater than the reserved capacity {1}.", value, _reservedCapacity);
                    throw new ArgumentOutOfRangeException(message);
                }
                _usedCapacity = value;
            }
        }

        protected abstract void OnReservedCapacityChanged(long oldReservedCapacity, long newReservedCapacity);

        protected override bool AllowMultipleDisposeCalls
        {
            get { return true; }
        }

        protected override void DisposeCore(bool manual)
        {
            if (_pointer != null)
            {
                DisposePointer(_pointer, _usedCapacity);
                _pointer = null;
            }
        }

        protected abstract void DisposePointer(void* pointer, long size);
    }
}
