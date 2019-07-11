namespace Platform.Memory
{
    /// <summary>
    /// Represents a memory block interface with access via indexer.
    /// Представляет интерфейс блока памяти с доступом через индексатор.
    /// </summary>
    /// <typeparam name="TElement">Element type. Тип элемента.</typeparam>
    public interface IArrayMemory<TElement> : IMemory
    {
        /// <summary>
        /// Gets or sets the element at the specified index.
        /// Возвращает или устанавливает элемент по указанному индексу.
        /// </summary>
        /// <param name="index">The index of the element to get or set. Индекс элемента, который нужно получить или установить.</param>
        TElement this[long index] { get; set; }
    }
}
