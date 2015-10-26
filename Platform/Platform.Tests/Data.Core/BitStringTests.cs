using System;
using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Platform.Data.Core;

namespace Platform.Tests.Data.Core
{
    [TestClass]
    public class BitStringTests
    {
        static private readonly Random Randomizer = new Random((int)DateTime.UtcNow.Ticks);

        [TestMethod]
        public void BitGetSetTest()
        {
            const int n = 250;

            var array = new BitArray(n);
            var str = new BitString(n);

            for (var i = 0; i < n; i++)
            {
                var value = Randomizer.Next(0, 2) == 0;
                array.Set(i, value);
                str.Set(i, value);

                Assert.IsTrue(array.Get(i) == value);
                Assert.IsTrue(str.Get(i) == value);
            }
        }
    }
}
