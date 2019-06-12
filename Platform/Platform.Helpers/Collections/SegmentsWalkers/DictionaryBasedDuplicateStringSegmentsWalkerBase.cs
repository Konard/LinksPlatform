using System;
using System.Collections.Generic;

namespace Platform.Helpers.Collections.SegmentsWalkers
{
    public abstract class DictionaryBasedDuplicateStringSegmentsWalkerBase : DuplicateStringSegmentsWalkerBase
    {
        public const bool DefaultResetDictionaryOnEachWalk = false;

        private readonly bool _resetDictionaryOnEachWalk;

        protected IDictionary<StringSegment, long> Dictionary;

        public DictionaryBasedDuplicateStringSegmentsWalkerBase(IDictionary<StringSegment, long> dictionary, bool resetDictionaryOnEachWalk = DefaultResetDictionaryOnEachWalk)
        {
            Dictionary = dictionary;
            _resetDictionaryOnEachWalk = resetDictionaryOnEachWalk;
        }

        public DictionaryBasedDuplicateStringSegmentsWalkerBase(bool resetDictionaryOnEachWalk = DefaultResetDictionaryOnEachWalk)
            : this(resetDictionaryOnEachWalk ? null : new Dictionary<StringSegment, long>(), resetDictionaryOnEachWalk)
        {
        }

        public override void WalkAll(string @string)
        {
            if (_resetDictionaryOnEachWalk)
            {
                var capacity = Math.Ceiling(Math.Pow(@string.Length, 2) / 2);
                Dictionary = new Dictionary<StringSegment, long>((int)capacity);
            }

            base.WalkAll(@string);
        }

        protected override long GetSegmentFrequency(ref StringSegment segment) => Dictionary.GetOrDefault(segment);

        protected override void SetSegmentFrequency(ref StringSegment segment, long frequency) => Dictionary[segment] = frequency;
    }
}
