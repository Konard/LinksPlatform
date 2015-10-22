using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using Platform.Helpers;
using Platform.Helpers.Disposal;

namespace Platform.Links.System.Memory
{
    /// <summary>
    /// Представляет блок памяти.
    /// </summary>
    /// <remarks>
    /// Необходимо реализовать либо класс TemporaryMemory или встроить свойство определяющее используется ли реальный файл
    /// -> Реализовать пустой конструктор (в этом случае внутри используется
    /// После использования WinApi подумать над реализацией под Mono
    /// После реализации кроссплатформенной версии этот класс и его интерфейс IMemory можно опубликовать,
    /// для этого реализацию этого класса нужно будет перенести в проект Konard.Helpers
    /// </remarks>
    public unsafe class FileMappedMemory : DisposalBase, IMemory
    {
        #region Structure

        private readonly string _address;
        private MemoryMappedFile _file;
        private MemoryMappedViewAccessor _accessor;

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
        /// <exception cref="ObjectDisposedException">В случае, если объект FileMappedMemory уже был высвобожден из памяти.</exception>
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
                if (value < 0)
                    value = 0;
                if (value < _usedCapacity)
                    throw new Exception(
                        string.Format(
                            "Размер зарезервированной ёмкости блока памяти '{0}' не может быть меньше размера используемой ёмкости {1}.",
                            _address, _usedCapacity));
                if (value != _reservedCapacity)
                {
                    MemoryHelpers.AlignSizeToSystemPageSize(ref value);

                    if (value != _reservedCapacity)
                    {
                        UnmapFile();

                        ForceResize(_address, ref value);

                        // Может ли previousCapacity изменить фактический размер _usedCapacity?

                        _reservedCapacity = value;

                        MapFile();
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
        /// <exception cref="ObjectDisposedException">В случае, если объект FileMappedMemory уже был высвобожден из памяти.</exception>
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
                    value = 0;
                if (value > _reservedCapacity)
                    throw new Exception(
                        string.Format(
                            "Размер используемой ёмкости блока памяти '{0}' не может быть больше размера зарезервированной ёмкости {1}.",
                            _address, _reservedCapacity));
                _usedCapacity = value;
            }
        }

        #endregion

        #region Logic

        public FileMappedMemory(string address, long minimumReservedCapacity)
        {
            if (minimumReservedCapacity < 0)
            {
                throw new ArgumentOutOfRangeException("minimumReservedCapacity");
            }

            _address = address;
            _reservedCapacity = minimumReservedCapacity;
            _usedCapacity = 0;

            MemoryHelpers.AlignSizeToSystemPageSize(ref _reservedCapacity);

            // Изменение размера должно учитывать вариант "без использования файлов",
            // после этого вынести изменение размера из этого условия
            Resize(_address, ref _reservedCapacity, out _usedCapacity);
            MapFile();
        }

        /// <remarks>Проверить насколько корректно будет работать данный случай.</remarks>
        public FileMappedMemory(string address)
            : this(address, 0)
        {
        }

        private void MapFile()
        {
            _file = MemoryMappedFile.CreateFromFile(_address, FileMode.Open, Guid.NewGuid().ToString(),
                _reservedCapacity, MemoryMappedFileAccess.ReadWrite);
            _accessor = _file.CreateViewAccessor();
            _accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref _pointer);
        }

        private void UnmapFile()
        {
            if (_pointer != null && _accessor != null)
            {
                _accessor.SafeMemoryMappedViewHandle.ReleasePointer();
                _pointer = null;
            }
            if (_accessor != null)
            {
                _accessor.Dispose();
                _accessor = null;
            }
            if (_file != null)
            {
                _file.Dispose();
                _file = null;
            }
        }

        #endregion

        #region Static Utility Methods

        private static void ForceResize(string address, ref long newCapacity)
        {
            long previousCapacity;
            Resize(address, ref newCapacity, out previousCapacity, force: true);
        }

        private static void Resize(string address, ref long newCapacity, out long previousCapacity, bool force = false)
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
            if (manual) UnmapFile();
            ForceResize(_address, ref _usedCapacity);
        }

        protected override void EnsureNotDisposed()
        {
            EnsureNotDisposed(string.Format("Хранимый блок памяти '{0}'.", _address));
        }

        #endregion
    }
}