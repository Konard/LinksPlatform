namespace Platform.Helpers.Numbers
{
    public static class ArithmeticExtensions
    {
        public static T Decrement<T>(this ref T x) where T : struct => x = ArithmeticHelpers<T>.Decrement(x);
        public static T Increment<T>(this ref T x) where T : struct => x = ArithmeticHelpers<T>.Increment(x);
    }
}
