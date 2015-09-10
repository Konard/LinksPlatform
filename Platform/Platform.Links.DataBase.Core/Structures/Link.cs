using System;

namespace Platform.Links.DataBase.Core.Structures
{
    /// <summary>
    /// Структура описывающая уникальную связь.
    /// </summary>
    public struct Link : IEquatable<Link>
    {
        public readonly ulong Source;
        public readonly ulong Target;

        public Link(ulong source, ulong target)
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
            return new Link((ulong) link.Source.GetHashCode(), (ulong) link.Target.GetHashCode());
        }

        public override int GetHashCode()
        {
            var hash = 17;
            hash = hash*31 + (int) Source;
            hash = hash*31 + (int) Target;
            return hash;
        }

        public override bool Equals(object other)
        {
            return other is Link && Equals((Link) other);
        }

        public bool Equals(Link other)
        {
            return Source == other.Source &&
                   Target == other.Target;
        }

        public static string ToString(ILink link)
        {
            return ToString((ulong) link.Source.GetHashCode(), (ulong) link.Target.GetHashCode());
        }

        private static string ToString(ulong source, ulong target)
        {
            return string.Format("({0}->{1})", source, target);
        }

        public override string ToString()
        {
            return ToString(Source, Target);
        }
    }
}