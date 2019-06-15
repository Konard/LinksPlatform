using System;
using System.Collections.Generic;

namespace Platform.Helpers.Collections.SegmentsWalkers
{
    public abstract class DictionaryBasedDuplicateSegmentsWalkerBase<T, TSegment> : DuplicateSegmentsWalkerBase<T, TSegment>
        where TSegment : Segment<T>
    {
        public const bool DefaultResetDictionaryOnEachWalk = false;

        private readonly bool _resetDictionaryOnEachWalk;

        protected IDictionary<TSegment, long> Dictionary;

        public DictionaryBasedDuplicateSegmentsWalkerBase(IDictionary<TSegment, long> dictionary, bool resetDictionaryOnEachWalk = DefaultResetDictionaryOnEachWalk)
        {
            Dictionary = dictionary;
            _resetDictionaryOnEachWalk = resetDictionaryOnEachWalk;
        }

        public DictionaryBasedDuplicateSegmentsWalkerBase(bool resetDictionaryOnEachWalk = DefaultResetDictionaryOnEachWalk)
            : this(resetDictionaryOnEachWalk ? null : new Dictionary<TSegment, long>(), resetDictionaryOnEachWalk)
        {
        }

        public override void WalkAll(IList<T> elements)
        {
            if (_resetDictionaryOnEachWalk)
            {
                var capacity = Math.Ceiling(Math.Pow(elements.Count, 2) / 2);
                Dictionary = new Dictionary<TSegment, long>((int)capacity);
            }

            base.WalkAll(elements);
        }

        protected override long GetSegmentFrequency(TSegment segment) => Dictionary.GetOrDefault(segment);

        protected override void SetSegmentFrequency(TSegment segment, long frequency) => Dictionary[segment] = frequency;
    }

    public abstract class DictionaryBasedDuplicateSegmentsWalkerBase<T> : DictionaryBasedDuplicateSegmentsWalkerBase<T, Segment<T>>
    {
        public DictionaryBasedDuplicateSegmentsWalkerBase(IDictionary<Segment<T>, long> dictionary, bool resetDictionaryOnEachWalk = DefaultResetDictionaryOnEachWalk)
            : base(dictionary, resetDictionaryOnEachWalk)
        {
        }

        public DictionaryBasedDuplicateSegmentsWalkerBase(bool resetDictionaryOnEachWalk = DefaultResetDictionaryOnEachWalk)
            : base(resetDictionaryOnEachWalk)
        {
        }
    }
}