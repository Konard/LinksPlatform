using System;

namespace Platform.Helpers
{
    /// <summary>
    /// Представляет фабрику по созданию уникальных отметок времени.
    /// </summary>
    public class UniqueTimestampFactory : IFactory<UniqueTimestamp>
    {
        private ulong _lastTicks = 0;

        /// <summary>
        /// Создаёт отмеку времени соответствующую текущей дате и времени по UTC.
        /// </summary>
        public UniqueTimestamp Create()
        {
            var utcTicks = (ulong)DateTime.UtcNow.Ticks;
            if (utcTicks <= _lastTicks)
                return new UniqueTimestamp(_lastTicks++);
            else
                return new UniqueTimestamp(utcTicks);
        }
    }
}
