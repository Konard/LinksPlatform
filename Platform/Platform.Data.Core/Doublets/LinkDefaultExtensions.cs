using Platform.Data.Core.Common;

namespace Platform.Data.Core.Doublets
{
    public static class LinkDefaultExtensions
    {
        public static bool IsFullPoint(this UInt64Link link) => Point<ulong>.IsFullPoint(link);

        public static bool IsPartialPoint(this UInt64Link link) => Point<ulong>.IsPartialPoint(link);
    }
}
