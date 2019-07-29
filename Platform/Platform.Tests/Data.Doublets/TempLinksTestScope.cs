using System.IO;
using Platform.Disposables;
using Platform.Data.Doublets;
using Platform.Data.Doublets.ResizableDirectMemory;
using Platform.Data.Doublets.Sequences;

namespace Platform.Tests.Data.Doublets
{
    public class TempLinksTestScope : DisposableBase
    {
        public readonly ILinks<ulong> MemoryAdapter;
        public readonly SynchronizedLinks<ulong> Links;
        public readonly Sequences Sequences;
        public readonly string TempFilename;
        public readonly string TempTransactionLogFilename;
        private readonly bool _deleteFiles;

        public TempLinksTestScope(bool deleteFiles = true, bool useSequences = false, SequencesOptions<ulong> sequencesOptions = new SequencesOptions<ulong>(), bool useLog = false)
        {
            _deleteFiles = deleteFiles;
            TempFilename = Path.GetTempFileName();
            TempTransactionLogFilename = Path.GetTempFileName();

            var coreMemoryAdapter = new UInt64ResizableDirectMemoryLinks(TempFilename);

            MemoryAdapter = useLog ? (ILinks<ulong>)new UInt64LinksTransactionsLayer(coreMemoryAdapter, TempTransactionLogFilename) : coreMemoryAdapter;

            Links = new SynchronizedLinks<ulong>(new UInt64Links(MemoryAdapter));
            if (useSequences)
            {
                Sequences = new Sequences(Links, sequencesOptions);
            }
        }

        protected override void DisposeCore(bool manual, bool wasDisposed)
        {
            Disposable.TryDispose(Links.Unsync);

            if (!wasDisposed && _deleteFiles)
            {
                DeleteFiles();
            }
        }

        public void DeleteFiles()
        {
            File.Delete(TempFilename);
            File.Delete(TempTransactionLogFilename);
        }

        // TODO: THIS IS EXCEPTION WORKAROUND, REMOVE IT THEN https://github.com/linksplatform/Disposables/issues/13 FIXED
        protected override bool AllowMultipleDisposeCalls => true;
    }
}
