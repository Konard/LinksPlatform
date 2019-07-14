using System;
using System.Threading;
using Platform.Disposables;
using Platform.Ranges;
using Platform.Helpers.Exceptions;

namespace Platform.Memory
{
    /// <summary>
    /// Provides a base implementation for the resizable memory block with direct access (via unmanaged pointers).
    /// Предоставляет базовую реализацию для блока памяти c изменяемым размером и прямым доступом (через неуправляемые указатели).
    /// </summary>
    public abstract class ResizableDirectMemoryBase : DisposableBase, IResizableDirectMemory
    {
        #region Constants

        public const long MinimumCapacity = 4096;

        #endregion

        #region Fields

        private IntPtr _pointer;
        private long _reservedCapacity;
        private long _usedCapacity;

        #endregion

        #region Properties

        /// <exception cref="ObjectDisposedException">The memory block is disposed. Блок памяти уже высвобожден.</exception>
        public long Size
        {
            get
            {
                EnsureNotDisposed();
                return UsedCapacity;
            }
        }

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

        #endregion

        #region DisposableBase Properties

        protected override bool AllowMultipleDisposeCalls => true;

        #endregion

        #region Methods

        protected abstract void OnReservedCapacityChanged(long oldReservedCapacity, long newReservedCapacity);

        protected abstract void DisposePointer(IntPtr pointer, long size);

        #endregion

        #region DisposableBase Methods

        protected override void DisposeCore(bool manual, bool wasDisposed)
        {
            if (!wasDisposed)
            {
                var pointer = Interlocked.Exchange(ref _pointer, IntPtr.Zero);
                if (pointer != IntPtr.Zero)
                    DisposePointer(pointer, _usedCapacity);
            }
        }

        #endregion
    }
}
