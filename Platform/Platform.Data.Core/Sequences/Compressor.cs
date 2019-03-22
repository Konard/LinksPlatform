using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Platform.Data.Core.Doublets;
using Platform.Helpers;
using Platform.Helpers.Collections;

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
        private readonly Dictionary<Doublet, Data> _doubletsCache;
        private Doublet _maxDoublet;
        private Data _maxDoubletData;

        private struct HalfDoublet
        {
            public ulong Element;
            public Data DoubletData;

            public HalfDoublet(ulong element, Data doubletData)
            {
                Element = element;
                DoubletData = doubletData;
            }

            public override string ToString()
            {
                return $"{Element}: ({DoubletData})";
            }
        }

        private struct Doublet
        {
            public ulong Source;
            public ulong Target;

            public Doublet(ulong source, ulong target)
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
        private class DoubletComparer : IEqualityComparer<Doublet>
        {
            public static readonly DoubletComparer Default = new DoubletComparer();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Equals(Doublet x, Doublet y) => x.Source == y.Source && x.Target == y.Target;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int GetHashCode(Doublet obj) => (int)((obj.Source << 15) ^ obj.Target);
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
            ResetMaxDoublet();
            _doubletsCache = new Dictionary<Doublet, Data>(4096, DoubletComparer.Default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Data IncrementFrequency(ulong source, ulong target)
        {
            var doublet = new Doublet(source, target);

            return IncrementFrequency(ref doublet);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Data IncrementFrequency(ref Doublet doublet)
        {
            Data data;
            if (_doubletsCache.TryGetValue(doublet, out data))
                data.Frequency++;                  
            else
            {
                var link = _links.SearchOrDefault(doublet.Source, doublet.Target);

                data = new Data(1, link);

                if (link != default(ulong))
                    data.Frequency += _countLinkFrequency(link);
                    
                _doubletsCache.Add(doublet, data);
            }
            return data;
        }

        public void ResetFrequencies()
        {
            _doubletsCache.Clear();
        }

        public void ValidateFrequencies()
        {
            foreach(var entry in _doubletsCache)
            {
                var value = entry.Value;
                   
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
                //        linkIndex = _createLink(entry.Key.Source, entry.Key.Target);
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
        /// Faster version (doublets' frequencies dictionary is not recreated).
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
            var copy = new HalfDoublet[sequence.Length];

            var doublet = new Doublet();

            for (var i = 1; i < sequence.Length; i++)
            {
                doublet.Source = sequence[i - 1];
                doublet.Target = sequence[i];

                var data = IncrementFrequency(ref doublet);

                copy[i - 1].Element = sequence[i - 1];
                copy[i - 1].DoubletData = data;

                UpdateMaxDoublet(ref doublet, data);
            }
            copy[sequence.Length - 1].Element = sequence[sequence.Length - 1];
            copy[sequence.Length - 1].DoubletData = new Data();

            if (_maxDoubletData.Frequency > default(ulong))
            {
                var newLength = ReplaceDoublets(copy);

                sequence = new ulong[newLength];

                for (int i = 0; i < newLength; i++)
                    sequence[i] = copy[i].Element;
            }

            return sequence;
        }

        /// <remarks>
        /// Original algorithm idea: https://en.wikipedia.org/wiki/Byte_pair_encoding .
        /// </remarks>
        private int ReplaceDoublets(HalfDoublet[] copy)
        {
            var oldLength = copy.Length;
            var newLength = copy.Length;

            while (_maxDoubletData.Frequency > default(ulong))
            {
                var maxDoubletSource = _maxDoublet.Source;
                var maxDoubletTarget = _maxDoublet.Target;

                if (_maxDoubletData.Link == Constants.Null)
                    _maxDoubletData.Link = _createLink(maxDoubletSource, maxDoubletTarget);

                var maxDoubletReplacementLink = _maxDoubletData.Link;

                oldLength--;
                var oldLengthMinusTwo = oldLength - 1;

                // Substitute all usages
                int w = 0, r = 0; // (r == read, w == write)
                for (; r < oldLength; r++)
                {
                    if (copy[r].Element == maxDoubletSource && copy[r + 1].Element == maxDoubletTarget)
                    {
                        if (r > 0)
                        {
                            var previous = copy[w - 1].Element;
                            copy[w - 1].DoubletData.Frequency--;
                            copy[w - 1].DoubletData = IncrementFrequency(previous, maxDoubletReplacementLink);
                        }
                        if (r < oldLengthMinusTwo)
                        {
                            var next = copy[r + 2].Element;
                            copy[r + 1].DoubletData.Frequency--;
                            copy[w].DoubletData = IncrementFrequency(maxDoubletReplacementLink, next);
                        }

                        copy[w++].Element = maxDoubletReplacementLink;
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

                ResetMaxDoublet();

                UpdateMaxDoublet(copy, newLength);
            }

            return newLength;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResetMaxDoublet()
        {
            _maxDoublet = new Doublet();
            _maxDoubletData = new Data();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateMaxDoublet(HalfDoublet[] copy, int length)
        {
            var doublet = new Doublet();

            for (var i = 1; i < length; i++)
            {
                doublet.Source = copy[i - 1].Element;
                doublet.Target = copy[i].Element;

                UpdateMaxDoublet(ref doublet, copy[i - 1].DoubletData);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateMaxDoublet(ref Doublet doublet, Data data)
        {
            var frequency = data.Frequency;
            var maxFrequency = _maxDoubletData.Frequency;

            //if (frequency > _minFrequencyToCompress && (maxFrequency < frequency || maxFrequency == frequency && doublet.Source + doublet.Target < /* gives better compression string data (and gives collisions quickly) */ _maxDoublet.Source + _maxDoublet.Target)) 
            if (frequency > _minFrequencyToCompress && (maxFrequency < frequency || maxFrequency == frequency && doublet.Source + doublet.Target > /* gives better stability and better compression on sequent data and even on rundom numbers data (but gives collisions anyway) */ _maxDoublet.Source + _maxDoublet.Target)) 
            {
                _maxDoublet = doublet;
                _maxDoubletData = data;
            }
        }
    }
}
