using System.IO;
using Platform.Helpers.Disposables;
using Platform.Data.Core.Pairs;
using Platform.Data.Core.Sequences;

namespace Platform.Tests.Data.Core
{
    public class TempLinksTestScope : DisposableBase
    {
        public readonly ILinksMemoryManager<ulong> MemoryManager;
        public readonly UInt64Links Links;
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
            Links = useLog ? new UInt64Links(MemoryManager, TempTransactionLogFilename) : new UInt64Links(MemoryManager);
            if (useSequences)
                Sequences = new Sequences(new SynchronizedLinks<ulong>(Links), sequencesOptions);
        }

        protected override void DisposeCore(bool manual)
        {
            if (manual)
                Disposable.TryDispose(Links);

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
