using Platform.Data.Core.Pairs;
using Platform.Helpers;
using Platform.Memory;
using Xunit;

namespace Platform.Tests.Helpers
{
    public class ScopeTests
    {
        [Fact]
        public void SingleDependencyTest()
        {
            using (var scope = new Scope())
            {
                scope.IncludeAssemblyOf<IMemory>();
                var instance = scope.Use<IDirectMemory>();
                Assert.IsType<HeapResizableDirectMemory>(instance);
            }
        }

        [Fact]
        public void CascadeDependencyTest()
        {
            using (var scope = new Scope())
            {
                scope.Include<TemporaryFileMappedResizableDirectMemory>();
                scope.Include<UInt64LinksMemoryManager>();
                scope.Include<UInt64Links>();
                var instance = scope.Use<ILinks<ulong>>();
                Assert.IsType<UInt64Links>(instance);
            }
        }

        [Fact]
        public void FullAutoResolutionTest()
        {
            using (var scope = new Scope(autoInclude: true, autoExplore: true))
            {
                var instance = scope.Use<UInt64Links>();
                Assert.IsType<UInt64Links>(instance);
            }
        }
    }
}
