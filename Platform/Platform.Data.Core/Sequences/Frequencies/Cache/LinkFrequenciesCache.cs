using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Platform.Helpers;
using Platform.Data.Core.Doublets;

namespace Platform.Data.Core.Sequences
{
    /// <remarks>
    /// Can be used to operate with many CompressingConverters (to keep global frequencies data between them).
    /// TODO: Extract interface to implement frequencies storage inside Links storage
    /// </remarks>
    public class DoubletFrequenciesCache<TLink> : LinksOperatorBase<TLink>
    {
        private readonly Dictionary<Link<TLink>, FrequencyAndLink<TLink>> _doubletsCache;
        private readonly ICounter<TLink, TLink> _frequencyCounter;

        /// <remarks>
        /// TODO: Может стоит попробовать ref во всех методах (IRefEqualityComparer)
        /// 2x faster with comparer 
        /// </remarks>
        private class DoubletComparer : IEqualityComparer<Link<TLink>>
        {
            public static readonly DoubletComparer Default = new DoubletComparer();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Equals(Link<TLink> x, Link<TLink> y) => MathHelpers<TLink>.IsEquals(x.Source, y.Source) && MathHelpers<TLink>.IsEquals(x.Target, y.Target);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int GetHashCode(Link<TLink> obj) => (int)(((ulong)(Integer<TLink>)obj.Source << 15) ^ (ulong)(Integer<TLink>)obj.Target);
        }

        public DoubletFrequenciesCache(ILinks<TLink> links, ICounter<TLink, TLink> frequencyCounter)
            : base(links)
        {
            _doubletsCache = new Dictionary<Link<TLink>, FrequencyAndLink<TLink>>(4096, DoubletComparer.Default);
            _frequencyCounter = frequencyCounter;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FrequencyAndLink<TLink> IncrementFrequency(TLink source, TLink target)
        {
            var doublet = new Link<TLink>(source, target);

            return IncrementFrequency(ref doublet);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FrequencyAndLink<TLink> IncrementFrequency(ref Link<TLink> doublet)
        {
            if (_doubletsCache.TryGetValue(doublet, out FrequencyAndLink<TLink> data))
                data.Frequency = MathHelpers.Increment(data.Frequency);
            else
            {
                var link = Links.SearchOrDefault(doublet.Source, doublet.Target);

                data = new FrequencyAndLink<TLink>(Integer<TLink>.One, link);

                if (!MathHelpers<TLink>.IsEquals(link, default))
                    data.Frequency = MathHelpers.Add(data.Frequency, _frequencyCounter.Count(link));

                _doubletsCache.Add(doublet, data);
            }
            return data;
        }

        public void ResetFrequencies() => _doubletsCache.Clear();

        public void ValidateFrequencies()
        {
            foreach (var entry in _doubletsCache)
            {
                var value = entry.Value;

                var linkIndex = value.Link;

                if (!MathHelpers<TLink>.IsEquals(linkIndex, default))
                {
                    var frequency = value.Frequency;
                    var count = _frequencyCounter.Count(linkIndex);

                    // TODO: Why `frequency` always greater than `count` by 1?
                    if ((MathHelpers.GreaterThan(frequency, count) && MathHelpers.GreaterThan(MathHelpers.Subtract(frequency, count), Integer<TLink>.One))
                     || (MathHelpers.GreaterThan(count, frequency) && MathHelpers.GreaterThan(MathHelpers.Subtract(count, frequency), Integer<TLink>.One)))
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
    }
}
