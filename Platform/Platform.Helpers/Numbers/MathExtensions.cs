namespace Platform.Helpers.Numbers
{
    public static class MathExtensions
    {
        public static T Abs<T>(this ref T x) where T : struct => x = MathHelpers<T>.Abs(x);
        public static T Negate<T>(this ref T x) where T : struct => x = MathHelpers<T>.Negate(x);
    }
}
