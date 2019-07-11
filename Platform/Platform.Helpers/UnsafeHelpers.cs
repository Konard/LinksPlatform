using System.Runtime.InteropServices;

namespace Platform.Helpers
{
    public static class UnsafeHelpers
    {
        /// <summary>
        /// Returns the size of an unmanaged type in bytes.
        /// This method do this without throwing exceptions for generic types as Marshal.SizeOf[T]() and Marshal.SizeOf(Type type) do.
        /// </summary>
        /// <remarks>
        /// Based on proposed solution at https://stackoverflow.com/a/18167584/710069
        /// For actual differences in .NET code see:
        /// https://github.com/Microsoft/referencesource/blob/f82e13c3820cd04553c21bf6da01262b95d9bd43/mscorlib/system/runtime/interopservices/marshal.cs#L202
        /// https://github.com/Microsoft/referencesource/blob/f82e13c3820cd04553c21bf6da01262b95d9bd43/mscorlib/system/runtime/interopservices/marshal.cs#L219
        /// Note that this behaviour can be changed in future versions of .NET
        /// </remarks>
        public static int SizeOf<TStruct>() where TStruct : struct => Marshal.SizeOf(default(TStruct));
    }
}
