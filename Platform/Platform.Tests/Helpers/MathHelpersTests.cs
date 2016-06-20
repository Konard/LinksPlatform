using System;
using Platform.Helpers;
using Xunit;

namespace Platform.Tests.Helpers
{
    public class MathHelpersTests
    {
        [Theory]
        [InlineData(00, -1)] // 0000 0000 (none, -1)
        [InlineData(01, 00)] // 0000 0001 (first, 0)
        [InlineData(08, 03)] // 0000 1000 (forth, 3)
        [InlineData(88, 03)] // 0101 1000 (forth, 3)
        public void GetLowestBitPositionTest(ulong value, int expectedPosition)
        {
            Assert.True(MathHelpers.GetLowestBitPosition(value) == expectedPosition);
        }

        [Fact]
        public void CompiledOperationsTest()
        {
            Assert.True(MathHelpers<short>.Add(1, 2) == 3);
            Assert.True(MathHelpers<byte>.Increment(1) == 2);
            Assert.True(MathHelpers<ulong>.Decrement(2) == 1);
            Assert.True(Math.Abs(MathHelpers<double>.Subtract(3D, 2D) - 1D) < 0.01);
            Assert.True(MathHelpers<sbyte>.Equals(2, 2));
            Assert.True(MathHelpers<sbyte>.GreaterThan(3, 2));
            Assert.True(MathHelpers<sbyte>.GreaterOrEqualThan(5, 5));
            Assert.True(MathHelpers<sbyte>.GreaterOrEqualThan(5, 1));
            Assert.True(MathHelpers<sbyte>.LessThan(4, 5));
            Assert.True(MathHelpers<sbyte>.LessOrEqualThan(5, 5));
            Assert.True(MathHelpers<sbyte>.LessOrEqualThan(1, 5));
            Assert.Throws<NotSupportedException>(() => MathHelpers<string>.Subtract("1", "2"));
        }
    }
}
