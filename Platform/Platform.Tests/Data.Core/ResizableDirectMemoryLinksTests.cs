using System.IO;
using Xunit;
using Platform.Helpers.Singletons;
using Platform.Memory;
using Platform.Data.Core.Doublets;
using Platform.Data;
using Platform.Data.Constants;

namespace Platform.Tests.Data.Core
{
    public static class ResizableDirectMemoryLinksTests
    {
        private static readonly LinksConstants<ulong, ulong, int> Constants = Default<LinksConstants<ulong, ulong, int>>.Instance;

        [Fact]
        public static void BasicFileMappedMemoryTest()
        {
            var tempFilename = Path.GetTempFileName();

            using (var memoryAdapter = new UInt64ResizableDirectMemoryLinks(tempFilename))
            {
                memoryAdapter.TestBasicMemoryOperations();
            }

            File.Delete(tempFilename);
        }

        [Fact]
        public static void BasicHeapMemoryTest()
        {
            using (var memory = new HeapResizableDirectMemory(UInt64ResizableDirectMemoryLinks.DefaultLinksSizeStep))
            using (var memoryAdapter = new UInt64ResizableDirectMemoryLinks(memory, UInt64ResizableDirectMemoryLinks.DefaultLinksSizeStep))
            {
                memoryAdapter.TestBasicMemoryOperations();
            }
        }

        private static void TestBasicMemoryOperations(this ILinks<ulong> memoryAdapter)
        {
            var link = memoryAdapter.Create();
            memoryAdapter.Delete(link);
        }

        [Fact]
        public static void NonexistentReferencesHeapMemoryTest()
        {
            using (var memory = new HeapResizableDirectMemory(UInt64ResizableDirectMemoryLinks.DefaultLinksSizeStep))
            using (var memoryAdapter = new UInt64ResizableDirectMemoryLinks(memory, UInt64ResizableDirectMemoryLinks.DefaultLinksSizeStep))
            {
                memoryAdapter.TestNonexistentReferences();
            }
        }

        private static void TestNonexistentReferences(this ILinks<ulong> memoryAdapter)
        {
            var link = memoryAdapter.Create();

            memoryAdapter.Update(link, ulong.MaxValue, ulong.MaxValue);

            var resultLink = Constants.Null;

            memoryAdapter.Each(foundLink =>
            {
                resultLink = foundLink[Constants.IndexPart];
                return Constants.Break;
            }, Constants.Any, ulong.MaxValue, ulong.MaxValue);

            Assert.True(resultLink == link);

            Assert.True(memoryAdapter.Count(ulong.MaxValue) == 0);

            memoryAdapter.Delete(link);
        }
    }
}
