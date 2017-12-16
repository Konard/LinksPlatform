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
        public ulong CreateOptimalVariant(params ulong[] sequence)
        {
            var length = sequence.Length;

            if (length == 1)
                return sequence[0];

            var links = Links;
            
            if (length == 2)
                return links.GetOrCreate(sequence[0], sequence[1]);
                          
            sequence = sequence.ToArray();
            
            var levels = CalculateLocalElementLevels(sequence);
                    
            while (length > 2)
            {
                var levelRepeat = 1;
                var currentLevel = levels[0];
                var previousLevel = levels[0];
                var skipOnce = false;
                var w = 0;
                for (var i = 1; i < length; i++)
                {                        
                    if (currentLevel == levels[i])
                    {
                        levelRepeat++;
                        
                        skipOnce = false;
                        
                        if (levelRepeat == 2)
                        {
                            sequence[w] = links.GetOrCreate(sequence[i - 1], sequence[i]);                                                                  
                            var newLevel = i >= length - 1 ? 
                                GetPreviousLowerThanCurrentOrCurrent(previousLevel, currentLevel) :
                                i < 2 ?
                                GetNextLowerThanCurrentOrCurrent(currentLevel, levels[i + 1]) :
                                GetGreatestNeigbourLowerThanCurrentOrCurrent(previousLevel, currentLevel, levels[i + 1]);
                            levels[w] = newLevel;
                            previousLevel = currentLevel;
                            w++;
                            levelRepeat = 0;
                            skipOnce = true;
                        }
                        else if (i == length - 1)
                        {
                            sequence[w] = sequence[i];
                            levels[w] = levels[i];
                            w++;
                        }
                    }
                    else
                    {
                        currentLevel = levels[i];
                        levelRepeat = 1;                  
                                           
                        if (skipOnce) 
                            skipOnce = false;
                        else
                        {
                            sequence[w] = sequence[i - 1];
                            levels[w] = levels[i - 1];
                            previousLevel = levels[w];
                            w++;
                        }
                        if (i == length - 1)
                        {
                            sequence[w] = sequence[i];
                            levels[w] = levels[i];
                            w++;
                        }                                                
                    }
                }
                
                length = w;                
            }

            return links.GetOrCreate(sequence[0], sequence[1]);
        }
        
        private ulong GetGreatestNeigbourLowerThanCurrentOrCurrent(ulong previous, ulong current, ulong next)
        {
            if (previous > next)
                return previous < current ? previous : current;
            else
                return next < current ? next : current;
        }
        
        private ulong GetNextLowerThanCurrentOrCurrent(ulong current, ulong next)
        {            
            return next < current ? next : current;
        }
        
        private ulong GetPreviousLowerThanCurrentOrCurrent(ulong previous, ulong current)
        {
            return previous < current ? previous : current;          
        }
                        
        public ulong[] CalculateLocalElementLevels(ulong[] sequence)
        {
            var levels = new ulong[sequence.Length];
            levels[0] = GetPairFrequencyUInt64Number(sequence[0], sequence[1]);   
            
            for (var i = 1; i < sequence.Length - 1; i++)
            {
                var previous = GetPairFrequencyUInt64Number(sequence[i - 1], sequence[i]);
                var next = GetPairFrequencyUInt64Number(sequence[i], sequence[i + 1]);
                levels[i] = previous > next ? previous : next;
            }
            
            levels[levels.Length - 1] = GetPairFrequencyUInt64Number(sequence[sequence.Length - 2], sequence[sequence.Length - 1]);
            return levels;
        }
        
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
            
            var previousFrequencyContainer = GetPairFrequencyContainer(pair);
            var previousFrequency = GetPairFrequency(previousFrequencyContainer);
            var frequency = IncrementFrequency(previousFrequency);
                      
            SetPairFrequency(pair, previousFrequencyContainer, frequency);                       
        }
        
        public void PrintPairFrequency(ulong source, ulong target)
        {          
            var number = GetPairFrequencyUInt64Number(source, target);           
            Console.WriteLine("({0},{1}) - {2}", source, target, number);
        }
        
        public ulong GetPairFrequencyContainer(ulong pair)
        {
            var frequencyContainer = 0UL;
            
            var property = Links.SearchOrDefault(pair, _frequencyPropertyMarker);
            
            if (property == 0)
                return frequencyContainer;
            
            Links.Each(property, Constants.Any, candidate =>
            {
                var candidateTarget = Links.GetTarget(candidate);
                var frequencyTarget = Links.GetTarget(candidateTarget);
                              
                if (frequencyTarget == _frequencyMarker)
                {
                    frequencyContainer = candidate;
                    return Constants.Break;
                }
                
                return Constants.Continue;
            });
            
            return frequencyContainer;
        }
        
        public ulong GetPairFrequency(ulong frequencyContainer)
        {
            var frequency = frequencyContainer == 0 ? 0UL : Links.GetTarget(frequencyContainer);
            return frequency;
        }
            
        public ulong GetPairFrequencyUInt64Number(ulong source, ulong target)
        {
            var pair = Links.GetOrCreate(source, target);
            var previousFrequencyContainer = GetPairFrequencyContainer(pair);
            var frequency = GetPairFrequency(previousFrequencyContainer);
            if (frequency == 0)
                return 0;
            var frequencyNumber = Links.GetSource(frequency);
            var number = ConvertUnaryNumberToUInt64(frequencyNumber);
            return number;
        }
        
        public ulong IncrementFrequency(ulong frequency)
        {
            if (frequency == 0)
                return Links.GetOrCreate(_unaryOne, _frequencyMarker);
            var source = Links.GetSource(frequency);
            var incrementedSource = IncrementUnaryNumber(source);
            return Links.GetOrCreate(incrementedSource, _frequencyMarker);
        }
        
        public void SetPairFrequency(ulong pair, ulong previousFrequencyContainer, ulong frequency)
        {
            if (previousFrequencyContainer != 0)
                Links.Delete(previousFrequencyContainer);
            var property = Links.GetOrCreate(pair, _frequencyPropertyMarker);
            Links.CreateAndUpdate(property, frequency);            
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
        
        private ulong _frequencyPropertyMarker;
        
        public ulong GetFrequencyProperyMarker()
        {
            return _frequencyPropertyMarker;
        }
        
        public void SetFrequencyPropertyMarker(ulong frequencyPropertyMarker)
        {
            _frequencyPropertyMarker = frequencyPropertyMarker;
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
