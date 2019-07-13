using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Platform.Helpers.Collections.Arrays
{
    public class ArrayFiller<TElement, TReturnConstant>
    {
        private readonly TReturnConstant _returnConstant;
        protected readonly TElement[] _array;
        protected long _position;

        public ArrayFiller(TElement[] array, long offset, TReturnConstant returnConstant)
        {
            _array = array;
            _position = offset;
            _returnConstant = returnConstant;
        }

        public ArrayFiller(TElement[] array, TReturnConstant returnConstant)
            : this(array, 0, returnConstant)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(TElement element) => _array[_position++] = element;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AddAndReturnTrue(TElement element)
        {
            _array[_position++] = element;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TReturnConstant AddAndReturnConstant(TElement element)
        {
            _array[_position++] = element;
            return _returnConstant;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AddFirstAndReturnTrue(IList<TElement> collection)
        {
            _array[_position++] = collection[0];
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TReturnConstant AddFirstAndReturnConstant(IList<TElement> collection)
        {
            _array[_position++] = collection[0];
            return _returnConstant;
        }
    }
}
