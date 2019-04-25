using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Platform.Helpers;
using Platform.Helpers.Collections;
using Platform.Data.Core.Doublets;

namespace Platform.Data.Core.Sequences
{
    /// <remarks>
    /// TODO: Возможно будет лучше если алгоритм будет выполняться полностью изолированно от Links на этапе сжатия.
    ///     А именно будет создаваться временный список пар необходимых для выполнения сжатия, в таком случае тип значения элемента массива может быть любым, как char так и ulong.
    ///     Как только список/словарь пар был выявлен можно разом выполнить создание всех этих пар, а так же разом выполнить замену.
    /// </remarks>
    public class CompressingConverter<TLink> : LinksListToSequenceConverterBase<TLink>
    {
        private static readonly LinksConstants<bool, TLink, long> Constants = Default<LinksConstants<bool, TLink, long>>.Instance;

        private readonly IConverter<IList<TLink>, TLink> _baseConverter;
        private readonly DoubletFrequenciesCache<TLink> _doubletFrequenciesCache;
        private readonly TLink _minFrequencyToCompress;
        private Link<TLink> _maxDoublet;
        private FrequencyAndLink<TLink> _maxDoubletData;

        private struct HalfDoublet
        {
            public TLink Element;
            public FrequencyAndLink<TLink> DoubletData;

            public HalfDoublet(TLink element, FrequencyAndLink<TLink> doubletData)
            {
                Element = element;
                DoubletData = doubletData;
            }

            public override string ToString() => $"{Element}: ({DoubletData})";
        }

        public CompressingConverter(ILinks<TLink> links, IConverter<IList<TLink>, TLink> baseConverter, DoubletFrequenciesCache<TLink> doubletFrequenciesCache)
            : this(links, baseConverter, doubletFrequenciesCache, Integer<TLink>.One)
        {
        }

        public CompressingConverter(ILinks<TLink> links, IConverter<IList<TLink>, TLink> baseConverter, DoubletFrequenciesCache<TLink> doubletFrequenciesCache, TLink minFrequencyToCompress)
            : base(links)
        {
            _baseConverter = baseConverter;
            _doubletFrequenciesCache = doubletFrequenciesCache;

            if (MathHelpers.LessThan(minFrequencyToCompress, Integer<TLink>.One)) minFrequencyToCompress = Integer<TLink>.One;
            _minFrequencyToCompress = minFrequencyToCompress;

            ResetMaxDoublet();
        }

        public override TLink Convert(IList<TLink> source) => _baseConverter.Convert(Compress(source));

        /// <remarks>
        /// Original algorithm idea: https://en.wikipedia.org/wiki/Byte_pair_encoding .
        /// Faster version (doublets' frequencies dictionary is not recreated).
        /// </remarks>
        private IList<TLink> Compress(IList<TLink> sequence)
        {
            if (sequence.IsNullOrEmpty())
                return null;

            if (sequence.Count == 1)
                return sequence;

            if (sequence.Count == 2)
                return new[] { Links.GetOrCreate(sequence[0], sequence[1]) };

            // TODO: arraypool with min size (to improve cache locality)
            var copy = new HalfDoublet[sequence.Count];

            for (var i = 1; i < sequence.Count; i++)
            {
                var doublet = new Link<TLink>(sequence[i - 1], sequence[i]);

                var data = _doubletFrequenciesCache.IncrementFrequency(ref doublet);

                copy[i - 1].Element = sequence[i - 1];
                copy[i - 1].DoubletData = data;

                UpdateMaxDoublet(ref doublet, data);
            }
            copy[sequence.Count - 1].Element = sequence[sequence.Count - 1];
            copy[sequence.Count - 1].DoubletData = new FrequencyAndLink<TLink>();

            if (MathHelpers.GreaterThan(_maxDoubletData.Frequency, default))
            {
                var newLength = ReplaceDoublets(copy);

                sequence = new TLink[newLength];

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

            while (MathHelpers.GreaterThan(_maxDoubletData.Frequency, default))
            {
                var maxDoubletSource = _maxDoublet.Source;
                var maxDoubletTarget = _maxDoublet.Target;

                if (MathHelpers<TLink>.IsEquals(_maxDoubletData.Link, Constants.Null))
                    _maxDoubletData.Link = Links.GetOrCreate(maxDoubletSource, maxDoubletTarget);

                var maxDoubletReplacementLink = _maxDoubletData.Link;

                oldLength--;
                var oldLengthMinusTwo = oldLength - 1;

                // Substitute all usages
                int w = 0, r = 0; // (r == read, w == write)
                for (; r < oldLength; r++)
                {
                    if (MathHelpers<TLink>.IsEquals(copy[r].Element, maxDoubletSource) && MathHelpers<TLink>.IsEquals(copy[r + 1].Element, maxDoubletTarget))
                    {
                        if (r > 0)
                        {
                            var previous = copy[w - 1].Element;
                            copy[w - 1].DoubletData.Frequency = MathHelpers.Decrement(copy[w - 1].DoubletData.Frequency);
                            copy[w - 1].DoubletData = _doubletFrequenciesCache.IncrementFrequency(previous, maxDoubletReplacementLink);
                        }
                        if (r < oldLengthMinusTwo)
                        {
                            var next = copy[r + 2].Element;
                            copy[r + 1].DoubletData.Frequency = MathHelpers.Decrement(copy[r + 1].DoubletData.Frequency);
                            copy[w].DoubletData = _doubletFrequenciesCache.IncrementFrequency(maxDoubletReplacementLink, next);
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
            _maxDoublet = new Link<TLink>();
            _maxDoubletData = new FrequencyAndLink<TLink>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateMaxDoublet(HalfDoublet[] copy, int length)
        {
            for (var i = 1; i < length; i++)
            {
                var doublet = new Link<TLink>(copy[i - 1].Element, copy[i].Element);
                UpdateMaxDoublet(ref doublet, copy[i - 1].DoubletData);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateMaxDoublet(ref Link<TLink> doublet, FrequencyAndLink<TLink> data)
        {
            var frequency = data.Frequency;
            var maxFrequency = _maxDoubletData.Frequency;

            //if (frequency > _minFrequencyToCompress && (maxFrequency < frequency || maxFrequency == frequency && doublet.Source + doublet.Target < /* gives better compression string data (and gives collisions quickly) */ _maxDoublet.Source + _maxDoublet.Target)) 
            if (MathHelpers.GreaterThan(frequency, _minFrequencyToCompress) && (MathHelpers.LessThan(maxFrequency, frequency) || MathHelpers<TLink>.IsEquals(maxFrequency, frequency) && MathHelpers.GreaterThan(MathHelpers.Add(doublet.Source, doublet.Target), MathHelpers.Add(_maxDoublet.Source, _maxDoublet.Target)))) /* gives better stability and better compression on sequent data and even on rundom numbers data (but gives collisions anyway) */
            {
                _maxDoublet = doublet;
                _maxDoubletData = data;
            }
        }
    }
}