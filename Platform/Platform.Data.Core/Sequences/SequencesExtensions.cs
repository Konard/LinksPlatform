using System.Collections.Generic;

namespace Platform.Data.Core.Sequences
{
    public static class SequencesExtensions
    {
        public static ulong Create(this Sequences sequences, IList<ulong[]> groupedSequence)
        {
            var finalSequence = new ulong[groupedSequence.Count];

            for (var i = 0; i < finalSequence.Length; i++)
            {
                var part = groupedSequence[i];
                finalSequence[i] = part.Length == 1 ? part[0] : sequences.Create(part);
            }

            return sequences.Create(finalSequence);
        }
    }
}
