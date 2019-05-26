namespace Platform.Helpers.Collections.SegmentsWalkers
{
    public abstract class AllStringSegmentsWalkerBase : AllCharArraySegmentsWalkerBase
    {
        public AllStringSegmentsWalkerBase(int minimumStringSegmentLength = DefaultMinimumStringSegmentLength)
            : base(minimumStringSegmentLength)
        {
        }

        public virtual void WalkAll(string @string)
        {
            var chars = @string.ToCharArray();
            WalkAll(chars);
        }
    }
}
