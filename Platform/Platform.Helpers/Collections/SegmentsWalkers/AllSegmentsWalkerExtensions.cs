namespace Platform.Helpers.Collections.SegmentsWalkers
{
    public static class AllSegmentsWalkerExtensions
    {
        public static void WalkAll(this AllSegmentsWalkerBase<char> walker, string @string) => walker.WalkAll(@string.ToCharArray());

        public static void WalkAll<TSegment>(this AllSegmentsWalkerBase<char, TSegment> walker, string @string) 
            where TSegment : Segment<char>
            => walker.WalkAll(@string.ToCharArray());
    }
}
