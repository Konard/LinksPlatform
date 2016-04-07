namespace Platform.Memory
{
    public interface IMemory
    {
        /// <summary>
        /// Возвращает размер блока памяти.
        /// </summary>
        long Size { get; }
    }
}