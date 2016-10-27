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
        private readonly UnsafeDictionary<Pair, Data> _pairsCache;
        private Pair _maxPair;
        private Data _maxPairData;

        private struct HalfPair
        {
            public ulong Element;
            public Data PairData;

            public HalfPair(ulong element, Data pairData)
            {
                Element = element;
                PairData = pairData;
            }

            public override string ToString()
            {
                return $"{Element}: ({PairData})";
            }
        }

        private struct Pair
        {
            public ulong Source;
            public ulong Target;

            public Pair(ulong source, ulong target)
            {
                Source = source;
                Target = target;
            }

            public override string ToString()
            {
                return $"{Source}->{Target}";
            }
        }

        private class Data
        {
            public ulong Frequency;
            public ulong Link;

            public Data(ulong frequency, ulong link)
            {
                Frequency = frequency;
                Link = link;
            }

            public Data()
            {
            }

            public override string ToString()
            {
                return $"F: {Frequency}, L: {Link}";
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

            // Looks like a more correct way to calculate usage than _links.Count(Constants.Any, link);
            _countLinkFrequency = _sequences.CalculateTotalSymbolFrequencyCore; // TODO: Why +1 or -1 happens?

            if (minFrequencyToCompress == 0) minFrequencyToCompress = 1;
            _minFrequencyToCompress = minFrequencyToCompress;
            ResetMaxPair();
            _pairsCache = new UnsafeDictionary<Pair, Data>(4096, PairComparer.Default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Data IncrementFrequency(ulong source, ulong target)
        {
            var pair = new Pair(source, target);

            return IncrementFrequency(ref pair);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Data IncrementFrequency(ref Pair pair)
        {
            Data data;
            var entryIndex = _pairsCache.FindEntry(pair);
            if (entryIndex >= 0)
            {
                data = _pairsCache.entries[entryIndex].value;
                data.Frequency++;
            }
            else
            {
                var link = _links.SearchOrDefault(pair.Source, pair.Target);

                data = new Data(1, link);

                if (link != default(ulong))
                    data.Frequency += _countLinkFrequency(link);
                    
                _pairsCache.InsertUnsafe(pair, data);
            }
            return data;
        }

        public void ResetFrequencies()
        {
            _pairsCache.Clear();
        }

        public void ValidateFrequencies()
        {
            var entries = _pairsCache.entries;
            var length = _pairsCache.count;

            for (var i = 0; i < length; i++)
            {
                if (entries[i].hashCode < 0) continue;

                var value = entries[i].value;
                var linkIndex = value.Link;

                if (linkIndex != default(ulong))
                {
                    var frequency = value.Frequency;
                    var count = _countLinkFrequency(linkIndex);

                    if ((frequency > count && frequency - count > _minFrequencyToCompress) || (count > frequency && count - frequency > _minFrequencyToCompress))
                        throw new Exception("Frequencies validation failed.");
                }
                //else
                //{
                //    if (value.Frequency > 0)
                //    {
                //        var frequency = value.Frequency;
                //        linkIndex = _createLink(entries[i].key.Source, entries[i].key.Target);
                //        var count = _countLinkFrequency(linkIndex);

                //        if ((frequency > count && frequency - count > 1) || (count > frequency && count - frequency > 1))
                //            throw new Exception("Frequencies validation failed.");
                //    }
                //}
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
                
            // TODO: arraypool with min size (to improve cache locality)
            var copy = new HalfPair[sequence.Length];

            var pair = new Pair();

            for (var i = 1; i < sequence.Length; i++)
            {
                pair.Source = sequence[i - 1];
                pair.Target = sequence[i];

                var data = IncrementFrequency(ref pair);

                copy[i - 1].Element = sequence[i - 1];
                copy[i - 1].PairData = data;

                UpdateMaxPair(ref pair, data);
            }
            copy[sequence.Length - 1].Element = sequence[sequence.Length - 1];
            copy[sequence.Length - 1].PairData = new Data();

            if (_maxPairData.Frequency > default(ulong))
            {
                var newLength = ReplacePairs(copy);

                sequence = new ulong[newLength];

                for (int i = 0; i < newLength; i++)
                    sequence[i] = copy[i].Element;
            }

            return sequence;
        }

        /// <remarks>
        /// Original algorithm idea: https://en.wikipedia.org/wiki/Byte_pair_encoding .
        /// </remarks>
        private int ReplacePairs(HalfPair[] copy)
        {
            var oldLength = copy.Length;
            var newLength = copy.Length;

            while (_maxPairData.Frequency > default(ulong))
            {
                var maxPairSource = _maxPair.Source;
                var maxPairTarget = _maxPair.Target;

                if (_maxPairData.Link == Constants.Null)
                    _maxPairData.Link = _createLink(maxPairSource, maxPairTarget);

                var maxPairReplacementLink = _maxPairData.Link;

                oldLength--;
                var oldLengthMinusTwo = oldLength - 1;

                // Substitute all usages
                int w = 0, r = 0; // (r == read, w == write)
                for (; r < oldLength; r++)
                {
                    if (copy[r].Element == maxPairSource && copy[r + 1].Element == maxPairTarget)
                    {
                        if (r > 0)
                        {
                            var previous = copy[w - 1].Element;
                            copy[w - 1].PairData.Frequency--;
                            copy[w - 1].PairData = IncrementFrequency(previous, maxPairReplacementLink);
                        }
                        if (r < oldLengthMinusTwo)
                        {
                            var next = copy[r + 2].Element;
                            copy[r + 1].PairData.Frequency--;
                            copy[w].PairData = IncrementFrequency(maxPairReplacementLink, next);
                        }

                        copy[w++].Element = maxPairReplacementLink;
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
        private void UpdateMaxPair(HalfPair[] copy, int length)
        {
            var pair = new Pair();

            for (var i = 1; i < length; i++)
            {
                pair.Source = copy[i - 1].Element;
                pair.Target = copy[i].Element;

                UpdateMaxPair(ref pair, copy[i - 1].PairData);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateMaxPair(ref Pair pair, Data data)
        {
            var frequency = data.Frequency;
            var maxFrequency = _maxPairData.Frequency;

            //if (frequency > _minFrequencyToCompress && (maxFrequency < frequency || maxFrequency == frequency && pair.Source + pair.Target < /* gives better compression string data (and gives collisions quickly) */ _maxPair.Source + _maxPair.Target)) 
            if (frequency > _minFrequencyToCompress && (maxFrequency < frequency || maxFrequency == frequency && pair.Source + pair.Target > /* gives better stability and better compression on sequent data (but gives collisions anyway) */ _maxPair.Source + _maxPair.Target)) 
            {
                _maxPair = pair;
                _maxPairData = data;
            }
        }
    }
}
