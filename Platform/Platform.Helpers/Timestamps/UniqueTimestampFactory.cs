using System;
using Platform.Interfaces;

namespace Platform.Helpers.Timestamps
{
    /// <summary>
    /// Represents a factory for creating unique timestamps.
    /// Представляет фабрику по созданию уникальных отметок времени.
    /// </summary>
    public class UniqueTimestampFactory : IFactory<Timestamp>
    {
        private ulong _lastTicks = 0;

        /// <summary>
        /// Creates a timestamp corresponding to the current UTC date and time or next unique timestamp
        /// Создаёт отмеку времени соответствующую текущей дате и времени по UTC или следующую уникальную отметку времени.
        /// </summary>
        public Timestamp Create()
        {
            var utcTicks = (ulong)DateTime.UtcNow.Ticks;
            if (utcTicks <= _lastTicks)
                return new Timestamp(_lastTicks++);
            else
                return new Timestamp(utcTicks);
        }
    }
}
