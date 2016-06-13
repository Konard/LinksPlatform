namespace Platform.Memory
{
    public class ArrayMemory<TElement> : IArrayMemory<TElement>
    {
        private readonly TElement[] _array;

        public long Size => _array.Length;

        public TElement this[long index]
        {
            get { return _array[index]; }
            set { _array[index] = value; }
        }

        public ArrayMemory(long size)
        {
            _array = new TElement[size];
        }
    }
}
