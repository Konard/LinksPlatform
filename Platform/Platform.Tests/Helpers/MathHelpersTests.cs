using System;
using Xunit;
using Platform.Helpers.Numbers;

namespace Platform.Tests.Helpers
{
    public class MathHelpersTests
    {
        [Fact]
        public void CompiledOperationsTest()
        {
            Assert.True(Math.Abs(ArithmeticHelpers<double>.Subtract(3D, 2D) - 1D) < 0.01);
        }
    }
}
