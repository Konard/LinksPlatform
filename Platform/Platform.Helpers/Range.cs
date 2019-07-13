using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Platform.Helpers
{
    /// <remarks>
    /// Based on http://stackoverflow.com/questions/5343006/is-there-a-c-sharp-type-for-representing-an-integer-range
    /// </remarks>
    public struct Range<T>
    {
        /// <summary>
        /// Minimum value of the range.
        /// </summary>
        public readonly T Minimum;

        /// <summary>
        /// Maximum value of the range.
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
        /// </summary>
        /// <returns>String representation of the Range</returns>
        public override string ToString() => $"[{Minimum}, {Maximum}]";

        /// <summary>
        /// Determines if the range is valid.
        /// </summary>
        /// <returns>True if range is valid, else false.</returns>
        public bool IsValid() => Compare(Minimum, Maximum) <= 0;

        /// <summary>
        /// Determines if the provided value is inside the range.
        /// </summary>
        /// <param name="value">The value to test</param>
        /// <returns>True if the value is inside Range, else false.</returns>
        public bool ContainsValue(T value) => (Compare(Minimum, value) <= 0) && (Compare(Maximum, value) >= 0);

        /// <summary>
        /// Determines if this Range is inside the bounds of another range.
        /// </summary>
        /// <param name="range">The parent range to test on.</param>
        /// <returns>True if range is inclusive, else false.</returns>
        public bool IsInsideRange(Range<T> range) => range.ContainsRange(this);

        /// <summary>
        /// Determines if another range is inside the bounds of this range.
        /// </summary>
        /// <param name="range">The child range to test.</param>
        /// <returns>True if range is inside, else false.</returns>
        public bool ContainsRange(Range<T> range) => IsValid() && range.IsValid() && ContainsValue(range.Minimum) && ContainsValue(range.Maximum);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Compare(T x, T y) => Comparer<T>.Default.Compare(x, y);
    }
}
