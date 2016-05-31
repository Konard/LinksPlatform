using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Platform.Helpers
{
    public unsafe class MemoryHelpers
    {
        /// <remarks>
        /// TODO: Test is it a correct algorithm.
        /// TODO: Check if there is high-performance multi-thread version of algorithm. Possible ideas: use SIMD, ask .NET Core team to include option/flag to AllocHGlobal and ReAllocHGlobal for zeroing of memory.
        /// </remarks>
        public static void ZeroMemory(void* pointer, long capacity)
        {
            var ulongs = capacity / sizeof(ulong);

            var partitioner = Partitioner.Create(0, ulongs);

            Parallel.ForEach(partitioner, range =>
            {
                for (long i = range.Item1; i < range.Item2; i++)
                    *((ulong*)pointer + i) = 0;
            });

            for (var i = ulongs * sizeof(ulong); i < capacity; i++)
                *((byte*)pointer + i) = 0;
        }
    }
}
