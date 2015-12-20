using System;
using System.IO;
using Platform.Helpers;
using Platform.Helpers.Disposal;
using Platform.WindowsAPI;

namespace Platform.Memory
{
    /// <summary>
    /// Представляет блок неуправляемой куче памяти предоставляемой операционной системой Windows.
    /// </summary>
    /// <remarks>
    /// После использования WinApi подумать над реализацией под Mono
    /// </remarks>
    public unsafe class HeapMemory : DisposalBase, IMemory
    {
        private static readonly IntPtr CurrentProcessHeapHandle = Kernel32.GetProcessHeap();

        #region Structure

        private byte* _pointer;
            // Может хранить как ссылку на MemoryMappedFile, так и на блок, выделенный в куче операционной системы.

        private long _reservedCapacity;
        private long _usedCapacity;

        #endregion

        #region Properties

        /// <summary>
        /// Возвращает указатель на начало блока памяти.
        /// При операциях над данными с адреса Pointer до адреса (Pointer + ReservedCapacity) будет верно следующее:
        /// 1. При чтении данные будут лениво подгружаться из исходного файла на диске.
        /// 2. При записи данные будут лениво записывается в исходный файл на диске.
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
        /// <exception cref="Exception">В случае, если размер зарезервированной ёмкости меньше объёма используемой ёмкости.</exception>
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
                {
                    throw new NotImplementedException();
                    //throw new Exception(string.Format("Размер зарезервированной ёмкости блока памяти '{0}' не может быть меньше размера используемой ёмкости {1}.", _address, _usedCapacity));
                }
                if (value < 0)
                {
                    value = 0;
                }
                // TODO: Check Int32.MaxValue
                if (value != _reservedCapacity)
                {
                    // Решить нужно ли это в случае управляемой кучи
                    MemoryHelpers.AlignSizeToSystemPageSize(ref value);

                    if (value != _reservedCapacity)
                    {
                        /*
                        UnmapFile();

                        long previousCapacity;
                        Resize(_address, ref value, out previousCapacity, force: true);

                        _reservedCapacity = value;

                        MapFile();
                        */
                    }
                }
            }
        }

        /// <summary>
        /// Возвращает или устанавливает размер используемой ёмкости блока памяти в байтах.
        /// Рекомендуется держать это значение в максимально актуальном состоянии, 
        /// так как при завершении работы с блоком памяти размер исходного файла будет уменьшен именно до этого значения.
        /// Не может быть больше размера зарезервированной ёмкости.
        /// </summary>
        /// <exception cref="ObjectDisposedException">В случае, если объект HeapMemory уже был высвобожден из памяти.</exception>
        /// <exception cref="Exception">В случае, если размер используемой ёмкости превышает объём зарезервированной ёмкости.</exception>
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
                {
                    value = 0;
                }
                // TODO: Check Int32.MaxValue
                if (value > _reservedCapacity)
                {
                    throw new NotImplementedException();
                    //throw new Exception(string.Format("Размер используемой ёмкости блока памяти '{0}' не может быть больше размера зарезервированной ёмкости {1}.", _address, _reservedCapacity));
                }
                _usedCapacity = value;
            }
        }

        #endregion

        #region Logic

        public HeapMemory(long minimumReservedCapacity)
        {
            if (minimumReservedCapacity < 0
                || minimumReservedCapacity > Int32.MaxValue)
                throw new ArgumentOutOfRangeException("minimumReservedCapacity");

            _reservedCapacity = minimumReservedCapacity;
            _usedCapacity = 0;

            // Решить, нужно ли это в случае управляемой кучи
            MemoryHelpers.AlignSizeToSystemPageSize(ref _reservedCapacity);

            _pointer = (byte*) Alloc((int) _reservedCapacity);
        }

        private void Alloc()
        {
            //_pointer = ; // выделить необходимый блок виртуальной памяти
        }

        public HeapMemory()
            : this(0)
        {
        }

        //private void MapFile()
        //{
        //    _file = MemoryMappedFile.CreateFromFile(_address, FileMode.Open, Guid.NewGuid().ToString(), _reservedCapacity, MemoryMappedFileAccess.ReadWrite);
        //    _accessor = _file.CreateViewAccessor();
        //    _accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref _pointer);
        //}

        //private void UnmapFile()
        //{
        //    if (_pointer != null && _accessor != null)
        //    {
        //        _accessor.SafeMemoryMappedViewHandle.ReleasePointer();
        //        //this.accessor.SafeMemoryMappedViewHandle.Dispose();
        //        _pointer = null;
        //    }
        //    if (_accessor != null)
        //    {
        //        _accessor.Dispose();
        //        _accessor = null;
        //    }
        //    if (_file != null)
        //    {
        //        _file.Dispose();
        //        _file = null;
        //    }
        //}

        #endregion

        #region Static Utility Methods

        private static void Resize(string address, ref long newCapacity, out long previousCapacity, bool force)
        {
            using (var fs = File.Open(address, FileMode.OpenOrCreate))
            {
                previousCapacity = fs.Length;

                if (previousCapacity < newCapacity || (force && previousCapacity != newCapacity))
                {
                    fs.SetLength(newCapacity);
                }
                else
                {
                    newCapacity = previousCapacity;
                    /* Пока не понятно, нужны ли такие манипуляции
                    AlignCapacityToSystemPageSize(ref newCapacity);
                    if (newCapacity != previousCapacity)
                    {
                        fs.SetLength(newCapacity);
                    }
                    */
                }
            }
        }

        #endregion

        #region DisposalBase

        protected override void DisposeCore(bool manual)
        {
            throw new NotImplementedException();
        }

        protected override void EnsureNotDisposed()
        {
            EnsureNotDisposed("Блок памяти в неуправляемой куче.");
        }

        #endregion

        #region Low Level

        // Allocates a memory block of the given size. The allocated memory is
        // automatically initialized to zero.
        public static void* Alloc(int size)
        {
            var result =
                Kernel32.HeapAlloc(CurrentProcessHeapHandle, Kernel32.HeapFlags.ZeroMemory, new UIntPtr((uint) size))
                    .ToPointer();
            if (result == null) throw new OutOfMemoryException();
            return result;
        }

        // Copies count bytes from src to dst. The source and destination
        // blocks are permitted to overlap.
        public static void Copy(void* src, void* dst, int count)
        {
            Kernel32.CopyMemory(new IntPtr(dst), new IntPtr(src), new UIntPtr((uint) count));

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
        public static void* ReAlloc(void* block, int size)
        {
            var result =
                Kernel32.HeapReAlloc(CurrentProcessHeapHandle, Kernel32.HeapFlags.ZeroMemory, new IntPtr(block),
                    new UIntPtr((uint) size)).ToPointer();
            if (result == null) throw new OutOfMemoryException();
            return result;
        }

        // Returns the size of a memory block.
        public static int SizeOf(void* block)
        {
            var result = (int) Kernel32.HeapSize(CurrentProcessHeapHandle, 0, new IntPtr(block)).ToUInt32();
            if (result == -1) throw new InvalidOperationException();
            return result;
        }

        #endregion
    }
}