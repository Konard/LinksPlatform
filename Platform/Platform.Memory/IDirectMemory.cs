using System;

namespace Platform.Memory
{
    public interface IDirectMemory : IMemory, IDisposable
    {
        /// <summary>
        /// Возвращает указатель на начало блока памяти.
        /// </summary>
        IntPtr Pointer { get; }
    }
}