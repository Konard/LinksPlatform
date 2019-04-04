using System.IO;
using Platform.Helpers.Disposables;
using Platform.Data.Core.Doublets;
using Platform.Data.Core.Sequences;

namespace Platform.Tests.Data.Core
{
    public class TempLinksTestScope : DisposableBase
    {
        public readonly ILinksMemoryManager<ulong> MemoryManager;
        public readonly SynchronizedLinks<ulong> Links;
        public readonly Sequences Sequences;
        public readonly string TempFilename;
        public readonly string TempTransactionLogFilename;
        private readonly bool _deleteFiles;

        public TempLinksTestScope(bool deleteFiles = true, bool useSequences = false, SequencesOptions sequencesOptions = new SequencesOptions(), bool useLog = false)
        {
            _deleteFiles = deleteFiles;
            TempFilename = Path.GetTempFileName();
            TempTransactionLogFilename = Path.GetTempFileName();

            MemoryManager = new UInt64LinksMemoryManager(TempFilename);
            //MemoryManager = new LinksMemoryManager<ulong>(TempFilename);
            ILinks<ulong> coreLinks = new UInt64Links(MemoryManager);

            Links = new SynchronizedLinks<ulong>(useLog ? new UInt64LinksTransactionsLayer(coreLinks, TempTransactionLogFilename) : coreLinks);
            if (useSequences)
                Sequences = new Sequences(Links, sequencesOptions);
        }

        protected override void DisposeCore(bool manual)
        {
            if (manual)
                Disposable.TryDispose(Links.Unsync);

            if (_deleteFiles)
                DeleteFiles();
        }

        public void DeleteFiles()
        {
            File.Delete(TempFilename);
            File.Delete(TempTransactionLogFilename);
        }
    }
}
