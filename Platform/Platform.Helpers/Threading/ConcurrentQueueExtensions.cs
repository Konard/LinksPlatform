using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Platform.Collections.Concurrent;

namespace Platform.Helpers.Threading
{
    public static class ConcurrentQueueExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task AwaitAll(this ConcurrentQueue<Task> queue)
        {
            foreach (var item in queue.DequeueAll())
                await item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task AwaitOne(this ConcurrentQueue<Task> queue)
        {
            if (queue.TryDequeue(out Task item))
                await item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnqueueAsRunnedTask(this ConcurrentQueue<Task> queue, Action action) => queue.Enqueue(Task.Run(action));
    }
}
