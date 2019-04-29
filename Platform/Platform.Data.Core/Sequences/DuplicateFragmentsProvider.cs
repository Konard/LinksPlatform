using System;
using System.Collections.Generic;
using Platform.Helpers;
using Platform.Helpers.Collections;
using Platform.Data.Core.Collections;
using Platform.Data.Core.Doublets;
using Platform.Data.Core.Common;

namespace Platform.Data.Core.Sequences
{
    public class DuplicateFragmentsProvider<TLink> : IProvider<IList<IList<TLink>>>
    {
        private readonly ILinks<TLink> _links;
        private readonly ISequences<TLink> _sequences;

        public DuplicateFragmentsProvider(ILinks<TLink> links, ISequences<TLink> sequences)
        {
            _links = links;
            _sequences = sequences;
        }

        public IList<IList<TLink>> Get()
        {
            var groups = new List<IList<TLink>>();
            var checkedSequences = new HashSet<TLink[]>();

            var count = _links.Count();

            var visited = new BitString((long)(Integer<TLink>)count + 1);

            _links.Each(link =>
            {
                var linkIndex = _links.GetIndex(link);
                var linkBitIndex = (long)(Integer<TLink>)linkIndex;

                if (!visited.Get(linkBitIndex))
                {
                    var sequenceElements = new List<TLink>();
                    _sequences.EachPart(sequenceElements.AddAndReturnTrue, linkIndex);

                    var sequenceElementsArray = sequenceElements.ToArray();

                    if (sequenceElements.Count > 2)
                    {
                        var maxOffset = sequenceElements.Count - 3;
                        for (int offset = 0; offset <= maxOffset; offset++)
                        {
                            var maxLength = sequenceElementsArray.Length - offset;
                            for (int length = 3; length <= maxLength; length++)
                            {
                                TLink[] sequenceElementsCopy;
                                if (offset == 0 && length == sequenceElementsArray.Length)
                                    sequenceElementsCopy = sequenceElementsArray;
                                else
                                {
                                    sequenceElementsCopy = new TLink[length];
                                    Array.Copy(sequenceElementsArray, offset, sequenceElementsCopy, 0, length);
                                }

                                if (checkedSequences.Add(sequenceElementsCopy))
                                    CollectDuplicatesForSequence(groups, visited, sequenceElementsCopy);
                            }
                        }
                    }
                }

                return _links.Constants.Continue;
            });

            return groups;
        }

        private void CollectDuplicatesForSequence(List<IList<TLink>> groups, BitString visited, TLink[] sequenceElements)
        {
            List<TLink> duplicates = CollectDuplicatesForSequence(visited, sequenceElements);

            if (duplicates.Count > 1)
            {
                groups.Add(duplicates);
#if DEBUG
                PrintDuplicates(duplicates);
#endif
            }
        }

        private List<TLink> CollectDuplicatesForSequence(BitString visited, TLink[] sequenceElements)
        {
            var duplicates = new List<TLink>();
            var readAsElement = new HashSet<TLink>();

            _sequences.Each(sequence =>
            {
                var sequenceBitIndex = (long)(Integer<TLink>)sequence;
                if (!visited.Get(sequenceBitIndex))
                {
                    visited.Set(sequenceBitIndex);
                    duplicates.Add(sequence);
                    readAsElement.Add(sequence);
                }

                return true; // Continue
            }, sequenceElements);

            var sequencesExperiments = _sequences as Sequences;
            if (sequencesExperiments != null)
            {
                var partiallyMatched = sequencesExperiments.GetAllPartiallyMatchingSequences4((HashSet<ulong>)(object)readAsElement, (ulong[])(object)sequenceElements);
                foreach (var partiallyMatchedSequence in partiallyMatched)
                {
                    var sequenceBitIndex = (long)(Integer<TLink>)partiallyMatchedSequence;
                    if (!visited.Get(sequenceBitIndex))
                    {
                        TLink sequenceIndex = (Integer<TLink>)partiallyMatchedSequence;
                        visited.Set(sequenceBitIndex);
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