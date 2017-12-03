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
            for (var i = 1; i < sequence.Length; i++)
                IncrementPairFrequency(sequence[i - 1], sequence[i]);
        }
        
        public void PrintPairsFrequencies(ulong[] sequence)
        {
            for (var i = 1; i < sequence.Length; i++)
                PrintPairFrequency(sequence[i - 1], sequence[i]);
        }
        
        public void IncrementPairFrequency(ulong source, ulong target)
        {
            var pair = Links.GetOrCreate(source, target);
            
            var previousFrequency = GetPairFrequency(pair);
            var frequency = IncrementFrequency(previousFrequency);
            
            Console.WriteLine("frequency = {0}", frequency);
            
            SetPairFrequency(pair, previousFrequency, frequency);
            
            Console.WriteLine("GetPairFrequency(pair) = {0}", GetPairFrequency(pair) );
            
            PrintPairFrequency(source, target);
        }
        
        public void PrintPairFrequency(ulong source, ulong target)
        {
            var pair = Links.GetOrCreate(source, target);
                           
            var previousFrequency = GetPairFrequency(pair);
            if (previousFrequency == 0)
                Console.WriteLine("({0},{1}) - 0", source, target);
            else
            {
                var frequency = Links.GetSource(previousFrequency);
                var number = ConvertUnaryNumberToUInt64(frequency);
            
                Console.WriteLine("({0},{1}) - {2}", source, target, number);
            }
        }
        
        public ulong GetPairFrequency(ulong pair)
        {
            var frequency = 0UL;
            
            Links.Each(pair, Constants.Any, candidate =>
            {
                var candidateTarget = Links.GetTarget(candidate);
                
                Console.WriteLine("candidate = {0}", candidate);
                Console.WriteLine("candidateTarget = {0}", candidateTarget);
                Console.WriteLine("_frequencyMarker = {0}", _frequencyMarker);
                
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
            var createdLink = Links.CreateAndUpdate(pair, frequency);
            Console.WriteLine("createdLink = {0}", createdLink);
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
            InitUnaryToUInt64(unaryOne);
        }
        
        private Dictionary<ulong, ulong> _unaryToUInt64;
        
        private void InitUnaryToUInt64(ulong unaryOne)
        {
            _unaryToUInt64 = new Dictionary<ulong, ulong>();
            
            _unaryToUInt64.Add(unaryOne, 1);
            
            var unary = unaryOne;
            ulong uInt64 = 1UL;
            
            for (var i = 1; i < 64; i++)
                _unaryToUInt64.Add(unary = Links.GetOrCreate(unary, unary), uInt64 = uInt64 * 2UL);
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
        
        public ulong ConvertUnaryNumberToUInt64(ulong unaryNumber)
        {
            if (unaryNumber == 0)
                return 0; 
            if (unaryNumber == _unaryOne)
                return 1;
            
            var source = Links.GetSource(unaryNumber);
            var target = Links.GetTarget(unaryNumber);
            
            if (source == target)
                return _unaryToUInt64[unaryNumber];
            else
            {
                ulong result = _unaryToUInt64[source];
                ulong lastValue = 0;
                while (!_unaryToUInt64.TryGetValue(target, out lastValue))
                {
                    source = Links.GetSource(target);
                    result += _unaryToUInt64[source];
                    target = Links.GetTarget(target);
                }
                result += lastValue;
                return result;
            }
        }
    }
}
