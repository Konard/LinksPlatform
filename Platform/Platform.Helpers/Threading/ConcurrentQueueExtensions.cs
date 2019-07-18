using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Platform.Collections.Concurrent;

namespace Platform.Helpers.Threading
{
    public static class ConcurrentQueueExtensions
    {
        public static async Task AwaitAll(this ConcurrentQueue<Task> queue)
        {
            foreach (var item in queue.DequeueAll())
                await item;
        }

        public static async Task AwaitOne(this ConcurrentQueue<Task> queue)
        {
            if (queue.TryDequeue(out Task item))
                await item;
        }

        public static void EnqueueAsRunnedTask(this ConcurrentQueue<Task> queue, Action action) => queue.Enqueue(Task.Run(action));
    }
}
