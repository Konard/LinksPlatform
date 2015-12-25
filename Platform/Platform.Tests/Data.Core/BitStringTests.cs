using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Platform.Data.Core;
using Platform.Data.Core.Collections;
using Platform.Helpers;

namespace Platform.Tests.Data.Core
{
    [TestClass]
    public class BitStringTests
    {
        [TestMethod]
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

                Assert.IsTrue(array.Get(i) == value);
                Assert.IsTrue(str.Get(i) == value);
            }
        }
    }
}
