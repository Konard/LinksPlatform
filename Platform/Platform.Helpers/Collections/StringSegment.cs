using System.Linq;
using System.Collections.Generic;

namespace Platform.Helpers.Collections
{
    public unsafe class StringSegment : Segment<char>
    {
        public StringSegment(IList<char> @base, int offset, int length)
            : base(@base, offset, length)
        {
        }

        /// <remarks>
        /// Based on https://github.com/Microsoft/referencesource/blob/3b1eaf5203992df69de44c783a3eda37d3d4cd10/mscorlib/system/string.cs#L833
        /// </remarks>
        public override int GetHashCode()
        {
            var hashSeed = 5381;
            var hashAccumulator = hashSeed;

            if (Base is char[] baseArray)
                unsafe
                {
                    fixed (char* src = &baseArray[Offset])
                        for (char* s = src, last = s + Length; s < last; s++)
                            hashAccumulator = ((hashAccumulator << 5) + hashAccumulator) ^ *s;
                }
            else
                for (var i = 0; i < Length; i++)
                    hashAccumulator = ((hashAccumulator << 5) + hashAccumulator) ^ this[i];

            return hashAccumulator + (hashSeed * 1566083941);
        }

        /// <remarks>
        /// Based on https://github.com/Microsoft/referencesource/blob/3b1eaf5203992df69de44c783a3eda37d3d4cd10/mscorlib/system/string.cs#L364
        /// </remarks>
        public override bool Equals(Segment<char> other)
        {
            if (Length != other.Length) return false;

            if (Base is char[] baseArray && other.Base is char[] otherArray)
            {
                fixed (char* ap = &baseArray[Offset]) fixed (char* bp = &otherArray[other.Offset])
                {
                    var a = ap;
                    var b = bp;

                    var length = Length;

                    while (length >= 10)
                    {
                        if (*(int*)a != *(int*)b) return false;
                        if (*(int*)(a + 2) != *(int*)(b + 2)) return false;
                        if (*(int*)(a + 4) != *(int*)(b + 4)) return false;
                        if (*(int*)(a + 6) != *(int*)(b + 6)) return false;
                        if (*(int*)(a + 8) != *(int*)(b + 8)) return false;
                        a += 10; b += 10; length -= 10;
                    }

                    // This depends on the fact that the String objects are
                    // always zero terminated and that the terminating zero is not included
                    // in the length. For odd string sizes, the last compare will include
                    // the zero terminator.
                    while (length > 0)
                    {
                        if (*(int*)a != *(int*)b) break;
                        a += 2; b += 2; length -= 2;
                    }

                    return length <= 0;
                }
            }
            else
            {
                for (int i = 0; i < Length; i++)
                    if (Base[i] != other.Base[i])
                        return false;
                return true;
            }
        }

        public static implicit operator string(StringSegment segment)
        {
            if (!(segment.Base is char[] array))
                array = segment.Base.ToArray();
            return new string(array, segment.Offset, segment.Length);
        }

        public override string ToString() => this;
    }
}
