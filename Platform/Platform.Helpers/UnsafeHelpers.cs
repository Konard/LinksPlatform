using System.Reflection;
using System.Runtime.InteropServices;

namespace Platform.Helpers
{
    public static class UnsafeHelpers
    {
        public static int SizeOf<TStruct>()
        {
            return (int)typeof(Marshal)
                .GetTypeInfo()
                .GetMethod("SizeOfHelper", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, new object[] { typeof(TStruct), true });
        }
    }
}
