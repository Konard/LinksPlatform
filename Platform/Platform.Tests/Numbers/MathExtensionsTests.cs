using Xunit;
using Platform.Numbers;

namespace Platform.Tests.Numbers
{
    public class MathExtensionsTests
    {
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
