using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Platform.Data.Core.Pairs;
using Platform.Helpers;
using Platform.Helpers.Collections;
using Platform.Helpers.Collections.Optimizations;

namespace Platform.Data.Core.Sequences
{
    /// <remarks>
    /// TODO: Возможно будет лучше если алгоритм будет выполняться полностью изолированно от Links на этапе сжатия.
    ///     А именно будет создаваться временный список пар необходимых для выполнения сжатия, в таком случае тип значения элемента массива может быть любым, как char так и ulong.
    ///     Как только список/словарь пар был выявлен можно разом выполнить создание всех этих пар, а так же разом выполнить замену.
    /// </remarks>
    public class Compressor
    {
        private static readonly LinksConstants<bool, ulong, long> Constants = Default<LinksConstants<bool, ulong, long>>.Instance;

        private readonly Func<ulong, ulong, ulong> _createLink;
        private readonly Func<ulong[], ulong> _createSequence;
        private UInt64Link _maxPair;
        private readonly ILinks<ulong> _links;
        private readonly ulong _minFrequencyToCompress;
        private ulong _maxFrequency;
        private UnsafeDictionary<UInt64Link, ulong> _pairsFrequencies;

        /// <remarks>
        /// TODO: Может стоит попробовать ref во всех методах
        /// </remarks>
        public class LinkComparer : IEqualityComparer<UInt64Link>
        {
            public static readonly LinkComparer Default = new LinkComparer();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Equals(UInt64Link x, UInt64Link y) => x.Source == y.Source &&
                                                              x.Target == y.Target;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int GetHashCode(UInt64Link obj)
            {
                unchecked // Overflow is fine, just wrap
                {
                    return (int)((obj.Source << 16) ^ obj.Target);
                }
            }
        }

        public Compressor(ILinks<ulong> links, Sequences sequences, ulong minFrequencyToCompress = 1)
        {
            _links = links;
            _createLink = _links.GetOrCreate;
            _createSequence = sequences.CreateBalancedVariantCore;

            if (minFrequencyToCompress == 0) minFrequencyToCompress = 1;
            _minFrequencyToCompress = minFrequencyToCompress;
            ResetMaxPair();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ulong IncrementFrequency(UInt64Link pair)
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
        private void DecrementFrequency(UInt64Link pair)
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

        public ulong Compress(ulong[] sequence) => _createSequence(Precompress(sequence));

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
                return new[] { _createLink(sequence[0], sequence[1]) };

            if (_pairsFrequencies == null)
                _pairsFrequencies = new UnsafeDictionary<UInt64Link, ulong>(4096, LinkComparer.Default);

            for (var i = 1; i < sequence.Length; i++)
            {
                var source = sequence[i - 1];
                var target = sequence[i];

                var link = _links.SearchOrDefault(source, target);
                var pair = new UInt64Link(link, source, target);

                // Preload actual frequency from storage {
                if (link != default(ulong))
                {
                    ulong frequency;
                    if (!_pairsFrequencies.TryGetValue(pair, out frequency))
                        _pairsFrequencies.Add(pair, _links.Count(Constants.Any, link));
                }
                // }

                UpdateMaxPair(pair, IncrementFrequency(pair));
            }

            if (_maxFrequency > _minFrequencyToCompress)
            {
                // Can be faster if source sequence allowed to be changed
                // TODO: Try to use ArrayPool
                var copy = new ulong[sequence.Length];
                Array.Copy(sequence, copy, sequence.Length);

                var newLength = ReplacePairs(copy);

                var final = new ulong[newLength];
                Array.Copy(copy, final, newLength);

                return final;
            }

            return sequence;
        }

        /// <remarks>
        /// Original algorithm idea: https://en.wikipedia.org/wiki/Byte_pair_encoding .
        /// Faster version (pairs' frequencies dictionary is not recreated).
        /// </remarks>
        private int ReplacePairs(ulong[] copy)
        {
            var oldLength = copy.Length;
            var newLength = copy.Length;

            while (!_maxPair.IsNull())
            {
                var maxPairSource = _maxPair.Source;
                var maxPairTarget = _maxPair.Target;
                ulong maxPairResult;
                if (_maxPair.Index == Constants.Null)
                {
                    maxPairResult = _createLink(maxPairSource, maxPairTarget);
                    _pairsFrequencies.Remove(_maxPair);
                    _maxPair = new UInt64Link(maxPairResult, _maxPair.Source, _maxPair.Target);
                    _pairsFrequencies.Add(_maxPair, _maxFrequency);
                }
                else
                {
                    maxPairResult = _maxPair.Index;
                }

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
                            DecrementFrequency(new UInt64Link(previous, maxPairSource));
                            IncrementFrequency(new UInt64Link(previous, maxPairResult));
                        }
                        if (r < oldLengthMinusTwo)
                        {
                            var next = copy[r + 2];
                            DecrementFrequency(new UInt64Link(maxPairTarget, next));
                            IncrementFrequency(new UInt64Link(maxPairResult, next));
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
                if (w < newLength) copy[w] = copy[r];

                oldLength = newLength;

                UpdateMaxPair(copy, newLength);
            }

            return newLength;
        }

        private void ResetMaxPair()
        {
            _maxPair = UInt64Link.Null;
            _maxFrequency = 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateMaxPair(ulong[] sequence, int length)
        {
            ResetMaxPair();

            for (var i = 1; i < length; i++)
            {
                var pair = new UInt64Link(sequence[i - 1], sequence[i]);

                ulong frequency;
                UnsafeDictionary<UInt64Link, ulong>.Entry entry;
                if (_pairsFrequencies.TryGetEntry(pair, out entry))
                {
                    pair = entry.key;
                    frequency = entry.value;
                }
                else
                    throw new Exception("Pair frequency not found.");

                UpdateMaxPair(pair, frequency);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateMaxPair(UInt64Link pair, ulong frequency)
        {
            if (frequency > _minFrequencyToCompress)
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
    }
}
