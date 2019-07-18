using System;
using System.Reflection;
using Xunit;
using Platform.Collections;
using Platform.Collections.Lists;
using Platform.Reflection;

namespace Platform.Tests.Helpers
{
    public class ReflectionTests
    {
        [Fact]
        public void ILBytesForDelegateAreAvailableTest()
        {
            var x = new Func<object, int>(y => 0);

            var bytes = x.GetMethodInfo().GetILBytes();

            Assert.False(bytes.IsNullOrEmpty());
        }

        [Fact]
        public void ILBytesForDifferentDelegatesAreTheSameTest()
        {
            var x = new Func<object, int>(y => 0);

            var z = new Func<object, int>(y => 0);

            Assert.False(x == z);

            var xBytes = x.GetMethodInfo().GetILBytes();
            Assert.False(xBytes.IsNullOrEmpty());

            var zBytes = x.GetMethodInfo().GetILBytes();
            Assert.False(zBytes.IsNullOrEmpty());

            Assert.True(xBytes.EqualTo(zBytes));
        }
    }
}
