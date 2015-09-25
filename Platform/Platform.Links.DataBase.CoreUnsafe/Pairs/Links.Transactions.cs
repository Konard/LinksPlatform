//#define LinksTransactions

namespace Platform.Links.DataBase.CoreUnsafe.Pairs
{
#if LinksTransactions

using System;

namespace Links.Core
{
    partial class Links
    {
        // В разработке
        // Суммарный размер структурки: 40 байт
        private struct TransactionElement
        {
            public DateTime Timestamp; // 8 байт
            public ulong TransactionId; // 8 байт
            public Link Before; // 8 * 3 = 24 байт
            public Link After; // 8 * 3 = 24 байт
        }

        // TODO: Будут ли работать эти функции как ожидается?

        public void EnterTransaction()
        {
            _rwLock.EnterWriteLock();
        }

        public void ExitTransaction()
        {
            _rwLock.ExitWriteLock();
        }
    }
}

#endif
}