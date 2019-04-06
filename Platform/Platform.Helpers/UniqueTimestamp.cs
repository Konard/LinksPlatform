using System;
using System.Threading;

namespace Platform.Helpers
{
    /// <summary>
    /// Представляет уникальную отметку времени.
    /// </summary>
    /// <remarks>
    /// Чтобы эта метка была дейстительно уникальна рекомендуется использовать <see cref="UniqueTimestampFactory"/>.
    /// </remarks>
    public struct UniqueTimestamp
    {
        public const string Format = "yyyy.MM.dd hh:mm:ss.fffffff";

        /// <summary>
        /// Возвращает или устанавливает количество тиков, которые представляют дату и время в UTC.
        /// </summary>
        public ulong Ticks;

        public UniqueTimestamp(ulong ticks)
        {
            Ticks = ticks;
        }

        /// <summary>
        /// Создаёт и возвращает следующую отметку времени, но всегда после последней. | Creates and returns the next timestamp, but always after the last one.
        /// </summary>
        /// <param name="lastTicks">Последняя отметка времени в тиках. | Last timestamp in ticks.</param>
        /// <returns>Уникальную отметку времени, следующую сразу после последней. | A unique timestamp immediately following the last one.</returns>
        static public UniqueTimestamp CreateNext(ref long lastTicks)
        {
            var next = DateTime.UtcNow.Ticks;
            if (next <= lastTicks)
                next = Interlocked.Increment(ref lastTicks);
            else
                Interlocked.Exchange(ref lastTicks, next);
            return new UniqueTimestamp((ulong)next);
        }

        public override string ToString() => new DateTime((long)Ticks).ToString(Format);
    }
}
