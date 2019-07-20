namespace Platform.Helpers.Numbers
{
    public class ArithmeticHelpers
    {
        public static T Add<T>(T x, T y) => ArithmeticHelpers<T>.Add(x, y);
        public static T And<T>(T x, T y) => ArithmeticHelpers<T>.And(x, y);
        public static T Increment<T>(T x) => ArithmeticHelpers<T>.Increment(x);
        public static T Subtract<T>(T x, T y) => ArithmeticHelpers<T>.Subtract(x, y);
        public static T Subtract<T>(Integer<T> x, Integer<T> y) => ArithmeticHelpers<T>.Subtract(x, y);
        public static T Decrement<T>(T x) => ArithmeticHelpers<T>.Decrement(x);
    }
}
