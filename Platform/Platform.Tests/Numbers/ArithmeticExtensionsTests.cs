using Xunit;
using Platform.Numbers;

namespace Platform.Tests.Numbers
{
    public class ArithmeticExtensionsTests
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
    }
}
