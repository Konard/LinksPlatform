using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Platform.Helpers.Threading
{
    public static class TaskExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T AwaitResult<T>(this Task<T> task) => task.GetAwaiter().GetResult();
    }
}
