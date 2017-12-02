using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.Data.Core.Collections;
using Platform.Data.Core.Exceptions;
using Platform.Data.Core.Pairs;
using Platform.Helpers;
using Platform.Helpers.Collections;
using LinkIndex = System.UInt64;

namespace Platform.Data.Core.Sequences
{
    partial class Sequences
    {
        public bool IncrementPairsFrequencies(ulong[] sequence)
        {
            var indexed = true;

            var i = sequence.Length;
            //while (--i >= 1 && (indexed = Links.SearchOrDefault(sequence[i - 1], sequence[i]) != Constants.Null)) { }

            for (; i >= 1; i--)
                Links.GetOrCreate(sequence[i - 1], sequence[i]);

            return indexed;
        }
        
        public void IncrementPairFrequency(ulong source, ulong target)
        {
            var pair = Links.GetOrCreate(source, target);
            
            var frequency = GetPairFrequency(pair);
            frequency = IncrementPairFrequency(frequency);
            SetPairFrequency(pair, frequency);
        }
    }
}
