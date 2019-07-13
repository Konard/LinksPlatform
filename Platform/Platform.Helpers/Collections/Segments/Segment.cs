using System;
using System.Collections;
using System.Collections.Generic;

namespace Platform.Helpers.Collections.Segments
{
    public class Segment<T> : IEquatable<Segment<T>>, IList<T>
    {
        public readonly IList<T> Base;
        public readonly int Offset;
        public readonly int Length;

        public Segment(IList<T> @base, int offset, int length)
        {
            Base = @base;
            Offset = offset;
            Length = length;
        }

        public T this[int i]
        {
            get => Base[Offset + i];
            set => Base[Offset + i] = value;
        }

        /// <remarks>
        /// Based on https://github.com/Microsoft/referencesource/blob/3b1eaf5203992df69de44c783a3eda37d3d4cd10/mscorlib/system/string.cs#L833
        /// </remarks>
        public override int GetHashCode()
        {
            var hashSeed = 5381;
            var hashAccumulator = hashSeed;

            for (var i = 0; i < Length; i++)
                hashAccumulator = (hashAccumulator << 5) + hashAccumulator ^ this[i].GetHashCode();

            return hashAccumulator + hashSeed * 1566083941;
        }

        public virtual bool Equals(Segment<T> other)
        {
            if (Length != other.Length) return false;

            for (int i = 0; i < Length; i++)
                if (!MathHelpers<T>.IsEquals(this[i], other[i]))
                    return false;
            return true;
        }

        public override bool Equals(object obj) => obj is Segment<T> other ? Equals(other) : false;

        #region IList

        public int Count => Length;

        public bool IsReadOnly => true;

        public int IndexOf(T item)
        {
            var index = Base.IndexOf(item);
            if (index >= Offset)
            {
                var actualIndex = index - Offset;
                if (actualIndex < Length)
                    return index - Offset;
            }
            return -1;
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(T item) => IndexOf(item) >= 0;

        public void CopyTo(T[] array, int arrayIndex)
        {
            for (int i = 0; i < Length; i++)
                array[arrayIndex + i] = this[i];
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Length; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}
