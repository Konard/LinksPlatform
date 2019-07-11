using System;

namespace Platform.Memory
{
    /// <summary>
    /// Represents a memory block interface with direct access (via unmanaged pointers).
    /// Представляет интерфейс блока памяти с прямым доступом (через неуправляемые указатели).
    /// </summary>
    public interface IDirectMemory : IMemory, IDisposable
    {
        /// <summary>
        /// Gets the pointer to the beginning of this memory block.
        /// Возвращает указатель на начало блока памяти.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The memory block is disposed. Блок памяти уже высвобожден.</exception>
        IntPtr Pointer { get; }
    }
}