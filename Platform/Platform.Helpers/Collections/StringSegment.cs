using System;

namespace Platform.Helpers.Collections
{
    public unsafe struct StringSegment : IEquatable<StringSegment>
    {
        public readonly char[] Chars;
        public readonly int Offset;
        public readonly int Length;

        public StringSegment(char[] chars, int offset, int length)
        {
            Chars = chars;
            Offset = offset;
            Length = length;
        }

        public char this[int i]
        {
            get => Chars[Offset + i];
            set => Chars[Offset + i] = value;
        }

        /// <remarks>
        /// Based on https://github.com/Microsoft/referencesource/blob/3b1eaf5203992df69de44c783a3eda37d3d4cd10/mscorlib/system/string.cs#L833
        /// </remarks>
        public override int GetHashCode()
        {

//#if FEATURE_RANDOMIZED_STRING_HASHING
//            if(HashHelpers.s_UseRandomizedStringHashing)
//            {
//                return InternalMarvin32HashString(this, this.Length, 0);
//            }
//#endif // FEATURE_RANDOMIZED_STRING_HASHING

            unsafe
            {
                fixed (char* src = &Chars[Offset])
                {
                    //Contract.Assert(src[this.Length] == '\0', "src[this.Length] == '\\0'");
                    //Contract.Assert(((int)src) % 4 == 0, "Managed string should start at 4 bytes boundary");

//#if WIN32
//                    int hash1 = (5381<<16) + 5381;
//#else
                    int hash1 = 5381;
//#endif
                    int hash2 = hash1;

//#if WIN32
//                    // 32 bit machines.
//                    int* pint = (int *)src;
//                    int len = this.Length;
//                    while (len > 2)
//                    {
//                        hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ pint[0];
//                        hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ pint[1];
//                        pint += 2;
//                        len  -= 4;
//                    }

//                    if (len > 0)
//                    {
//                        hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ pint[0];
//                    }
//#else
                    for (char* s = src, last = s + Length; s < last; s++)
                        hash1 = ((hash1 << 5) + hash1) ^ *s;
//#endif

                    return hash1 + (hash2 * 1566083941);
                }
            }
        }

        public override bool Equals(object obj) => obj is StringSegment other ? Equals(other) : false;

        /// <remarks>
        /// Based on https://github.com/Microsoft/referencesource/blob/3b1eaf5203992df69de44c783a3eda37d3d4cd10/mscorlib/system/string.cs#L364
        /// </remarks>
        public bool Equals(StringSegment other)
        {
            if (Length != other.Length) return false;

            int length = Length;

            fixed (char* ap = &Chars[Offset]) fixed (char* bp = &other.Chars[other.Offset])
            {
                char* a = ap;
                char* b = bp;

                // unroll the loop
//#if AMD64
//                // for AMD64 bit platform we unroll by 12 and
//                // check 3 qword at a time. This is less code
//                // than the 32 bit case and is shorter
//                // pathlength

//                while (length >= 12)
//                {
//                    if (*(long*)a     != *(long*)b) return false;
//                    if (*(long*)(a+4) != *(long*)(b+4)) return false;
//                    if (*(long*)(a+8) != *(long*)(b+8)) return false;
//                    a += 12; b += 12; length -= 12;
//                }
//#else
                while (length >= 10)
                {
                    if (*(int*)a != *(int*)b) return false;
                    if (*(int*)(a + 2) != *(int*)(b + 2)) return false;
                    if (*(int*)(a + 4) != *(int*)(b + 4)) return false;
                    if (*(int*)(a + 6) != *(int*)(b + 6)) return false;
                    if (*(int*)(a + 8) != *(int*)(b + 8)) return false;
                    a += 10; b += 10; length -= 10;
                }
//#endif

                // This depends on the fact that the String objects are
                // always zero terminated and that the terminating zero is not included
                // in the length. For odd string sizes, the last compare will include
                // the zero terminator.
                while (length > 0)
                {
                    if (*(int*)a != *(int*)b) break;
                    a += 2; b += 2; length -= 2;
                }

                return (length <= 0);
            }
        }

        public static implicit operator string(StringSegment segment) => new string(segment.Chars, segment.Offset, segment.Length);

        public override string ToString() => this;
    }
}
