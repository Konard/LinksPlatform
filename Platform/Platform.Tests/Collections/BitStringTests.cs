using System.Collections;
using Xunit;
using Platform.Collections;
using Platform.Random;

namespace Platform.Tests.Collections
{
    public class BitStringTests
    {
        [Fact]
        public void BitGetSetTest()
        {
            const int n = 250;

            var array = new BitArray(n);
            var str = new BitString(n);

            for (var i = 0; i < n; i++)
            {
                var value = RandomHelpers.Default.Next(0, 2) == 0;
                array.Set(i, value);
                str.Set(i, value);

                Assert.True(array.Get(i) == value);
                Assert.True(str.Get(i) == value);
            }
        }
    }
}
