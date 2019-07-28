using System;
using System.Collections;
using System.Collections.Generic;
using Platform.Exceptions;
using Platform.Helpers.Singletons;

namespace Platform.Data.Core.Doublets
{
    /// <summary>
    /// Структура описывающая уникальную связь.
    /// </summary>
    public struct Link<TLink> : IEquatable<Link<TLink>>, IList<TLink>
    {
        public static readonly Link<TLink> Null = new Link<TLink>();

        private static readonly LinksConstants<bool, TLink, int> Constants = Default<LinksConstants<bool, TLink, int>>.Instance;
        private static readonly EqualityComparer<TLink> EqualityComparer = EqualityComparer<TLink>.Default;

        private const int Length = 3;

        public readonly TLink Index;
        public readonly TLink Source;
        public readonly TLink Target;

        public Link(params TLink[] values)
        {
            Index = values.Length > Constants.IndexPart ? values[Constants.IndexPart] : Constants.Null;
            Source = values.Length > Constants.SourcePart ? values[Constants.SourcePart] : Constants.Null;
            Target = values.Length > Constants.TargetPart ? values[Constants.TargetPart] : Constants.Null;
        }

        public Link(IList<TLink> values)
        {
            Index = values.Count > Constants.IndexPart ? values[Constants.IndexPart] : Constants.Null;
            Source = values.Count > Constants.SourcePart ? values[Constants.SourcePart] : Constants.Null;
            Target = values.Count > Constants.TargetPart ? values[Constants.TargetPart] : Constants.Null;
        }

        public Link(TLink index, TLink source, TLink target)
        {
            Index = index;
            Source = source;
            Target = target;
        }

        public Link(TLink source, TLink target)
            : this(Constants.Null, source, target)
        {
            Source = source;
            Target = target;
        }

        public static Link<TLink> Create(TLink source, TLink target) => new Link<TLink>(source, target);

        public override int GetHashCode()
        {
            var hash = 17;
            hash = hash * 31 + Index.GetHashCode();
            hash = hash * 31 + Source.GetHashCode();
            hash = hash * 31 + Target.GetHashCode();
            return hash;
        }

        public bool IsNull() => EqualityComparer.Equals(Index, Constants.Null) && EqualityComparer.Equals(Source, Constants.Null) && EqualityComparer.Equals(Target, Constants.Null);

        public override bool Equals(object other) => other is Link<TLink> && Equals((Link<TLink>)other);

        public bool Equals(Link<TLink> other) => EqualityComparer.Equals(Index, other.Index) &&
                                             EqualityComparer.Equals(Source, other.Source) &&
                                             EqualityComparer.Equals(Target, other.Target);

        public static string ToString(TLink index, TLink source, TLink target) => $"({index}: {source}->{target})";

        public static string ToString(TLink source, TLink target) => $"({source}->{target})";

        public static implicit operator TLink[] (Link<TLink> link) => link.ToArray();

        public static implicit operator Link<TLink>(TLink[] linkArray) => new Link<TLink>(linkArray);

        #region IList

        public override string ToString() => EqualityComparer.Equals(Index, Constants.Null) ? ToString(Source, Target) : ToString(Index, Source, Target);

        public TLink[] ToArray()
        {
            var array = new TLink[Length];
            CopyTo(array, 0);
            return array;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<TLink> GetEnumerator()
        {
            yield return Index;
            yield return Source;
            yield return Target;
        }

        public void Add(TLink item) => throw new NotSupportedException();

        public void Clear() => throw new NotSupportedException();

        public bool Contains(TLink item) => IndexOf(item) > 0;

        public void CopyTo(TLink[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (arrayIndex + Length > array.Length) throw new ArgumentException();

            array[arrayIndex++] = Index;
            array[arrayIndex++] = Source;
            array[arrayIndex] = Target;
        }

        public bool Remove(TLink item) => Throw.A.NotSupportedExceptionAndReturn<bool>();

        public int Count => Length;
        public bool IsReadOnly => true;

        public int IndexOf(TLink item)
        {
            if (EqualityComparer.Equals(Index, item)) return Constants.IndexPart;
            if (EqualityComparer.Equals(Source, item)) return Constants.SourcePart;
            if (EqualityComparer.Equals(Target, item)) return Constants.TargetPart;
            return -1;
        }

        public void Insert(int index, TLink item) => throw new NotSupportedException();

        public void RemoveAt(int index) => throw new NotSupportedException();

        public TLink this[int index]
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
            set => throw new NotSupportedException();
        }

        #endregion
    }
}
