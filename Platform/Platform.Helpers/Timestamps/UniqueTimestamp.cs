using System;

namespace Platform.Helpers.Timestamps
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

        public UniqueTimestamp(ulong ticks) => Ticks = ticks;

        public static implicit operator UniqueTimestamp(DateTime dateTime) => new UniqueTimestamp((ulong)dateTime.ToUniversalTime().Ticks);

        public static implicit operator DateTime(UniqueTimestamp timestamp) => new DateTime((long)timestamp.Ticks, DateTimeKind.Utc);

        public override string ToString() => ((DateTime)this).ToString(Format);
    }
}
