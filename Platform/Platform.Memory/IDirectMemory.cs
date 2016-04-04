using System;

namespace Platform.Memory
{
    public unsafe interface IDirectMemory : IMemory, IDisposable
    {
        /// <summary>
        /// Возвращает указатель на начало блока памяти.
        /// </summary>
        void* Pointer { get; }
    }
}