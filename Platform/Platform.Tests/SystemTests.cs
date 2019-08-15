using System;
using Xunit;

namespace Platform.Tests
{
    /// <summary>
    /// Contains tests for features of .NET Framework itself, that are required by current implementations.
    /// </summary>
    public class SystemTests
    {
        [Fact]
        public void UsingSupportsNullTest()
        {
            using (var disposable = null as IDisposable)
            {
                Assert.True(disposable == null);
            }
        }
    }
}
