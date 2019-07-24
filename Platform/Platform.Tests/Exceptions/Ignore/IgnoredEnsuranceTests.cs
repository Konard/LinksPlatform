using Xunit;
using Platform.Exceptions;

namespace Platform.Tests.Exceptions.Ignore
{
    public static class IgnoredEnsuranceTests
    {
        [Fact]
        public static void EnsuranceIgnoredTest()
        {
            // Should not throw an exception (because logic is overriden in EnsureAlwaysExtensions that is located within the same namespace)
            // And even should be optimized out at RELEASE (because method is now marked conditional DEBUG)
            // This can be useful in performance critical situations there even an check for exception is hurting performance enough
            Ensure.Always.ArgumentNotNull<object>(null, "object");
        }
    }
}
