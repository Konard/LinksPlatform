using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Platform.Data.Core.Pairs;

namespace Platform.Tests.Data.Core
{
    [TestClass]
    public class LinksMemoryManagerTests
    {
        private static readonly long DefaultLinksSizeStep = LinksMemoryManager.LinkSizeInBytes * 1024 * 1024;

        [TestMethod]
        public void BasicMemoryTest()
        {
            var tempFilename = Path.GetTempFileName();

            using (var memoryManager = new LinksMemoryManager(tempFilename, DefaultLinksSizeStep))
            {
                var link = memoryManager.AllocateLink();
                memoryManager.FreeLink(link);
            }

            File.Delete(tempFilename);
        }
    }
}
