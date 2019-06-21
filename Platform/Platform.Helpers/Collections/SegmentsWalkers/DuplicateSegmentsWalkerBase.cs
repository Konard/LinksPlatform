using System;

namespace Platform.Helpers.Collections.SegmentsWalkers
{
    public abstract class DuplicateSegmentsWalkerBase<T, TSegment> : AllSegmentsWalkerBase<T, TSegment>
        where TSegment : Segment<T>
    {
        protected DuplicateSegmentsWalkerBase(int minimumStringSegmentLength = DefaultMinimumStringSegmentLength)
            : base(minimumStringSegmentLength)
        {
        }

        protected override void Iteration(TSegment segment)
        {
            var frequency = GetSegmentFrequency(segment);
            if (frequency == 1)
                OnDublicateFound(segment);
            SetSegmentFrequency(segment, frequency + 1);
        }

        protected abstract void OnDublicateFound(TSegment segment);
        protected abstract long GetSegmentFrequency(TSegment segment);
        protected abstract void SetSegmentFrequency(TSegment segment, long frequency);
    }

    public abstract class DuplicateSegmentsWalkerBase<T> : DuplicateSegmentsWalkerBase<T, Segment<T>>
    {
    }
}
