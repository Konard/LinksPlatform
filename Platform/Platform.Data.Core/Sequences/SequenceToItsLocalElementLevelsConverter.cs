using System;
using System.Collections.Generic;
using Platform.Helpers;
using Platform.Data.Core.Doublets;

namespace Platform.Data.Core.Sequences
{
    public class SequenceToItsLocalElementLevelsConverter<TLink> : LinksOperatorBase<TLink>, IConverter<IList<TLink>>
    {
        private readonly IIncrementer<TLink> _linkFrequencyIncrementer;
        private readonly IConverter<TLink> _linkToItsFrequencyToNumberConveter;

        public SequenceToItsLocalElementLevelsConverter(
            ILinks<TLink> links,
            IIncrementer<TLink> linkFrequencyIncrementer,
            IConverter<TLink> linkToItsFrequencyToNumberConveter)
            : base(links)
        {
            _linkFrequencyIncrementer = linkFrequencyIncrementer;
            _linkToItsFrequencyToNumberConveter = linkToItsFrequencyToNumberConveter;
        }

        public IList<TLink> Convert(IList<TLink> sequence)
        {
            var levels = new TLink[sequence.Count];
            levels[0] = GetFrequencyNumber(sequence[0], sequence[1]);

            for (var i = 1; i < sequence.Count - 1; i++)
            {
                var previous = GetFrequencyNumber(sequence[i - 1], sequence[i]);
                var next = GetFrequencyNumber(sequence[i], sequence[i + 1]);
                levels[i] = MathHelpers.GreaterThan(previous, next) ? previous : next;
            }

            levels[levels.Length - 1] = GetFrequencyNumber(sequence[sequence.Count - 2], sequence[sequence.Count - 1]);
            return levels;
        }

        // TODO: Find a better place for these methods:

        public void IncrementDoubletsFrequencies(TLink[] sequence)
        {
            for (var i = 1; i < sequence.Length; i++)
                IncrementFrequency(sequence[i - 1], sequence[i]);
        }

        public void PrintDoubletsFrequencies(TLink[] sequence)
        {
            for (var i = 1; i < sequence.Length; i++)
                PrintFrequency(sequence[i - 1], sequence[i]);
        }

        public void IncrementFrequency(TLink source, TLink target)
        {
            var link = Links.GetOrCreate(source, target);
            _linkFrequencyIncrementer.Increment(link);
        }

        public void PrintFrequency(TLink source, TLink target)
        {
            var number = GetFrequencyNumber(source, target);
            Console.WriteLine("({0},{1}) - {2}", source, target, number);
        }

        public TLink GetFrequencyNumber(TLink source, TLink target)
        {
            var link = Links.GetOrCreate(source, target);
            return _linkToItsFrequencyToNumberConveter.Convert(link);
        }
    }
}
