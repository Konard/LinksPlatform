using System;
using System.Runtime.CompilerServices;
using Platform.Data.Core.Pairs;
using Platform.Data.Core.Structures;
using Platform.Helpers.Collections;

namespace Platform.Data.Core.Sequences
{
    public class Compressor
    {
        private readonly Links _links;
        private readonly Sequences _sequences;
        private Link _maxPair;
        private ulong _maxFrequency;
        private UnsafeDictionary<Link, ulong> _pairsFrequencies;

        public Compressor(Links links, Sequences sequences)
        {
            _links = links;
            _sequences = sequences;
            ResetMaxPair();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ulong IncrementFrequency(Link pair)
        {
            ulong frequency;
            if (_pairsFrequencies.TryGetValue(pair, out frequency))
            {
                frequency++;
                _pairsFrequencies[pair] = frequency;
            }
            else
            {
                frequency = 1;
                _pairsFrequencies.Add(pair, frequency);
            }
            return frequency;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DecrementFrequency(Link pair)
        {
            ulong frequency;
            if (_pairsFrequencies.TryGetValue(pair, out frequency))
            {
                frequency--;

                if (frequency == 0)
                    _pairsFrequencies.Remove(pair);
                else
                    _pairsFrequencies[pair] = frequency;
            }
            //return frequency;
        }

        /// <remarks>
        /// Original algorithm idea: https://en.wikipedia.org/wiki/Byte_pair_encoding .
        /// Faster version (pairs' frequencies dictionary is not recreated).
        /// </remarks>
        public ulong[] Precompress(ulong[] sequence)
        {
            if (sequence.IsNullOrEmpty())
                return null;

            if (sequence.Length == 1)
                return sequence;

            if (sequence.Length == 2)
                return new [] { _links.Create(sequence[0], sequence[1]) };

            if (_pairsFrequencies != null)
                throw new InvalidOperationException("Only one sequence at a time can be precompresed using single compressor.");

            _pairsFrequencies = new UnsafeDictionary<Link, ulong>();

            var oldLength = sequence.Length;
            var newLength = sequence.Length;

            // Can be faster if source sequence allowed to be changed
            var copy = new ulong[sequence.Length];
            copy[0] = sequence[0];

            for (var i = 1; i < sequence.Length; i++)
            {
                copy[i] = sequence[i];

                var pair = new Link(sequence[i - 1], sequence[i]);
                UpdateMaxPair(pair, IncrementFrequency(pair));
            }

            while (!_maxPair.IsNull())
            {
                var maxPairSource = _maxPair.Source;
                var maxPairTarget = _maxPair.Target;
                var maxPairResult = _links.Create(maxPairSource, maxPairTarget);

                oldLength--;
                var oldLengthMinusTwo = oldLength - 1;

                // Substitute all usages
                int w = 0, r = 0; // (r == read, w == write)
                for (; r < oldLength; r++)
                {
                    if (copy[r] == maxPairSource && copy[r + 1] == maxPairTarget)
                    {
                        if (r > 0)
                        {
                            var previous = copy[w - 1];
                            DecrementFrequency(new Link(previous, maxPairSource));
                            IncrementFrequency(new Link(previous, maxPairResult));
                        }
                        if (r < oldLengthMinusTwo)
                        {
                            var next = copy[r + 2];
                            DecrementFrequency(new Link(maxPairTarget, next));
                            IncrementFrequency(new Link(maxPairResult, next));
                        }

                        copy[w++] = maxPairResult;
                        r++;
                        newLength--;
                    }
                    else
                    {
                        copy[w++] = copy[r];
                    }
                }
                copy[w] = copy[r];

                _pairsFrequencies.Remove(_maxPair);

                oldLength = newLength;

                // Быстрее
                UpdateMaxPair();
            }

            _pairsFrequencies = null;

            var final = new ulong[newLength];
            Array.Copy(copy, final, newLength);

            return final;
        }

        public ulong Compress(ulong[] sequence)
        {
            var precompressedSequence = Precompress(sequence);
            return _sequences.CreateBalancedVariant(precompressedSequence);
        }

        private void ResetMaxPair()
        {
            _maxPair = Link.Null;
            _maxFrequency = 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateMaxPair(Link pair, ulong frequency)
        {
            if (frequency > 1)
            {
                if (_maxFrequency < frequency)
                {
                    _maxFrequency = frequency;
                    _maxPair = pair;
                }
                else if (_maxFrequency == frequency &&
                    (pair.Source + pair.Target) > (_maxPair.Source + _maxPair.Target))
                {
                    _maxPair = pair;
                }
            }
        }

        private void UpdateMaxPair()
        {
            ResetMaxPair();

            var entries = _pairsFrequencies.entries;
            for (var i = 0; i < entries.Length; i++)
            {
                if (entries[i].hashCode >= 0)
                {
                    var frequency = entries[i].value;
                    if (frequency > 1)
                    {
                        if (_maxFrequency > frequency)
                            continue;

                        if (_maxFrequency < frequency)
                        {
                            _maxFrequency = frequency;
                            _maxPair = entries[i].key;
                        }
                        else if (_maxFrequency == frequency &&
                            (entries[i].key.Source + entries[i].key.Target) > (_maxPair.Source + _maxPair.Target))
                        {
                            _maxPair = entries[i].key;
                        }
                    }
                }
            }
        }
    }
}
