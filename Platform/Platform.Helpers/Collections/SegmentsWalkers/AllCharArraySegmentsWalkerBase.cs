namespace Platform.Helpers.Collections.SegmentsWalkers
{
    public abstract class AllCharArraySegmentsWalkerBase
    {
        public const int DefaultMinimumStringSegmentLength = 2;

        private readonly int _minimumStringSegmentLength;

        public AllCharArraySegmentsWalkerBase(int minimumStringSegmentLength = DefaultMinimumStringSegmentLength) => _minimumStringSegmentLength = minimumStringSegmentLength;

        public virtual void WalkAll(char[] chars)
        {
            var maxOffset = chars.Length - _minimumStringSegmentLength;
            for (int offset = 0; offset <= maxOffset; offset++)
            {
                var maxLength = chars.Length - offset;
                for (int length = _minimumStringSegmentLength; length <= maxLength; length++)
                {
                    var segment = new StringSegment(chars, offset, length);
                    Iteration(ref segment);
                }
            }
        }

        protected abstract void Iteration(ref StringSegment segment);
    }
}
