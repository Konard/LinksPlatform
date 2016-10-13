using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly Func<ulong, ulong> _countLinkFrequency;
        private readonly ILinks<ulong> _links;
        private readonly Sequences _sequences;
        private readonly ulong _minFrequencyToCompress;
        private Pair _maxPair;
        private Data _maxPairData;
        private UnsafeDictionary<Pair, Data> _pairsFrequencies;

        private struct Pair
        {
            public ulong Source;
            public ulong Target;

            public Pair(ulong source, ulong target)
            {
                Source = source;
                Target = target;
            }
        }

        private struct Data
        {
            public ulong Frequency;
            public ulong Link;

            public Data(ulong frequency, ulong link)
            {
                Frequency = frequency;
                Link = link;
            }
        }

        /// <remarks>
        /// TODO: Может стоит попробовать ref во всех методах (IRefEqualityComparer)
        /// 2x faster with comparer 
        /// </remarks>
        private class PairComparer : IEqualityComparer<Pair>
        {
            public static readonly PairComparer Default = new PairComparer();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Equals(Pair x, Pair y) => x.Source == y.Source && x.Target == y.Target;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int GetHashCode(Pair obj) => (int)((obj.Source << 15) ^ obj.Target);
        }

        public Compressor(ILinks<ulong> links, Sequences sequences, ulong minFrequencyToCompress = 1)
        {
            _links = links;
            _sequences = sequences;
            _createLink = _links.GetOrCreate;
            _createSequence = _sequences.CreateBalancedVariantCore;
            _countLinkFrequency = link =>
            {
                // Looks like a more correct way to calculate usage than _links.Count(Constants.Any, link);

                var usagesSet = _sequences.AllBottomUsages(link);
                var total = (ulong)usagesSet.Sum(x => (long)_sequences.CalculateSymbolFrequency(x, link));
                return total; // TODO: Why +1 or -1 happens?
            };

            if (minFrequencyToCompress == 0) minFrequencyToCompress = 1;
            _minFrequencyToCompress = minFrequencyToCompress;
            ResetMaxPair();
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private void IncrementFrequency(Pair pair, out Data data)
        //{
        //    if (_pairsFrequencies.TryGetValue(pair, out data))
        //    {
        //        if (data.Frequency < ulong.MaxValue)
        //        {
        //            data.Frequency++;
        //            _pairsFrequencies[pair] = data;
        //        }
        //    }
        //    else
        //    {
        //        data.Frequency++;
        //        _pairsFrequencies.Add(pair, data);
        //    }
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void IncrementFrequency(ulong source, ulong target)
        {
            var pair = new Pair(source, target);

            //Data data;
            //if (_pairsFrequencies.TryGetValue(pair, out data))
            //{
            //    if (data.Frequency < ulong.MaxValue)
            //    {
            //        data.Frequency++;
            //        _pairsFrequencies[pair] = data;
            //    }
            //}
            //else
            //{
            //    data.Frequency++;
            //    _pairsFrequencies.Add(pair, data);
            //}

            var entryIndex = _pairsFrequencies.FindEntry(ref pair);
            if (entryIndex >= 0)
                _pairsFrequencies.entries[entryIndex].value.Frequency++;
            else
                _pairsFrequencies.InsertUnsafe(pair, new Data(1, default(ulong)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DecrementFrequency(ulong source, ulong target)
        {
            var pair = new Pair(source, target);

            //Data data;
            //if (_pairsFrequencies.TryGetValue(pair, out data))
            //{
            //    if (data.Frequency > ulong.MinValue)
            //    {
            //        data.Frequency--;
            //        _pairsFrequencies[pair] = data;
            //    }
            //}

            var entryIndex = _pairsFrequencies.FindEntry(ref pair);
            if (entryIndex >= 0)
                _pairsFrequencies.entries[entryIndex].value.Frequency--;
        }

        public void ResetFrequencies()
        {
            _pairsFrequencies.Clear();
        }

        public void ValidateFrequencies()
        {
            var entries = _pairsFrequencies.entries;
            var length = _pairsFrequencies.count;

            for (var i = 0; i < length; i++)
            {
                if (entries[i].hashCode < 0) continue;

                var value = entries[i].value;
                var linkIndex = value.Link;

                if (linkIndex != Constants.Null)
                {
                    var frequency = value.Frequency;
                    var count = _countLinkFrequency(linkIndex);

                    if ((frequency > count && frequency - count > 1) || (count > frequency && count - frequency > 1))
                        throw new Exception("Frequencies validation failed.");
                }
            }
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
                _pairsFrequencies = new UnsafeDictionary<Pair, Data>(4096, PairComparer.Default);

            for (var i = 1; i < sequence.Length; i++)
            {
                var source = sequence[i - 1];
                var target = sequence[i];

                var pair = new Pair(source, target);

                // Preload actual frequency from storage

                //Data data;
                //if (!_pairsFrequencies.TryGetValue(pair, out data))
                //{
                //    data.Link = _links.SearchOrDefault(source, target);
                //    if (data.Link != default(ulong))
                //    {
                //        data.Frequency = _countLinkFrequency(data.Link);
                //        _pairsFrequencies.Add(pair, data);
                //    }
                //}
                //IncrementFrequency(pair, out data);

                Data data;
                var entryIndex = _pairsFrequencies.FindEntry(pair);
                if (entryIndex >= 0)
                {
                    _pairsFrequencies.entries[entryIndex].value.Frequency++;
                    data = _pairsFrequencies.entries[entryIndex].value;
                }
                else
                {
                    data.Link = _links.SearchOrDefault(source, target);
                    if (data.Link != default(ulong))
                        data.Frequency = _countLinkFrequency(data.Link) + 1;
                    else
                        data.Frequency = 1;
                    _pairsFrequencies.InsertUnsafe(pair, data);
                }

                UpdateMaxPair(ref pair, ref data);
            }

            if (_maxPairData.Frequency > _minFrequencyToCompress)
            {
                // Can be faster if source sequence allowed to be changed
                var copy = new ulong[sequence.Length];
                //var copy = ArrayPool.Allocate<ulong>(sequence.Length);
                Array.Copy(sequence, copy, sequence.Length);

                var newLength = ReplacePairs(copy);

                sequence = new ulong[newLength];
                Array.Copy(copy, sequence, newLength);
                //ArrayPool.Free(copy);
            }

            return sequence;
        }

        /// <remarks>
        /// Original algorithm idea: https://en.wikipedia.org/wiki/Byte_pair_encoding .
        /// </remarks>
        private int ReplacePairs(ulong[] copy)
        {
            var oldLength = copy.Length;
            var newLength = copy.Length;

            while (_maxPair.Source != default(ulong) && _maxPair.Target != default(ulong))
            {
                var maxPairSource = _maxPair.Source;
                var maxPairTarget = _maxPair.Target;

                if (_maxPairData.Link == Constants.Null)
                {
                    _maxPairData.Link = _createLink(maxPairSource, maxPairTarget);
                    _pairsFrequencies[_maxPair] = _maxPairData;
                }

                var maxPairReplacementLink = _maxPairData.Link;

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
                            DecrementFrequency(previous, maxPairSource);
                            IncrementFrequency(previous, maxPairReplacementLink);
                        }
                        if (r < oldLengthMinusTwo)
                        {
                            var next = copy[r + 2];
                            DecrementFrequency(maxPairTarget, next);
                            IncrementFrequency(maxPairReplacementLink, next);
                        }

                        copy[w++] = maxPairReplacementLink;
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

                ResetMaxPair();

                UpdateMaxPair(copy, newLength);
            }

            return newLength;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResetMaxPair()
        {
            _maxPair = new Pair();
            _maxPairData = new Data();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateMaxPair(ulong[] sequence, int length)
        {
            for (var i = length - 1; i >= 1; i--)
            {
                var pair = new Pair(sequence[i - 1], sequence[i]);
                //var data = _pairsFrequencies[pair];
                //UpdateMaxPair(ref pair, ref data);

                UpdateMaxPair(ref pair, ref _pairsFrequencies.entries[_pairsFrequencies.FindEntry(ref pair)].value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateMaxPair(ref Pair pair, ref Data data)
        {
            var frequency = data.Frequency;
            var maxFrequency = _maxPairData.Frequency;
            if (frequency > _minFrequencyToCompress)
            {
                if (maxFrequency < frequency)
                {
                    _maxPair = pair;
                    _maxPairData = data;
                }
                //else if (maxFrequency == frequency && (_maxPair.Source > pair.Source || (_maxPair.Source == pair.Source && _maxPair.Target > pair.Target))) // Gives worse compression
                else if (maxFrequency == frequency && pair.Source + pair.Target < _maxPair.Source + _maxPair.Target) // Gives better compression
                {
                    _maxPair = pair;
                    _maxPairData = data;
                }
            }
        }
    }
}
