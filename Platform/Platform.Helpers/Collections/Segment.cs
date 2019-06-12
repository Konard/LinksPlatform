using System;
using System.Collections.Generic;

namespace Platform.Helpers.Collections
{
    public class Segment<T> : IEquatable<Segment<T>>
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
                hashAccumulator = ((hashAccumulator << 5) + hashAccumulator) ^ this[i].GetHashCode();

            return hashAccumulator + (hashSeed * 1566083941);
        }

        public virtual bool Equals(Segment<T> other)
        {
            if (Length != other.Length) return false;

            for (int i = 0; i < Length; i++)
                if (!MathHelpers<T>.IsEquals(Base[i], other.Base[i]))
                    return false;
            return true;
        }

        public override bool Equals(object obj) => obj is Segment<T> other ? Equals(other) : false;
    }
}
