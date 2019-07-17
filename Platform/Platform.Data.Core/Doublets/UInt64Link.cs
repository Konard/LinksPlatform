using System;
using System.Collections;
using System.Collections.Generic;
using Platform.Exceptions;
using Platform.Helpers;

namespace Platform.Data.Core.Doublets
{
    /// <summary>
    /// Структура описывающая уникальную связь.
    /// </summary>
    public struct UInt64Link : IEquatable<UInt64Link>, IList<ulong>
    {
        private static readonly LinksConstants<bool, ulong, int> Constants = Default<LinksConstants<bool, ulong, int>>.Instance;

        private const int Length = 3;

        public readonly ulong Index;
        public readonly ulong Source;
        public readonly ulong Target;

        public static readonly UInt64Link Null = new UInt64Link();
        

        public UInt64Link(params ulong[] values)
        {
            Index = values.Length > Constants.IndexPart ? values[Constants.IndexPart] : Constants.Null;
            Source = values.Length > Constants.SourcePart ? values[Constants.SourcePart] : Constants.Null;
            Target = values.Length > Constants.TargetPart ? values[Constants.TargetPart] : Constants.Null;
        }

        public UInt64Link(IList<ulong> values)
        {
            Index = values.Count > Constants.IndexPart ? values[Constants.IndexPart] : Constants.Null;
            Source = values.Count > Constants.SourcePart ? values[Constants.SourcePart] : Constants.Null;
            Target = values.Count > Constants.TargetPart ? values[Constants.TargetPart] : Constants.Null;
        }

        public UInt64Link(ulong index, ulong source, ulong target)
        {
            Index = index;
            Source = source;
            Target = target;
        }

        public UInt64Link(ulong source, ulong target)
            : this(Constants.Null, source, target)
        {
            Source = source;
            Target = target;
        }

        public static UInt64Link Create(ulong source, ulong target) => new UInt64Link(source, target);

        public override int GetHashCode()
        {
            var hash = 17;
            hash = hash * 31 + Index.GetHashCode();
            hash = hash * 31 + Source.GetHashCode();
            hash = hash * 31 + Target.GetHashCode();
            return hash;
        }

        public bool IsNull() => Index == Constants.Null && Source == Constants.Null && Target == Constants.Null;

        public override bool Equals(object other) => other is UInt64Link && Equals((UInt64Link)other);

        public bool Equals(UInt64Link other) => Index == other.Index &&
                                                Source == other.Source &&
                                                Target == other.Target;

        public static string ToString(ulong index, ulong source, ulong target) => $"({index}: {source}->{target})";

        public static string ToString(ulong source, ulong target) => $"({source}->{target})";

        public static implicit operator ulong[](UInt64Link link) => link.ToArray();

        public static implicit operator UInt64Link(ulong[] linkArray) => new UInt64Link(linkArray);

        #region IList

        public override string ToString() => Index == Constants.Null ? ToString(Source, Target) : ToString(Index, Source, Target);

        public ulong[] ToArray()
        {
            var array = new ulong[Length];
            CopyTo(array, 0);
            return array;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<ulong> GetEnumerator()
        {
            yield return Index;
            yield return Source;
            yield return Target;
        }

        public void Add(ulong item) => Throw.NotSupportedException();

        public void Clear() => Throw.NotSupportedException();

        public bool Contains(ulong item) => IndexOf(item) > 0;

        public void CopyTo(ulong[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (arrayIndex + Length > array.Length) throw new ArgumentException();

            array[arrayIndex++] = Index;
            array[arrayIndex++] = Source;
            array[arrayIndex] = Target;
        }

        public bool Remove(ulong item) => Throw.NotSupportedExceptionAndReturn<bool>();

        public int Count => Length;
        public bool IsReadOnly => true;

        public int IndexOf(ulong item)
        {
            if (Index == item) return Constants.IndexPart;
            if (Source == item) return Constants.SourcePart;
            if (Target == item) return Constants.TargetPart;
            return -1;
        }

        public void Insert(int index, ulong item) => Throw.NotSupportedException();

        public void RemoveAt(int index) => Throw.NotSupportedException();

        public ulong this[int index]
        {
            get
            {
                if (index == Constants.IndexPart)
                    return Index;
                if (index == Constants.SourcePart)
                    return Source;
                if (index == Constants.TargetPart)
                    return Target;
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            set { throw new NotSupportedException(); }
        }

        #endregion
    }
}