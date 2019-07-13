using Xunit;
using Platform.Helpers.Numbers;

namespace Platform.Tests.Helpers
{
    public class MathExtensionsTests
    {
        [Fact]
        public void IncrementTest()
        {
            ulong number = 0;

            var returnValue = number.Increment();

            Assert.Equal(1UL, returnValue);
            Assert.Equal(1UL, number);
        }

        [Fact]
        public void DecrementTest()
        {
            ulong number = 1;

            var returnValue = number.Decrement();

            Assert.Equal(0UL, returnValue);
            Assert.Equal(0UL, number);
        }

        [Fact]
        public void AbsTest()
        {
            long number = -1L;

            var returnValue = number.Abs();

            Assert.Equal(1L, returnValue);
            Assert.Equal(1L, number);
        }

        [Fact]
        public void NegateTest()
        {
            long number = 2;

            var returnValue = number.Negate();

            Assert.Equal(-2L, returnValue);
            Assert.Equal(-2L, number);
        }
    }
}
