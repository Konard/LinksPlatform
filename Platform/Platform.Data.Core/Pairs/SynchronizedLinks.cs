using System;
using System.Collections.Generic;
using Platform.Helpers.Threading;

namespace Platform.Data.Core.Pairs
{
    /// <remarks>
    /// TODO: Autogeneration of synchronized wrapper (decorator).
    /// TODO: Try to unfold code of each method using IL generation for performance improvements.
    /// TODO: Or even to unfold multiple layers of implementations.
    /// </remarks>
    public class SynchronizedLinks<T> : ISynchronizedLinks<T>
    {
        public ILinksCombinedConstants<bool, T, int> Constants { get; }
        public ISynchronization SyncRoot { get; }
        public ILinks<T> Sync { get; }
        public ILinks<T> Unsync { get; }

        public SynchronizedLinks(ILinks<T> links)
            : this(new SafeSynchronization(), links)
        {
        }

        public SynchronizedLinks(ISynchronization synchronization, ILinks<T> links)
        {
            SyncRoot = synchronization;
            Sync = this;
            Unsync = links;
            Constants = links.Constants;
        }

        public T Count(params T[] restrictions) => SyncRoot.ExecuteReadOperation(restrictions, Unsync.Count);
        public bool Each(Func<IList<T>, bool> handler, IList<T> restrictions) => SyncRoot.ExecuteReadOperation(handler, restrictions, Unsync.Each);
        public T Create() => SyncRoot.ExecuteWriteOperation(Unsync.Create);
        public T Update(IList<T> restrictions) => SyncRoot.ExecuteWriteOperation(restrictions, Unsync.Update);
        public void Delete(T link) => SyncRoot.ExecuteWriteOperation(link, Unsync.Delete);
    }
}
