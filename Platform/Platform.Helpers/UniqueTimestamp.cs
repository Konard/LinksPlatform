using System;

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

        public override string ToString()
        {
            return new DateTime((long)Ticks).ToString(Format);
        }
    }
}
