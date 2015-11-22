using System.Threading.Tasks;

namespace Platform.Helpers.Threading
{
    public static class TaskExtensions
    {
        public static T AwaitResult<T>(this Task<T> task)
        {
            return task.GetAwaiter().GetResult();
        }
    }
}
