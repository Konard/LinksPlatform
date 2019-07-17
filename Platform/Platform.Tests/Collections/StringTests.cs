using Xunit;
using Platform.Helpers.Collections;

namespace Platform.Tests.Collections
{
    public class StringTests
    {
        [Fact]
        public static void CapitalizeFirstLetterTest()
        {
            var source1 = "hello";

            var result1 = source1.CapitalizeFirstLetter();

            Assert.Equal("Hello", result1);

            var source2 = "Hello";

            var result2 = source2.CapitalizeFirstLetter();

            Assert.Equal("Hello", result2);

            var source3 = "  hello";

            var result3 = source3.CapitalizeFirstLetter();

            Assert.Equal("  Hello", result3);
        }
    }
}
