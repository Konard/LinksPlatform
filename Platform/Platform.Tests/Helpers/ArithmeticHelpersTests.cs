using System;
using Xunit;
using Platform.Helpers.Numbers;

namespace Platform.Tests.Helpers
{
    public class ArithmeticHelpersTests
    {
        [Fact]
        public void CompiledOperationsTest()
        {
            Assert.True(ArithmeticHelpers<short>.Add(1, 2) == 3);
            Assert.True(ArithmeticHelpers<byte>.Increment(1) == 2);
            Assert.True(ArithmeticHelpers<ulong>.Decrement(2) == 1);
            Assert.Throws<NotSupportedException>(() => ArithmeticHelpers<string>.Subtract("1", "2"));
        }
    }
}
