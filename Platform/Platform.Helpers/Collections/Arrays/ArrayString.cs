namespace Platform.Helpers.Collections.Array
{
    public class ArrayString<T>
    {
        public readonly T[] Array;
        public long Length;

        public ArrayString(long length)
            : this(new T[length], length)
        {
        }

        public ArrayString(T[] array)
            : this(array, array.Length)
        {
        }

        public ArrayString(T[] array, long length)
        {
            Array = array;
            Length = length;
        }

        public bool Contains(T value)
        {
            var index = System.Array.IndexOf(Array, value);
            return index >= 0 && index < Length;
        }
    }
}
