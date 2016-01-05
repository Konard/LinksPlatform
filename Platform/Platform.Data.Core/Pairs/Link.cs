using System;
using System.Collections;
using System.Collections.Generic;

namespace Platform.Data.Core.Pairs
{
    /// <summary>
    /// Структура описывающая уникальную связь.
    /// TODO: Возможно Index тоже должен быть частью этой структуры, можно попробовать реализовать IList[ulong], тогда структура сможет восприниматься как массив значений связи
    /// </summary>
    public struct Link : IEquatable<Link>, IList<ulong>
    {
        private const int Length = 3;

        public readonly ulong Index;
        public readonly ulong Source;
        public readonly ulong Target;

        public static readonly Link Null = new Link();

        public Link(params ulong[] values)
        {
            Index = values.Length > LinksConstants.IndexPart ? values[LinksConstants.IndexPart] : LinksConstants.Null;
            Source = values.Length > LinksConstants.SourcePart ? values[LinksConstants.SourcePart] : LinksConstants.Any;
            Target = values.Length > LinksConstants.TargetPart ? values[LinksConstants.TargetPart] : LinksConstants.Any;
        }

        public Link(ulong index, ulong source, ulong target)
        {
            Index = index;
            Source = source;
            Target = target;
        }

        public Link(ulong source, ulong target)
            : this(LinksConstants.Null, source, target)
        {
            Source = source;
            Target = target;
        }

        public static Link Create(ulong source, ulong target)
        {
            return new Link(source, target);
        }

        public static Link Create(ILink link)
        {
            return new Link((ulong)link.Source.GetHashCode(), (ulong)link.Target.GetHashCode());
        }

        public override int GetHashCode()
        {
            var hash = 17;
            hash = hash * 31 + Index.GetHashCode();
            hash = hash * 31 + Source.GetHashCode();
            hash = hash * 31 + Target.GetHashCode();
            return hash;
        }

        public bool IsNull()
        {
            return Index == LinksConstants.Null && Source == LinksConstants.Any && Target == LinksConstants.Any;
        }

        public override bool Equals(object other)
        {
            return other is Link && Equals((Link)other);
        }

        public bool Equals(Link other)
        {
            return Index == other.Index &&
                   Source == other.Source &&
                   Target == other.Target;
        }

        public static string ToString(ILink link)
        {
            return ToString((ulong)link.Source.GetHashCode(), (ulong)link.Target.GetHashCode());
        }

        public static string ToString(ulong index, ulong source, ulong target)
        {
            return string.Format("({0}: {1}->{2})", index, source, target);
        }

        public static string ToString(ulong source, ulong target)
        {
            return string.Format("({0}->{1})", source, target);
        }

        public static implicit operator ulong[](Link link)
        {
            return link.ToArray();
        }

        public static implicit operator Link(ulong[] linkArray)
        {
            return new Link(linkArray);
        }

        #region IList

        public override string ToString()
        {
            return Index == LinksConstants.Null ? ToString(Source, Target) : ToString(Index, Source, Target);
        }

        public ulong[] ToArray()
        {
            var array = new ulong[Length];
            CopyTo(array, 0);
            return array;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<ulong> GetEnumerator()
        {
            yield return Index;
            yield return Source;
            yield return Target;
        }

        public void Add(ulong item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(ulong item)
        {
            return IndexOf(item) > 0;
        }

        public void CopyTo(ulong[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException("array");
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException("arrayIndex");
            if (arrayIndex + Length >= array.Length) throw new ArgumentException();

            array[arrayIndex++] = Index;
            array[arrayIndex++] = Source;
            array[arrayIndex] = Target;
        }

        public bool Remove(ulong item)
        {
            throw new NotSupportedException();
        }

        public int Count { get { return Length; } }
        public bool IsReadOnly { get { return true; } }

        public int IndexOf(ulong item)
        {
            if (Index == item) return (int)LinksConstants.IndexPart;
            if (Source == item) return (int)LinksConstants.SourcePart;
            if (Target == item) return (int)LinksConstants.TargetPart;
            return -1;
        }

        public void Insert(int index, ulong item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public ulong this[int index]
        {
            get
            {
                switch (index)
                {
                    case (int)LinksConstants.IndexPart:
                        return Index;
                    case (int)LinksConstants.SourcePart:
                        return Source;
                    case (int)LinksConstants.TargetPart:
                        return Target;
                    default:
                        throw new ArgumentOutOfRangeException("index");
                }
            }
            set { throw new NotSupportedException(); }
        }

        #endregion
    }
}