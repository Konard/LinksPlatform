using System;
using System.Collections.Generic;
using Platform.Helpers;
using Platform.Helpers.Collections;
using Platform.Helpers.Collections.SegmentsWalkers;
using Platform.Data.Core.Collections;
using Platform.Data.Core.Doublets;
using Platform.Data.Core.Common;

namespace Platform.Data.Core.Sequences
{
    public class DuplicateFragmentsProvider<TLink> : DictionaryBasedDuplicateSegmentsWalkerBase<TLink>, IProvider<IList<IList<TLink>>>
    {
        private readonly ILinks<TLink> _links;
        private readonly ISequences<TLink> _sequences;
        private List<IList<TLink>> _groups;
        private HashSet<Segment<TLink>> _checkedSequences;
        private BitString _visited;

        public DuplicateFragmentsProvider(ILinks<TLink> links, ISequences<TLink> sequences)
        {
            _links = links;
            _sequences = sequences;
        }

        public IList<IList<TLink>> Get()
        {
            _groups = new List<IList<TLink>>();
            _checkedSequences = new HashSet<Segment<TLink>>();

            var count = _links.Count();

            _visited = new BitString((long)(Integer<TLink>)count + 1);

            _links.Each(link =>
            {
                var linkIndex = _links.GetIndex(link);
                var linkBitIndex = (long)(Integer<TLink>)linkIndex;

                if (!_visited.Get(linkBitIndex))
                {
                    var sequenceElements = new List<TLink>();
                    _sequences.EachPart(sequenceElements.AddAndReturnTrue, linkIndex);

                    var sequenceElementsArray = sequenceElements.ToArray();

                    if (sequenceElements.Count > 2)
                    {
                        WalkAll(sequenceElements);
                    }
                }

                return _links.Constants.Continue;
            });

            return _groups;
        }

        protected override Segment<TLink> CreateSegment(IList<TLink> elements, int offset, int length) => new Segment<TLink>(elements, offset, length);

        protected override void OnDublicateFound(Segment<TLink> segment)
        {
            if (_checkedSequences.Add(segment))
                CollectDuplicatesForSequence(segment);
        }

        private void CollectDuplicatesForSequence(Segment<TLink> segment)
        {
            List<TLink> duplicates = CollectDuplicatesForSegment(segment);

            if (duplicates.Count > 1)
            {
                _groups.Add(duplicates);
#if DEBUG
                PrintDuplicates(duplicates);
#endif
            }
        }

        private List<TLink> CollectDuplicatesForSegment(Segment<TLink> segment)
        {
            var duplicates = new List<TLink>();
            var readAsElement = new HashSet<TLink>();

            _sequences.Each(sequence =>
            {
                var sequenceBitIndex = (long)(Integer<TLink>)sequence;
                if (!_visited.Get(sequenceBitIndex))
                {
                    _visited.Set(sequenceBitIndex);
                    duplicates.Add(sequence);
                    readAsElement.Add(sequence);
                }

                return true; // Continue
            }, segment);

            var sequencesExperiments = _sequences as Sequences;
            if (sequencesExperiments != null)
            {
                var partiallyMatched = sequencesExperiments.GetAllPartiallyMatchingSequences4((HashSet<ulong>)(object)readAsElement, (IList<ulong>)(object)segment);
                foreach (var partiallyMatchedSequence in partiallyMatched)
                {
                    var sequenceBitIndex = (long)(Integer<TLink>)partiallyMatchedSequence;
                    if (!_visited.Get(sequenceBitIndex))
                    {
                        TLink sequenceIndex = (Integer<TLink>)partiallyMatchedSequence;
                        _visited.Set(sequenceBitIndex);
                        duplicates.Add(sequenceIndex);
                    }
                }
            }

            return duplicates;
        }

        private void PrintDuplicates(List<TLink> duplicatesList)
        {
            if (!(_links is ILinks<ulong> ulongLinks))
                return;

            for (int i = 0; i < duplicatesList.Count; i++)
            {
                ulong sequenceIndex = (ulong)(Integer<TLink>)duplicatesList[i];

                var formatedSequenceStructure = ulongLinks.FormatStructure(sequenceIndex, x => Point<ulong>.IsPartialPoint(x), (sb, link) => _ = UnicodeMap.IsCharLink(link.Index) ? sb.Append(UnicodeMap.FromLinkToChar(link.Index)) : sb.Append(link.Index));
                Console.WriteLine(formatedSequenceStructure);

                var sequenceString = UnicodeMap.FromSequenceLinkToString(sequenceIndex, ulongLinks);
                Console.WriteLine(sequenceString);
            }
            Console.WriteLine();
        }
    }
}