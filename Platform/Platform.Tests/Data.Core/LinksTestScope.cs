using System.IO;
using Platform.Helpers.Disposal;
using Platform.Data.Core.Pairs;

namespace Platform.Tests.Data.Core
{
    public class LinksTestScope : DisposalBase
    {
        public readonly Links Links;
        private LinksMemoryManager _memoryManager;
        private string _tempFilename;

        public LinksTestScope()
        {
            _tempFilename = Path.GetTempFileName();

            _memoryManager = new LinksMemoryManager(_tempFilename);
            Links = new Links(_memoryManager);
        }

        protected override void DisposeCore(bool manual)
        {
            Links.Dispose();
            File.Delete(_tempFilename);
        }
    }
}
