namespace Platform.Memory
{
    /// <summary>
    /// Represents a memory block with access via indexer.
    /// Представляет блок памяти с доступом через индексатор.
    /// </summary>
    /// <typeparam name="TElement">Element type. Тип элемента.</typeparam>
    public class ArrayMemory<TElement> : IArrayMemory<TElement>
    {
        #region Fields

        private readonly TElement[] _array;

        #endregion

        #region Properties

        public long Size => _array.Length;

        public TElement this[long index]
        {
            get => _array[index];
            set => _array[index] = value;
        }

        #endregion

        #region Constuctors

        public ArrayMemory(long size) => _array = new TElement[size];

        #endregion
    }
}
