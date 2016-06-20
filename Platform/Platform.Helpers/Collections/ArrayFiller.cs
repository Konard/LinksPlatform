using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Platform.Helpers.Collections
{
    public class ArrayFiller<TElement>
    {
        private readonly TElement[] _array;
        private long _position;

        public ArrayFiller(TElement[] array)
        {
            _array = array;
            _position = 0;
        }

        public ArrayFiller(TElement[] array, long offset)
        {
            _array = array;
            _position = offset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(TElement element)
        {
            _array[_position++] = element;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AddAndReturnTrue(TElement element)
        {
            _array[_position++] = element;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AddFirstAndReturnTrue(IList<TElement> collection)
        {
            _array[_position++] = collection[0];
            return true;
        }
    }
}
