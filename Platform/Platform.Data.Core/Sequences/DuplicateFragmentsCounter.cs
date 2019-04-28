using System;
using System.Collections.Generic;
using Platform.Helpers;
using Platform.Helpers.Collections;
using Platform.Data.Core.Collections;
using Platform.Data.Core.Doublets;
using Platform.Data.Core.Common;

namespace Platform.Data.Core.Sequences
{
    public class DuplicateFragmentsCounter<TLink> : ICounter<TLink>
    {
        private readonly ILinks<TLink> _links;
        private readonly ISequences<TLink> _sequences;

        public DuplicateFragmentsCounter(ILinks<TLink> links, ISequences<TLink> sequences)
        {
            _links = links;
            _sequences = sequences;
        }

        public TLink Count()
        {
            var total = default(TLink);

            var count = _links.Count();

            var visited = new BitString((long)(Integer<TLink>)count + 1);

            _links.Each(link =>
            {
                var linkIndex = _links.GetIndex(link);

                if (!visited.Get((long)(Integer<TLink>)linkIndex))
                {
                    var list = new List<TLink>();
                    _sequences.EachPart(list.AddAndReturnTrue, linkIndex);

                    if (list.Count > 2)
                    {
                        var duplicates = default(TLink);

#if DEBUG
                        var duplicatesList = new List<TLink>();
#endif

                        _sequences.Each(sequence =>
                        {
                            duplicates = MathHelpers.Increment(duplicates);

                            visited.Set((long)(Integer<TLink>)sequence);

#if DEBUG
                            duplicatesList.Add(sequence);
#endif

                            return true; // Continue
                        }, list.ToArray());

                        if (MathHelpers.GreaterThan(duplicates, Integer<TLink>.One))
                        {
                            total = MathHelpers.Add(total, duplicates);
#if DEBUG
                            PrintDuplicates(duplicatesList);
#endif
                        }
                    }
                }

                return _links.Constants.Continue;
            });

            return total;
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
