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

            var count = _links.Count();

            var visited = new BitString((long)(Integer<TLink>)count + 1);

            _links.Each(link =>
            {
                var linkIndex = _links.GetIndex(link);

                if (!visited.Get((long)(Integer<TLink>)linkIndex))
                {
                    var sequenceElements = new List<TLink>();
                    _sequences.EachPart(sequenceElements.AddAndReturnTrue, linkIndex);

                    if (sequenceElements.Count > 2)
                    {
                        var duplicates = new List<TLink>();

                        _sequences.Each(sequence =>
                        {
                            visited.Set((long)(Integer<TLink>)sequence);
                            duplicates.Add(sequence);

                            return true; // Continue
                        }, sequenceElements.ToArray());

                        if (duplicates.Count > 1)
                        {
                            groups.Add(duplicates);
#if DEBUG
                            PrintDuplicates(duplicatesList);
#endif
                        }
                    }
                }

                return _links.Constants.Continue;
            });

            return groups;
        }

        private void PrintDuplicates(List<TLink> duplicatesList)
        {
            if (!(_links is ILinks<ulong> ulongLinks))
                return;

            for (int i = 0; i < duplicatesList.Count; i++)
            {
                ulong sequenceIndex = (ulong)(Integer<TLink>)duplicatesList[i];

                var formatedSequenceStructure = ulongLinks.FormatStructure(sequenceIndex, x => Point<ulong>.IsPartialPoint(x));
                Console.WriteLine(formatedSequenceStructure);

                var sequenceString = UnicodeMap.FromSequenceLinkToString(sequenceIndex, ulongLinks);
                Console.WriteLine(sequenceString);
            }
            Console.WriteLine();
        }
    }
}