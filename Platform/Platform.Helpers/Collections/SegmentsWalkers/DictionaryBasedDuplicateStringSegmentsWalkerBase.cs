using System;
using System.Collections.Generic;

namespace Platform.Helpers.Collections.SegmentsWalkers
{
    public abstract class DictionaryBasedDuplicateStringSegmentsWalkerBase : DuplicateStringSegmentsWalkerBase
    {
        public const bool DefaultResetDictionaryOnEachWalk = false;

        private readonly bool _resetDictionaryOnEachWalk;

        protected IDictionary<CharsSegment, long> Dictionary;

        public DictionaryBasedDuplicateStringSegmentsWalkerBase(IDictionary<CharsSegment, long> dictionary, bool resetDictionaryOnEachWalk = DefaultResetDictionaryOnEachWalk)
        {
            Dictionary = dictionary;
            _resetDictionaryOnEachWalk = resetDictionaryOnEachWalk;
        }

        public DictionaryBasedDuplicateStringSegmentsWalkerBase(bool resetDictionaryOnEachWalk = DefaultResetDictionaryOnEachWalk)
            : this(resetDictionaryOnEachWalk ? null : new Dictionary<CharsSegment, long>(), resetDictionaryOnEachWalk)
        {
        }

        public override void WalkAll(string @string)
        {
            if (_resetDictionaryOnEachWalk)
            {
                var capacity = Math.Ceiling(Math.Pow(@string.Length, 2) / 2);
                Dictionary = new Dictionary<CharsSegment, long>((int)capacity);
            }

            base.WalkAll(@string);
        }

        protected override long GetSegmentFrequency(ref CharsSegment segment) => Dictionary.GetOrDefault(segment);

        protected override void SetSegmentFrequency(ref CharsSegment segment, long frequency) => Dictionary[segment] = frequency;
    }
}
