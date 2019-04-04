using System;
using System.Collections;
using System.Collections.Generic;
using Platform.Helpers;

namespace Platform.Data.Core.Doublets
{
    /// <summary>
    /// Структура описывающая уникальную связь.
    /// </summary>
    public struct Link<T> : IEquatable<Link<T>>, IList<T>
    {
        private const int Length = 3;

        public readonly T Index;
        public readonly T Source;
        public readonly T Target;

        public static readonly Link<T> Null = new Link<T>();

        private static readonly LinksConstants<bool, T, int> Constants = Default<LinksConstants<bool, T, int>>.Instance;

        public Link(params T[] values)
        {
            Index = values.Length > Constants.IndexPart ? values[Constants.IndexPart] : Constants.Null;
            Source = values.Length > Constants.SourcePart ? values[Constants.SourcePart] : Constants.Null;
            Target = values.Length > Constants.TargetPart ? values[Constants.TargetPart] : Constants.Null;
        }

        public Link(IList<T> values)
        {
            Index = values.Count > Constants.IndexPart ? values[Constants.IndexPart] : Constants.Null;
            Source = values.Count > Constants.SourcePart ? values[Constants.SourcePart] : Constants.Null;
            Target = values.Count > Constants.TargetPart ? values[Constants.TargetPart] : Constants.Null;
        }

        public Link(T index, T source, T target)
        {
            Index = index;
            Source = source;
            Target = target;
        }

        public Link(T source, T target)
            : this(Constants.Null, source, target)
        {
            Source = source;
            Target = target;
        }

        public static Link<T> Create(T source, T target) => new Link<T>(source, target);

        public override int GetHashCode()
        {
            var hash = 17;
            hash = hash * 31 + Index.GetHashCode();
            hash = hash * 31 + Source.GetHashCode();
            hash = hash * 31 + Target.GetHashCode();
            return hash;
        }

        public bool IsNull() => Equals(Index, Constants.Null) && Equals(Source, Constants.Null) && Equals(Target, Constants.Null);

        public override bool Equals(object other) => other is Link<T> && Equals((Link<T>)other);

        public bool Equals(Link<T> other) => Equals(Index, other.Index) &&
                                             Equals(Source, other.Source) &&
                                             Equals(Target, other.Target);

        public static string ToString(T index, T source, T target) => $"({index}: {source}->{target})";

        public static string ToString(T source, T target) => $"({source}->{target})";

        public static implicit operator T[] (Link<T> link) => link.ToArray();

        public static implicit operator Link<T>(T[] linkArray) => new Link<T>(linkArray);

        #region IList

        public override string ToString() => Equals(Index, Constants.Null) ? ToString(Source, Target) : ToString(Index, Source, Target);

        public T[] ToArray()
        {
            var array = new T[Length];
            CopyTo(array, 0);
            return array;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<T> GetEnumerator()
        {
            yield return Index;
            yield return Source;
            yield return Target;
        }

        public void Add(T item) => Throw.NotSupportedException();

        public void Clear() => Throw.NotSupportedException();

        public bool Contains(T item) => IndexOf(item) > 0;

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (arrayIndex + Length > array.Length) throw new ArgumentException();

            array[arrayIndex++] = Index;
            array[arrayIndex++] = Source;
            array[arrayIndex] = Target;
        }

        public bool Remove(T item) => Throw.NotSupportedExceptionAndReturn<bool>();

        public int Count => Length;
        public bool IsReadOnly => true;

        public int IndexOf(T item)
        {
            if (Equals(Index, item)) return Constants.IndexPart;
            if (Equals(Source, item)) return Constants.SourcePart;
            if (Equals(Target, item)) return Constants.TargetPart;
            return -1;
        }

        public void Insert(int index, T item) => Throw.NotSupportedException();

        public void RemoveAt(int index) => Throw.NotSupportedException();

        public T this[int index]
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
