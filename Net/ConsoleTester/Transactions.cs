using System;
using System.IO.MemoryMappedFiles;
using NetLibrary;
using System.IO;
using System.Runtime.InteropServices;

namespace ConsoleTester
{
    public static class Transactions
    {
        private const string TransactionsFileName = "transactions.log";
        private const string TransactionsMapName = "Links.Net.Transactions";

        private enum TransactionItemType
        {
            Creation,
            UpdateOf,
            UpdateTo,
            Deletion
        }

        private struct TransactionItem
        {
            public long TransactionId;
            public DateTime DateTime;
            public TransactionItemType Type;
            public Link Source;
            public Link Linker;
            public Link Target;
        }

        private struct TransactionsState
        {
            public long LastTransactionId;
            public long LastTransactionOffset;
            public long LastTransactionItemsCount;
            public long LastHandledTransactionId;
            public long LastHandledTransactionOffset;
            public long LastHandledTransactionItemOffset;
            public long FileEndOffset;
            public long FileSizeInTransactionItems;
        }

        static readonly private long BasicTransactionsOffset = Marshal.SizeOf(typeof(TransactionsState));
        static readonly private long TransactionItemSize = Marshal.SizeOf(typeof(TransactionItem));

        private static long CurrentFileSizeInBytes;
        private static MemoryMappedFile Log;
        private static MemoryMappedViewAccessor LogAccessor;
        private static TransactionsState CurrentState;

        private static bool TransactionOpened;

        static Transactions()
        {
            OpenFile();


            TransactionItem item = new TransactionItem()
            {
                TransactionId = CurrentState.LastTransactionId,
                DateTime = DateTime.UtcNow,
                Type = TransactionItemType.Creation,
                Source = Net.Link.Source,
                Linker = Net.Link.Linker,
                Target = Net.Link.Target,
            };

            //LogAccessor. += Marshal.SizeOf(typeof(TransactionItem));

            EnsureFileSize();

            LogAccessor.Write<TransactionItem>(BasicTransactionsOffset, ref item);

            LogAccessor.Read<TransactionItem>(BasicTransactionsOffset, out item);


            CloseFile();
        }

        static void StartTransaction()
        {
            // Защита от накопления кучи транзакций, которые не совершили ни одной операции
            if (CurrentState.LastTransactionItemsCount > 0)
            {
                CurrentState.LastTransactionId++;
                CurrentState.LastTransactionItemsCount = 0;
                CurrentState.LastTransactionOffset = CurrentState.FileEndOffset;
            }
            TransactionOpened = true;
        }

        static void CloseTransaction()
        {
            TransactionOpened = false;
        }

        static void LoadState()
        {
            LogAccessor.Read<TransactionsState>(0, out CurrentState);
            if (CurrentState.FileEndOffset == 0)
            {
                CurrentState.FileEndOffset = BasicTransactionsOffset;
            }
        }

        static void StoreState()
        {
            LogAccessor.Write<TransactionsState>(0, ref CurrentState);
        }

        static void EnsureFileSize()
        {
            long sizeInItems = CurrentState.FileSizeInTransactionItems;
            if ((CurrentState.FileEndOffset - BasicTransactionsOffset) / TransactionItemSize == sizeInItems)
            {
                if (sizeInItems < 16)
                    sizeInItems = 16;
                else
                    sizeInItems = sizeInItems * 2;

                var newSizeInBates = BasicTransactionsOffset + sizeInItems * TransactionItemSize;

                ReloadFile(newSizeInBates);

                CurrentState.LastTransactionItemsCount = sizeInItems;
            }
        }

        static void ReloadFile(long sizeInBytes = 0)
        {
            CloseFile();
            OpenFile(sizeInBytes);
        }

        static void CloseFile()
        {
            StoreState();
            LogAccessor.Flush();
            LogAccessor.Dispose();
            Log.Dispose();
        }

        static void OpenFile(long sizeInBytes = 0)
        {
            if (sizeInBytes < BasicTransactionsOffset)
                sizeInBytes = BasicTransactionsOffset;

            long savedSizeInBytes = 0;
            if (File.Exists(TransactionsFileName))
            {
                FileInfo fileInfo = new FileInfo(TransactionsFileName);
                savedSizeInBytes = fileInfo.Length;
            }

            if (sizeInBytes < savedSizeInBytes)
                sizeInBytes = savedSizeInBytes;

            Log = MemoryMappedFile.CreateFromFile(TransactionsFileName, FileMode.OpenOrCreate, TransactionsMapName, sizeInBytes);
            LogAccessor = Log.CreateViewAccessor();
            LoadState();

            CurrentFileSizeInBytes = sizeInBytes;
        }

        public static void Run()
        {
        }
    }
}
