namespace Platform.Memory
{
    /// <summary>
    /// Represents a memory block interface with size in bytes.
    /// Представляет интерфейс блока памяти с размером в байтах.
    /// </summary>
    public interface IMemory
    {
        /// <summary>
        /// Gets the size in bytes of this memory block.
        /// Возвращает размер блока памяти в байтах.
        /// </summary>
        long Size { get; }
    }
}