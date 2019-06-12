namespace Platform.Helpers.Collections.SegmentsWalkers
{
    public abstract class DuplicateStringSegmentsWalkerBase : AllStringSegmentsWalkerBase
    {
        protected override void Iteration(ref CharsSegment segment)
        {
            var frequency = GetSegmentFrequency(ref segment);
            if (frequency == 1)
                OnDublicateFound(ref segment);
            SetSegmentFrequency(ref segment, frequency + 1);
        }

        protected abstract void OnDublicateFound(ref CharsSegment segment);
        protected abstract long GetSegmentFrequency(ref CharsSegment segment);
        protected abstract void SetSegmentFrequency(ref CharsSegment segment, long frequency);
    }
}
