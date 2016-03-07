namespace Platform.Data.Core.Pairs
{
    public static class LinkDefaultExtensions
    {
        public static bool IsFullPoint(this Link link)
        {
            return Point<ulong>.IsFullPoint(link);
        }

        public static bool IsPartialPoint(this Link link)
        {
            return Point<ulong>.IsPartialPoint(link);
        }
    }
}
