using System.Collections;
using Xunit;
using Platform.Data.Core.Collections;
using Platform.Helpers;

namespace Platform.Tests.Data.Core
{
    public class BitStringTests
    {
        static BitStringTests()
        {
            BitString.Init();
        }

        [Fact]
        public void BitGetSetTest()
        {
            const int n = 250;

            var array = new BitArray(n);
            var str = new BitString(n);

            for (var i = 0; i < n; i++)
            {
                var value = RandomHelpers.DefaultFactory.Next(0, 2) == 0;
                array.Set(i, value);
                str.Set(i, value);

                Assert.True(array.Get(i) == value);
                Assert.True(str.Get(i) == value);
            }
        }
    }
}
