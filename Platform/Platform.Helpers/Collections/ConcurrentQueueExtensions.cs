using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Platform.Helpers.Collections
{
    public static class ConcurrentQueueExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> DequeueAll<T>(this ConcurrentQueue<T> queue)
        {
            T item;
            while (queue.TryDequeue(out item))
                yield return item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task AwaitAll(this ConcurrentQueue<Task> queue)
        {
            foreach (var item in queue.DequeueAll())
                await item;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task AwaitOne(this ConcurrentQueue<Task> queue)
        {
            Task item;
            if(queue.TryDequeue(out item))
                await item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnqueueRunnedTask(this ConcurrentQueue<Task> queue, Action action) => queue.Enqueue(Task.Run(action));
    }
}
