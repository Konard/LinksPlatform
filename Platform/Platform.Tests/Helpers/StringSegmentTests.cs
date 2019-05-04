using Platform.Helpers.Collections;
using Xunit;

namespace Platform.Tests.Helpers
{
    public class StringSegmentTests
    {
        [Fact]
        public static void GetHashCodeEqualsTest()
        {
            const string testString = "test test";

            var testArray = testString.ToCharArray();

            var first = new StringSegment(testArray, 0, 4);
            var firstHashCode = first.GetHashCode();

            var second = new StringSegment(testArray, 5, 4);
            var secondHashCode = second.GetHashCode();

            Assert.True(firstHashCode == secondHashCode);
        }

        [Fact]
        public static void EqualsTest()
        {
            const string testString = "test test";

            var testArray = testString.ToCharArray();

            var first = new StringSegment(testArray, 0, 4);
            var second = new StringSegment(testArray, 5, 4);

            Assert.True(first.Equals(second));
        }
    }
}
