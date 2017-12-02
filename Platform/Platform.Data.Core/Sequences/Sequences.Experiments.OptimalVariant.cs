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
        public void IncrementPairsFrequencies(ulong[] sequence)
        {
            for (; i >= 1; i--)
                IncrementPairFrequency(sequence[i - 1], sequence[i]);
        }
        
        public void IncrementPairFrequency(ulong source, ulong target)
        {
            var pair = Links.GetOrCreate(source, target);
            
            var frequency = GetPairFrequency(pair);
            frequency = IncrementFrequency(frequency);
            SetPairFrequency(pair, frequency);
        }
        
        public ulong GetPairFrequency(ulong pair)
        {
            return 0;
        }
        
        public ulong IncrementFrequency(ulong frequency)
        {
            return 0;
        }
        
        public void SetPairFrequency(ulong pair, ulong frequency)
        {
            
        }
        
        public ulong GetFrequencyMarker()
        {
            return 0;
        }
        
        public void SetFrequencyMarker(ulong frequencyMarker)
        {
            
        }
    }
}
