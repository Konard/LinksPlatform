using System.Collections.Generic;

namespace Platform.Helpers
{
    /// <summary>
    /// Represents a range between minumum and maximum values.
    /// Представляет диапазон между минимальным и максимальным значениями.
    /// </summary>
    /// <remarks>
    /// Based on http://stackoverflow.com/questions/5343006/is-there-a-c-sharp-type-for-representing-an-integer-range
    /// </remarks>
    public struct Range<T>
    {
        private static readonly Comparer<T> _comparer = Comparer<T>.Default;

        /// <summary>
        /// Returns minimum value of the range.
        /// Возвращает минимальное значение диапазона.
        /// </summary>
        public readonly T Minimum;

        /// <summary>
        /// Returns maximum value of the range.
        /// Возвращает максимальное значение диапазона.
        /// </summary>
        public readonly T Maximum;

        public Range(T minimumAndMaximum)
        {
            Minimum = minimumAndMaximum;
            Maximum = minimumAndMaximum;
        }

        public Range(T minimum, T maximum)
        {
            Minimum = minimum;
            Maximum = maximum;
        }

        /// <summary>
        /// Presents the Range in readable format.
        /// Представляет диапазон в удобном для чтения формате.
        /// </summary>
        /// <returns>String representation of the Range. Строковое представление диапазона.</returns>
        public override string ToString() => $"[{Minimum}, {Maximum}]";

        /// <summary>
        /// Determines if the range is valid. Определяет, является ли диапазон корректным.
        /// </summary>
        /// <returns>True if range is valid, else false. True, если диапазон корректный, иначе false.</returns>
        public bool IsValid() => _comparer.Compare(Minimum, Maximum) <= 0;

        /// <summary>
        /// Determines if the provided value is inside the range.
        /// Определяет, находится ли указанное значение внутри диапазона.
        /// </summary>
        /// <param name="value">The value to test. Значение для проверки.</param>
        /// <returns>True if the value is inside Range, else false. True, если значение находится внутри диапазона, иначе false.</returns>
        public bool ContainsValue(T value) => (_comparer.Compare(Minimum, value) <= 0) && (_comparer.Compare(Maximum, value) >= 0);

        /// <summary>
        /// Determines if this Range is inside the bounds of another range.
        /// Определяет, находится ли этот диапазон в пределах другого диапазона.
        /// </summary>
        /// <param name="range">The parent range to test on. Родительский диапазон для проверки.</param>
        /// <returns>True if range is inclusive, else false. True, если диапазон включен, иначе false.</returns>
        public bool IsInsideRange(Range<T> range) => range.ContainsRange(this);

        /// <summary>
        /// Determines if another range is inside the bounds of this range.
        /// Определяет, находится ли другой диапазон внутри границ этого диапазона.
        /// </summary>
        /// <param name="range">The child range to test. Дочерний диапазон для проверки.</param>
        /// <returns>True if range is inside, else false. True, если диапазон находится внутри, иначе false.</returns>
        public bool ContainsRange(Range<T> range) => IsValid() && range.IsValid() && ContainsValue(range.Minimum) && ContainsValue(range.Maximum);
    }
}
