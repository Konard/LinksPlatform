using System.IO;
using Platform.Helpers.Disposal;
using Platform.Data.Core.Pairs;
using Platform.Data.Core.Sequences;

namespace Platform.Tests.Data.Core
{
    public class TempLinksTestScope : DisposalBase
    {
        public readonly LinksMemoryManager MemoryManager;
        public readonly Links Links;
        public readonly Sequences Sequences;
        public readonly string TempFilename;
        public readonly string TempTransactionLogFilename;
        private readonly bool _deleteFiles;

        public TempLinksTestScope(bool deleteFiles = true, bool useSequences = false, SequencesOptions sequencesOptions = new SequencesOptions(), bool useLog = false)
        {
            _deleteFiles = deleteFiles;
            TempFilename = Path.GetTempFileName();
            TempTransactionLogFilename = Path.GetTempFileName();

            MemoryManager = new LinksMemoryManager(TempFilename);
            Links = useLog ? new Links(MemoryManager, TempTransactionLogFilename) : new Links(MemoryManager);
            if (useSequences)
                Sequences = new Sequences(Links, sequencesOptions);
        }

        protected override void DisposeCore(bool manual)
        {
            Links.Dispose();
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
