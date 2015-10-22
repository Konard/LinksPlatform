using System;
using Platform.Helpers;

namespace Platform.Data.Core.Structures
{
    /// <summary>
    /// Структура описывающая уникальную пространственно-временную зависимость.
    /// </summary>
    public struct Unique : IEquatable<Unique>
    {
        private const string DateFormat = "yyyy.MM.dd hh:mm:ss.fffffff";

        private static readonly Random SeedFactory = new Random((int) DateTime.UtcNow.Ticks);
        public static readonly Unique Null = new Unique(0, 0);

        public readonly ulong Time;
        public readonly ulong Seed;

        public Unique(ulong time, ulong seed)
        {
            Time = time;
            Seed = seed;
        }

        public static Unique Create()
        {
            var time = (ulong) DateTime.UtcNow.Ticks;

            //var rndBytes = new byte[8];
            //SeedFactory.NextBytes(rndBytes);
            //var seed = BitConverter.ToUInt64(rndBytes, 0);

            // TODO: Сравнить производительность
            var seed = SeedFactory.NextUInt64();

            return new Unique(time, seed);
        }

        public static Unique Create(ulong source, ulong target)
        {
            return new Unique(source, target);
        }

        public override int GetHashCode()
        {
            var hash = 17;
            hash = hash*31 + (int) Time;
            hash = hash*31 + (int) Seed;
            return hash;
        }

        public bool IsNull()
        {
            return Time == 0 && Seed == 0;
        }

        public override bool Equals(object other)
        {
            return other is Unique && Equals((Unique) other);
        }

        public bool Equals(Unique other)
        {
            return Time == other.Time &&
                   Seed == other.Seed;
        }

        public override string ToString()
        {
            return string.Format("[{0},{1}]", new DateTime((long) Time).ToString(DateFormat), Seed);
        }
    }
}