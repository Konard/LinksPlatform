using System;

namespace Platform.Helpers
{
    /// <summary>
    /// Представляет фабрику по созданию уникальных отметок времени.
    /// </summary>
    public class UniqueTimestampFactory
    {
        private UniqueTimestamp _lastTimestamp;

        /// <summary>
        /// Создаёт отмеку времени соответствующую текущей дате и времени по UTC.
        /// </summary>
        public UniqueTimestamp Create()
        {
            var utcNow = DateTime.UtcNow;
            var utcTicks = (ulong)utcNow.Ticks;

            if (utcTicks <= _lastTimestamp.Ticks)
            {
                _lastTimestamp.Ticks++;
                return _lastTimestamp;
            }

            return _lastTimestamp = new UniqueTimestamp(utcTicks);
        }
    }
}
