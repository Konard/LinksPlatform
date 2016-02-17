using System;
using Platform.Helpers;
using Platform.Helpers.Disposal;
using Platform.WindowsAPI;

namespace Platform.Memory
{
    /// <summary>
    /// Представляет блок неуправляемой куче памяти предоставляемой операционной системой Windows.
    /// </summary>
    /// <remarks>
    /// TODO: Реализовать вариант с Virtual Memory
    /// TODO: Реализовать абстрактный класс MemoryBase
    /// TODO: После использования WinApi подумать над реализацией под Mono
    /// TODO: Move Low Level functions to Kernel32
    /// </remarks>
    public unsafe class HeapMemory : DisposalBase, IMemory
    {
        private static readonly IntPtr CurrentProcessHeapHandle = Kernel32.GetProcessHeap();

        #region Instance Structure

        private void* _pointer;

        private long _reservedCapacity;
        private long _usedCapacity;

        #endregion

        #region Properties

        /// <summary>
        /// Возвращает указатель на начало блока памяти.
        /// </summary>
        public void* Pointer
        {
            get
            {
                EnsureNotDisposed();
                return _pointer;
            }
        }

        /// <summary>
        /// Возвращает или устанавливает размер зарезервированной ёмкости блока памяти в байтах.
        /// Рекомендуется изменять этот размер большими блоками, чтобы влияние на производительность было минимально.
        /// Не может быть меньше размера используемой ёмкости.
        /// </summary>
        /// <exception cref="ObjectDisposedException">В случае, если объект HeapMemory уже был высвобожден из памяти.</exception>
        /// <exception cref="ArgumentOutOfRangeException">В случае, если размер зарезервированной ёмкости меньше объёма используемой ёмкости.</exception>
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
                if (value < _usedCapacity)
                    throw new ArgumentOutOfRangeException(string.Format("Размер зарезервированной ёмкости блока памяти не может быть меньше размера используемой ёмкости {0}.", _usedCapacity));
                if (value < 0)
                    value = 0;

                if (value != _reservedCapacity)
                {
                    _pointer = _pointer == null ? Alloc(value) : ReAlloc(_pointer, value);

                    _reservedCapacity = value;
                }
            }
        }

        /// <summary>
        /// Возвращает или устанавливает размер используемой ёмкости блока памяти в байтах.
        /// Не может быть больше размера зарезервированной ёмкости.
        /// </summary>
        /// <exception cref="ObjectDisposedException">В случае, если объект HeapMemory уже был высвобожден из памяти.</exception>
        /// <exception cref="ArgumentOutOfRangeException">В случае, если размер используемой ёмкости превышает объём зарезервированной ёмкости.</exception>
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
                    throw new ArgumentOutOfRangeException(string.Format("Размер используемой ёмкости блока памяти не может быть больше размера зарезервированной ёмкости {0}.", _reservedCapacity));
                _usedCapacity = value;
            }
        }

        #endregion

        #region Logic

        public HeapMemory(long minimumReservedCapacity)
        {
            ReservedCapacity = minimumReservedCapacity;
            UsedCapacity = 0;
        }

        public HeapMemory()
            : this(0)
        {
        }

        #endregion

        #region DisposalBase

        protected override void DisposeCore(bool manual)
        {
            if (_pointer != null) Free(_pointer);
        }

        protected override void EnsureNotDisposed()
        {
            EnsureNotDisposed("Блок памяти в неуправляемой куче.");
        }

        #endregion

        #region Low Level

        // Allocates a memory block of the given size. The allocated memory is
        // automatically initialized to zero.
        public static void* Alloc(long size)
        {
            var result = Kernel32.HeapAlloc(CurrentProcessHeapHandle, Kernel32.HeapFlags.ZeroMemory, new UIntPtr((ulong)size)).ToPointer();
            if (result == null) throw new OutOfMemoryException();
            return result;
        }

        // Copies count bytes from src to dst. The source and destination
        // blocks are permitted to overlap.
        public static void Copy(void* src, void* dst, int count)
        {
            Kernel32.CopyMemory(new IntPtr(dst), new IntPtr(src), new UIntPtr((uint)count));

            /*
            byte* ps = (byte*)src;
            byte* pd = (byte*)dst;
            if (ps > pd)
            {
                for (; count != 0; count--) *pd++ = *ps++;
            }
            else if (ps < pd)
            {
                for (ps += count, pd += count; count != 0; count--) *--pd = *--ps;
            }*/
        }

        // Frees a memory block.
        public static void Free(void* block)
        {
            if (!Kernel32.HeapFree(CurrentProcessHeapHandle, 0, new IntPtr(block)))
                throw new InvalidOperationException();
        }

        // Re-allocates a memory block. If the reallocation request is for a
        // larger size, the additional region of memory is automatically
        // initialized to zero.
        public static void* ReAlloc(void* block, long size)
        {
            var result =
                Kernel32.HeapReAlloc(CurrentProcessHeapHandle, Kernel32.HeapFlags.ZeroMemory, new IntPtr(block),
                    new UIntPtr((ulong)size)).ToPointer();
            if (result == null) throw new OutOfMemoryException();
            return result;
        }

        // Returns the size of a memory block.
        public static int SizeOf(void* block)
        {
            var result = (int)Kernel32.HeapSize(CurrentProcessHeapHandle, 0, new IntPtr(block)).ToUInt32();
            if (result == -1) throw new InvalidOperationException();
            return result;
        }

        #endregion
    }
}