using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Platform.Helpers.Disposal;

namespace Platform.Data.Core.Pairs
{
    public class LinksInstancesCache : DisposalBase
    {
        private static readonly long DefaultLinksSizeStep = LinksMemoryManager.LinkSizeInBytes * 1024 * 1024 * 4;

        public static readonly LinksInstancesCache CacheInstance = new LinksInstancesCache();
        public static readonly ConcurrentDictionary<int, Links> Instances = new ConcurrentDictionary<int, Links>();
        private static int LastInstanceId = 0;

        public static int Create(string path, string logPath)
        {
            var memoryManager = new LinksMemoryManager(path, DefaultLinksSizeStep);
            var links = new Links(memoryManager, logPath);

            var id = Interlocked.Increment(ref LastInstanceId);

            if (!Instances.TryAdd(id, links))
                throw new InvalidOperationException("Cannot add links instance to cache.");

            return id;
        }

        public static Links Get(int id)
        {
            Links links;
            if (!Instances.TryGetValue(id, out links))
                throw new InvalidOperationException("Cannot get links instance from cache.");
            return links;
        }

        public static void Dispose(int id)
        {
            Links links;
            if (!Instances.TryRemove(id, out links))
                throw new InvalidOperationException("Cannot destroy links instance in cache.");
            links.Dispose();
        }

        protected override void DisposeCore(bool manual)
        {
            foreach (var instance in Instances.ToArray().Select(x => x.Value))
                instance.Dispose();
        }
    }
}
