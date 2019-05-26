using System;
using System.Collections.Generic;

namespace Platform.Helpers.Collections.SegmentsWalkers
{
    public abstract class DictionaryBasedDuplicateStringSegmentsWalkerBase : DuplicateStringSegmentsWalkerBase
    {
        public const bool DefaultResetDictionaryOnEachWalk = false;

        private readonly bool _resetDictionaryOnEachWalk;

        protected Dictionary<StringSegment, int> Dictionary;

        public DictionaryBasedDuplicateStringSegmentsWalkerBase(Dictionary<StringSegment, int> dictionary, bool resetDictionaryOnEachWalk = DefaultResetDictionaryOnEachWalk)
        {
            Dictionary = dictionary;
            _resetDictionaryOnEachWalk = resetDictionaryOnEachWalk;
        }

        public DictionaryBasedDuplicateStringSegmentsWalkerBase(bool resetDictionaryOnEachWalk = DefaultResetDictionaryOnEachWalk)
            : this(resetDictionaryOnEachWalk ? null : new Dictionary<StringSegment, int>(), resetDictionaryOnEachWalk)
        {
        }

        public override void WalkAll(string @string)
        {
            if (_resetDictionaryOnEachWalk)
            {
                var capacity = Math.Ceiling(Math.Pow(@string.Length, 2) / 2);
                Dictionary = new Dictionary<StringSegment, int>((int)capacity);
            }

            base.WalkAll(@string);
        }

        protected override int GetSegmentFrequency(ref StringSegment segment) => Dictionary.GetOrDefault(segment);

        protected override void SetSegmentFrequency(ref StringSegment segment, int frequency) => Dictionary[segment] = frequency;
    }
}
