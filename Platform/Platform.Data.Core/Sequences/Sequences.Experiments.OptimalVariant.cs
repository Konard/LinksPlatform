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
            for (var i = sequence.Length - 1; i >= 1; i--)
                IncrementPairFrequency(sequence[i - 1], sequence[i]);
        }
        
        public void IncrementPairFrequency(ulong source, ulong target)
        {
            var pair = Links.GetOrCreate(source, target);
            
            var previousFrequency = GetPairFrequency(pair);
            var frequency = IncrementFrequency(previousFrequency);
            SetPairFrequency(pair, previousFrequency, frequency);
        }
        
        public ulong GetPairFrequency(ulong pair)
        {
            var frequency = 0UL;
            
            Links.Unsync.Each(pair, Constants.Any, candidate =>
            {
                var candidateTarget = Links.GetTarget(candidate);
                
                if (candidateTarget == _frequencyMarker)
                {
                    frequency = candidate;
                    return Constants.Break;
                }
                
                return Constants.Continue;
            });
            
            return frequency;
        }
        
        public ulong IncrementFrequency(ulong frequency)
        {
            if (frequency == 0)
                return Links.GetOrCreate(_unaryOne, _frequencyMarker);
            var source = Links.GetSource(frequency);
            var incrementedSource = IncrementUnaryNumber(source);
            return Links.GetOrCreate(incrementedSource, _frequencyMarker);
        }
        
        public void SetPairFrequency(ulong pair, ulong previousFrequency, ulong frequency)
        {
            if (previousFrequency != 0)
                Links.Delete(previousFrequency);
            Links.GetOrCreate(pair, frequency);
        }
        
        private ulong _frequencyMarker;
        
        public ulong GetFrequencyMarker()
        {
            return _frequencyMarker;
        }
        
        public void SetFrequencyMarker(ulong frequencyMarker)
        {
            _frequencyMarker = frequencyMarker;
        }
        
        private ulong _unaryOne;
        
        public ulong GetUnaryOne()
        {
            return _unaryOne;
        }
        
        public void SetUnaryOne(ulong unaryOne)
        {
            _unaryOne = unaryOne;
        }
        
        public ulong IncrementUnaryNumber(ulong unaryNumber)
        {
            if (unaryNumber == _unaryOne)
                return Links.GetOrCreate(_unaryOne, _unaryOne);
            
            var source = Links.GetSource(unaryNumber);
            var target = Links.GetTarget(unaryNumber);
            
            if (source == target)
                return Links.GetOrCreate(unaryNumber, _unaryOne);
            else
                return Links.GetOrCreate(source, IncrementUnaryNumber(target));
        }
    }
}
