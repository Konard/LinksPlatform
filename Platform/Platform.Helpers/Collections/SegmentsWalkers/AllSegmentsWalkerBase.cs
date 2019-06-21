using System.Collections.Generic;

namespace Platform.Helpers.Collections.SegmentsWalkers
{
    public abstract class AllSegmentsWalkerBase<T, TSegment>
        where TSegment : Segment<T>
    {
        public const int DefaultMinimumStringSegmentLength = 2;

        private readonly int _minimumStringSegmentLength;

        protected AllSegmentsWalkerBase(int minimumStringSegmentLength = DefaultMinimumStringSegmentLength) => _minimumStringSegmentLength = minimumStringSegmentLength;

        public virtual void WalkAll(IList<T> elements)
        {
            var maxOffset = elements.Count - _minimumStringSegmentLength;
            for (int offset = 0; offset <= maxOffset; offset++)
            {
                var maxLength = elements.Count - offset;
                for (int length = _minimumStringSegmentLength; length <= maxLength; length++)
                    Iteration(CreateSegment(elements, offset, length));
            }
        }

        protected abstract TSegment CreateSegment(IList<T> elements, int offset, int length);

        protected abstract void Iteration(TSegment segment);
    }

    public abstract class AllSegmentsWalkerBase<T> : AllSegmentsWalkerBase<T, Segment<T>>
    {
        protected override Segment<T> CreateSegment(IList<T> elements, int offset, int length) => new Segment<T>(elements, offset, length);
    }
}
