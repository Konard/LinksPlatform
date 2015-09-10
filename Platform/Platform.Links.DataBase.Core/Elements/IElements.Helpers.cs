namespace Platform.Links.DataBase.Core.Elements
{
    /// <summary>
    /// Представляет интерфейс для работы с базой данных (хранилищем)
    /// в формате Elements (хранилища элементов и составных элементов (последовательностей)).
    /// </summary>
    /// <typeparam name="TElement">Тип индекса (идентификатора/адреса) элемента.</typeparam>
    /// <remarks>Здесь располагаются вспомогательные (избыточные) методы.</remarks>
    public partial interface IElements<TElement>
    {
        /// <summary>
        /// Возвращает общее число элементов находящихся в хранилище.
        /// </summary>
        ulong Total { get; }

        /// <summary>
        /// Возвращает левый элемент-продолжение.
        /// </summary>
        /// <param name="element">Элемент.</param>
        /// <returns>Индекс левого (Left) элемента, продолжающего указанный элемент влево.</returns>
        TElement GetLeft(TElement element);

        /// <summary>
        /// Возвращает правый элемент-продолжение.
        /// </summary>
        /// <param name="element">Элемент.</param>
        /// <returns>Индекс правого (Right) элемента, продолжающего указанный элемент вправо.</returns>
        TElement GetRight(TElement element);
    }
}