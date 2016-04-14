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
        public void GetLowestBitPosition(ulong value, int expectedPosition)
        {
            Assert.True(MathHelpers.GetLowestBitPosition(value) == expectedPosition);
        }
    }
}
