using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Platform.Data.Core.Doublets;
using Platform.Helpers;
using Platform.Helpers.Collections;
using Platform.Helpers.Threading;
using LinkIndex = System.UInt64;

namespace Platform.Data.Core.Sequences
{
    /// <summary>
    /// Представляет коллекцию последовательностей связей.
    /// </summary>
    /// <remarks>
    /// Обязательно реализовать атомарность каждого публичного метода.
    /// 
    /// TODO:
    /// 
    /// !!! Повышение вероятности повторного использования групп (подпоследовательностей),
    /// через естественную группировку по unicode типам, все whitespace вместе, все символы вместе, все числа вместе и т.п.
    /// + использовать ровно сбалансированный вариант, чтобы уменьшать вложенность (глубину графа)
    /// 
    /// x*y - найти все связи между, в последовательностях любой формы, если не стоит ограничитель на то, что является последовательностью, а что нет,
    /// то находятся любые структуры связей, которые содержат эти элементы именно в таком порядке.
    /// 
    /// Рост последовательности слева и справа.
    /// Поиск со звёздочкой.
    /// URL, PURL - реестр используемых во вне ссылок на ресурсы,
    /// так же проблема может быть решена при реализации дистанционных триггеров.
    /// Нужны ли уникальные указатели вообще?
    /// Что если обращение к информации будет происходить через содержимое всегда?
    /// 
    /// Писать тесты.
    /// 
    /// 
    /// Можно убрать зависимость от конкретной реализации Links,
    /// на зависимость от абстрактного элемента, который может быть представлен несколькими способами.
    /// 
    /// Можно ли как-то сделать один общий интерфейс 
    /// 
    /// 
    /// Блокчейн и/или гит для распределённой записи транзакций.
    /// 
    /// </remarks>
    public partial class Sequences : ISequences<ulong> // IList<string>, IList<ulong[]> (после завершения реализации Sequences)
    {
        private static readonly LinksConstants<bool, ulong, long> Constants = Default<LinksConstants<bool, ulong, long>>.Instance;

        /// <summary>Возвращает значение ulong, обозначающее любое количество связей.</summary>
        public const ulong ZeroOrMany = ulong.MaxValue;

        public SequencesOptions<ulong> Options;
        public readonly SynchronizedLinks<ulong> Links;
        public readonly ISynchronization Sync;

        public Sequences(SynchronizedLinks<ulong> links)
            : this(links, new SequencesOptions<ulong>())
        {
        }

        public Sequences(SynchronizedLinks<ulong> links, SequencesOptions<ulong> options)
        {
            Links = links;
            Sync = links.SyncRoot;
            Options = options;

            Options.ValidateOptions();
            Options.InitOptions(Links);
        }

        public bool IsSequence(ulong sequence)
        {
            return Sync.ExecuteReadOperation(() =>
            {
                if (Options.UseSequenceMarker)
                    return Options.MarkedSequenceMatcher.IsMatched(sequence);

                return !Links.Unsync.IsPartialPoint(sequence);
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ulong GetSequenceByElements(ulong sequence)
        {
            if (Options.UseSequenceMarker)
                return Links.SearchOrDefault(Options.SequenceMarkerLink, sequence);

            return sequence;
        }

        private ulong GetSequenceElements(ulong sequence)
        {
            if (Options.UseSequenceMarker)
            {
                var linkContents = new UInt64Link(Links.GetLink(sequence));

                if (linkContents.Source == Options.SequenceMarkerLink)
                    return linkContents.Target;
                if (linkContents.Target == Options.SequenceMarkerLink)
                    return linkContents.Source;
            }

            return sequence;
        }

        #region Count

        public ulong Count(params ulong[] sequence)
        {
            if (sequence.Length == 0)
                return Links.Count(Constants.Any, Options.SequenceMarkerLink, Constants.Any);

            if (sequence.Length == 1) // Первая связь это адрес
            {
                if (sequence[0] == Constants.Null)
                    return 0;

                if (sequence[0] == Constants.Any)
                    return Count();

                if (Options.UseSequenceMarker)
                    return Links.Count(Constants.Any, Options.SequenceMarkerLink, sequence[0]);

                return Links.Exists(sequence[0]) ? 1UL : 0;
            }

            throw new NotImplementedException();
        }

        private ulong CountReferences(params ulong[] restrictions)
        {
            if (restrictions.Length == 0)
                return 0;

            if (restrictions.Length == 1) // Первая связь это адрес
            {
                if (restrictions[0] == Constants.Null)
                    return 0;

                if (Options.UseSequenceMarker)
                {
                    var elementsLink = GetSequenceElements(restrictions[0]);
                    var sequenceLink = GetSequenceByElements(elementsLink);
                    if (sequenceLink != Constants.Null)
                        return Links.Count(sequenceLink) + Links.Count(elementsLink) - 1;
                    return Links.Count(elementsLink);
                }

                return Links.Count(restrictions[0]);
            }

            throw new NotImplementedException();
        }

        #endregion

        #region Create

        public ulong Create(params ulong[] sequence)
        {
            return Sync.ExecuteWriteOperation(() =>
            {
                if (sequence.IsNullOrEmpty())
                    return Constants.Null;

                Links.EnsureEachLinkExists(sequence);

                return CreateCore(sequence);
            });
        }

        private ulong CreateCore(params ulong[] sequence)
        {
            if (Options.UseIndex)
                Options.Indexer.Index(sequence);

            var sequenceRoot = default(ulong);

            if (Options.EnforceSingleSequenceVersionOnWriteBasedOnExisting)
            {
                var matches = Each(sequence);
                if (matches.Count > 0)
                    sequenceRoot = matches[0];
            }
            else if (Options.EnforceSingleSequenceVersionOnWriteBasedOnNew)
                return CompactCore(sequence);

            if (sequenceRoot == default(LinkIndex))
                sequenceRoot = Options.LinksToSequenceConverter.Convert(sequence);

            if (Options.UseSequenceMarker)
                Links.Unsync.CreateAndUpdate(Options.SequenceMarkerLink, sequenceRoot);

            return sequenceRoot; // Возвращаем корень последовательности (т.е. сами элементы)
        }



        #endregion

        #region Each

        public List<ulong> Each(params ulong[] sequence)
        {
            var results = new List<ulong>();
            Each(results.AddAndReturnTrue, sequence);
            return results;
        }

        public bool Each(Func<ulong, bool> handler, params ulong[] sequence)
        {
            return Sync.ExecuteReadOperation(() =>
            {
                if (sequence.IsNullOrEmpty())
                    return true;

                Links.EnsureEachLinkIsAnyOrExists(sequence);

                if (sequence.Length == 1)
                {
                    var link = sequence[0];

                    if (link == Constants.Any)
                        return Links.Unsync.Each(Constants.Any, Constants.Any, handler);

                    return handler(link);
                }
                if (sequence.Length == 2)
                    return Links.Unsync.Each(sequence[0], sequence[1], handler);

                if (Options.UseIndex && !Options.Indexer.CheckIndex(sequence))
                    return false;

                return EachCore(handler, sequence);
            });
        }

        private bool EachCore(Func<ulong, bool> handler, params ulong[] sequence)
        {
            var matcher = new Matcher(this, sequence, new HashSet<LinkIndex>(), handler);

            // TODO: Find out why matcher.HandleFullMatched executed twice for the same sequence Id.

            Func<ulong, bool> innerHandler = Options.UseSequenceMarker ? (Func<ulong, bool>)matcher.HandleFullMatchedSequence : matcher.HandleFullMatched;

            //if (sequence.Length >= 2)
            if (!StepRight(innerHandler, sequence[0], sequence[1]))
                return false;

            var last = sequence.Length - 2;
            for (var i = 1; i < last; i++)
                if (!PartialStepRight(innerHandler, sequence[i], sequence[i + 1]))
                    return false;

            if (sequence.Length >= 3)
                if (!StepLeft(innerHandler, sequence[sequence.Length - 2], sequence[sequence.Length - 1]))
                    return false;

            return true;
        }

        private bool PartialStepRight(Func<ulong, bool> handler, ulong left, ulong right)
        {
            return Links.Unsync.Each(Constants.Any, left, doublet =>
            {
                if (!StepRight(handler, doublet, right))
                    return false;

                if (left != doublet)
                    return PartialStepRight(handler, doublet, right);

                return true;
            });
        }

        private bool StepRight(Func<ulong, bool> handler, ulong left, ulong right)
        {
            return Links.Unsync.Each(left, Constants.Any, rightStep => TryStepRightUp(handler, right, rightStep));
        }

        private bool TryStepRightUp(Func<ulong, bool> handler, ulong right, ulong stepFrom)
        {
            var upStep = stepFrom;
            var firstSource = Links.Unsync.GetTarget(upStep);
            while (firstSource != right && firstSource != upStep)
            {
                upStep = firstSource;
                firstSource = Links.Unsync.GetSource(upStep);
            }

            if (firstSource == right)
                return handler(stepFrom);

            return true;
        }

        private bool StepLeft(Func<ulong, bool> handler, ulong left, ulong right)
        {
            return Links.Unsync.Each(Constants.Any, right, leftStep => TryStepLeftUp(handler, left, leftStep));
        }

        private bool TryStepLeftUp(Func<ulong, bool> handler, ulong left, ulong stepFrom)
        {
            var upStep = stepFrom;
            var firstTarget = Links.Unsync.GetSource(upStep);
            while (firstTarget != left && firstTarget != upStep)
            {
                upStep = firstTarget;
                firstTarget = Links.Unsync.GetTarget(upStep);
            }

            if (firstTarget == left)
                return handler(stepFrom);

            return true;
        }

        #endregion

        #region Update

        public ulong Update(ulong[] sequence, ulong[] newSequence)
        {
            if (sequence.IsNullOrEmpty() && newSequence.IsNullOrEmpty())
                return Constants.Null;

            if (sequence.IsNullOrEmpty())
                return Create(newSequence);

            if (newSequence.IsNullOrEmpty())
            {
                Delete(sequence);
                return Constants.Null;
            }

            return Sync.ExecuteWriteOperation(() =>
            {
                Links.EnsureEachLinkIsAnyOrExists(sequence);
                Links.EnsureEachLinkExists(newSequence);

                return UpdateCore(sequence, newSequence);
            });
        }

        private ulong UpdateCore(ulong[] sequence, ulong[] newSequence)
        {
            ulong bestVariant;
            if (Options.EnforceSingleSequenceVersionOnWriteBasedOnNew && !sequence.EqualTo(newSequence))
                bestVariant = CompactCore(newSequence);
            else
                bestVariant = CreateCore(newSequence);

            // TODO: Check all options only ones before loop execution
            // Возможно нужно две версии Each, возвращающий фактические последовательности и с маркером,
            // или возможно даже возвращать и тот и тот вариант. С другой стороны все варианты можно получить имея только фактические последовательности.
            foreach (var variant in Each(sequence))
                if (variant != bestVariant)
                    UpdateOneCore(variant, bestVariant);

            return bestVariant;
        }

        private void UpdateOneCore(ulong sequence, ulong newSequence)
        {
            if (Options.UseGarbageCollection)
            {
                var sequenceElements = GetSequenceElements(sequence);
                var sequenceElementsContents = new UInt64Link(Links.GetLink(sequenceElements));
                var sequenceLink = GetSequenceByElements(sequenceElements);

                var newSequenceElements = GetSequenceElements(newSequence);
                var newSequenceLink = GetSequenceByElements(newSequenceElements);

                if (Options.UseCascadeUpdate || CountReferences(sequence) == 0)
                {
                    if (sequenceLink != Constants.Null)
                        Links.Unsync.Merge(sequenceLink, newSequenceLink);
                    Links.Unsync.Merge(sequenceElements, newSequenceElements);
                }

                ClearGarbage(sequenceElementsContents.Source);
                ClearGarbage(sequenceElementsContents.Target);
            }
            else
            {
                if (Options.UseSequenceMarker)
                {
                    var sequenceElements = GetSequenceElements(sequence);
                    var sequenceLink = GetSequenceByElements(sequenceElements);

                    var newSequenceElements = GetSequenceElements(newSequence);
                    var newSequenceLink = GetSequenceByElements(newSequenceElements);

                    if (Options.UseCascadeUpdate || CountReferences(sequence) == 0)
                    {
                        if (sequenceLink != Constants.Null)
                            Links.Unsync.Merge(sequenceLink, newSequenceLink);
                        Links.Unsync.Merge(sequenceElements, newSequenceElements);
                    }
                }
                else
                {
                    if (Options.UseCascadeUpdate || CountReferences(sequence) == 0)
                        Links.Unsync.Merge(sequence, newSequence);
                }
            }
        }

        #endregion

        #region Delete

        public void Delete(params ulong[] sequence)
        {
            Sync.ExecuteWriteOperation(() =>
            {
                // TODO: Check all options only ones before loop execution
                foreach (var linkToDelete in Each(sequence))
                    DeleteOneCore(linkToDelete);
            });
        }

        private void DeleteOneCore(ulong link)
        {
            if (Options.UseGarbageCollection)
            {
                var sequenceElements = GetSequenceElements(link);
                var sequenceElementsContents = new UInt64Link(Links.GetLink(sequenceElements));
                var sequenceLink = GetSequenceByElements(sequenceElements);

                if (Options.UseCascadeDelete || CountReferences(link) == 0)
                {
                    if (sequenceLink != Constants.Null)
                        Links.Unsync.Delete(sequenceLink);
                    Links.Unsync.Delete(link);
                }

                ClearGarbage(sequenceElementsContents.Source);
                ClearGarbage(sequenceElementsContents.Target);
            }
            else
            {
                if (Options.UseSequenceMarker)
                {
                    var sequenceElements = GetSequenceElements(link);
                    var sequenceLink = GetSequenceByElements(sequenceElements);

                    if (Options.UseCascadeDelete || CountReferences(link) == 0)
                    {
                        if (sequenceLink != Constants.Null)
                            Links.Unsync.Delete(sequenceLink);
                        Links.Unsync.Delete(link);
                    }
                }
                else
                {
                    if (Options.UseCascadeDelete || CountReferences(link) == 0)
                        Links.Unsync.Delete(link);
                }
            }
        }

        #endregion

        #region Compactification

        /// <remarks>
        /// bestVariant можно выбирать по максимальному числу использований,
        /// но балансированный позволяет гарантировать уникальность (если есть возможность,
        /// гарантировать его использование в других местах).
        /// 
        /// Получается этот метод должен игнорировать Options.EnforceSingleSequenceVersionOnWrite
        /// </remarks>
        public ulong Compact(params ulong[] sequence)
        {
            return Sync.ExecuteWriteOperation(() =>
            {
                if (sequence.IsNullOrEmpty())
                    return Constants.Null;

                Links.EnsureEachLinkExists(sequence);

                return CompactCore(sequence);
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ulong CompactCore(params ulong[] sequence)
        {
            return UpdateCore(sequence, sequence);
        }

        #endregion

        #region Garbage Collection

        /// <remarks>
        /// TODO: Добавить дополнительный обработчик / событие CanBeDeleted которое можно определить извне или в унаследованном классе
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsGarbage(ulong link)
        {
            return link != Options.SequenceMarkerLink
                && !Links.Unsync.IsPartialPoint(link)
                && Links.Count(link) == 0;
        }

        private void ClearGarbage(ulong link)
        {
            if (IsGarbage(link))
            {
                var contents = new UInt64Link(Links.GetLink(link));
                Links.Unsync.Delete(link);
                ClearGarbage(contents.Source);
                ClearGarbage(contents.Target);
            }
        }

        #endregion

        #region Walkers

        public bool EachPart(Func<ulong, bool> handler, ulong sequence)
        {
            return (new Walker(this)).WalkRight(sequence, handler);
        }

        public class Walker
        {
            protected readonly SynchronizedLinks<ulong> Links;
            protected readonly Sequences Sequences;

            public Walker(Sequences sequences)
            {
                Sequences = sequences;
                Links = sequences.Links;
            }

            protected virtual bool IsElement(LinkIndex link)
            {
                return Links.Unsync.GetTarget(link) == link || Links.Unsync.GetSource(link) == link;
            }

            public void WalkRight(LinkIndex sequence, Action<LinkIndex> visit)
            {
                SequenceWalker.WalkRight(sequence, Links.Unsync.GetSource, Links.Unsync.GetTarget, IsElement, visit);
            }

            public void WalkLeft(LinkIndex sequence, Action<LinkIndex> visit)
            {
                SequenceWalker.WalkLeft(sequence, Links.Unsync.GetSource, Links.Unsync.GetTarget, IsElement, visit);
            }

            public bool WalkRight(LinkIndex sequence, Func<LinkIndex, bool> visit)
            {
                return StopableSequenceWalker.WalkRight(sequence, Links.Unsync.GetSource, Links.Unsync.GetTarget, IsElement, visit);
            }

            public bool WalkLeft(LinkIndex sequence, Func<LinkIndex, bool> visit)
            {
                return StopableSequenceWalker.WalkLeft(sequence, Links.Unsync.GetSource, Links.Unsync.GetTarget, IsElement, visit);
            }
        }

        public class Matcher : Walker
        {
            private readonly ulong[] _patternSequence;
            private readonly HashSet<LinkIndex> _linksInSequence;
            private readonly HashSet<LinkIndex> _results;
            private readonly Func<ulong, bool> _stopableHandler;
            private readonly HashSet<ulong> _readAsElements;
            private long _filterPosition;

            public Matcher(Sequences sequences, LinkIndex[] patternSequence, HashSet<LinkIndex> results, Func<LinkIndex, bool> stopableHandler, HashSet<LinkIndex> readAsElements = null)
                : base(sequences)
            {
                _patternSequence = patternSequence;
                _linksInSequence = new HashSet<LinkIndex>(patternSequence.Where(x => x != Constants.Any && x != ZeroOrMany));
                _results = results;
                _stopableHandler = stopableHandler;
                _readAsElements = readAsElements;
            }

            protected override bool IsElement(ulong link)
            {
                return Links.Unsync.IsPartialPoint(link) || (_readAsElements != null && _readAsElements.Contains(link)) || _linksInSequence.Contains(link);
            }

            public bool FullMatch(LinkIndex sequenceToMatch)
            {
                _filterPosition = 0;

                WalkRight(sequenceToMatch, FullMatchCore);

                return _filterPosition == _patternSequence.Length;
            }

            private bool FullMatchCore(LinkIndex element)
            {
                if (_filterPosition == _patternSequence.Length)
                {
                    _filterPosition = -2; // Длиннее чем нужно
                    return false;
                }

                if (_patternSequence[_filterPosition] != Constants.Any &&
                    element != _patternSequence[_filterPosition])
                {
                    _filterPosition = -1;
                    return false; // Начинается/Продолжается иначе
                }

                _filterPosition++;
                return true;
            }

            public void AddFullMatchedToResults(ulong sequenceToMatch)
            {
                if (FullMatch(sequenceToMatch))
                    _results.Add(sequenceToMatch);
            }

            public bool HandleFullMatched(ulong sequenceToMatch)
            {
                if (FullMatch(sequenceToMatch) && _results.Add(sequenceToMatch))
                    return _stopableHandler(sequenceToMatch);
                return true;
            }

            public bool HandleFullMatchedSequence(ulong sequenceToMatch)
            {
                var sequence = Sequences.GetSequenceByElements(sequenceToMatch);
                if (sequence != Constants.Null && FullMatch(sequenceToMatch) && _results.Add(sequenceToMatch))
                    return _stopableHandler(sequence);
                return true;
            }

            /// <remarks>
            /// TODO: Add support for LinksConstants.Any
            /// </remarks>
            public bool PartialMatch(LinkIndex sequenceToMatch)
            {
                _filterPosition = -1;

                WalkRight(sequenceToMatch, PartialMatchCore);

                return _filterPosition == _patternSequence.Length - 1;
            }

            private bool PartialMatchCore(LinkIndex element)
            {
                if (_filterPosition == (_patternSequence.Length - 1))
                    return false; // Нашлось

                if (_filterPosition >= 0)
                {
                    if (element == _patternSequence[_filterPosition + 1])
                        _filterPosition++;
                    else
                        _filterPosition = -1;
                }

                if (_filterPosition < 0)
                {
                    if (element == _patternSequence[0])
                        _filterPosition = 0;
                }

                return true; // Ищем дальше
            }

            public void AddPartialMatchedToResults(ulong sequenceToMatch)
            {
                if (PartialMatch(sequenceToMatch))
                    _results.Add(sequenceToMatch);
            }

            public bool HandlePartialMatched(ulong sequenceToMatch)
            {
                if (PartialMatch(sequenceToMatch))
                    return _stopableHandler(sequenceToMatch);
                return true;
            }

            public void AddAllPartialMatchedToResults(IEnumerable<ulong> sequencesToMatch)
            {
                foreach (var sequenceToMatch in sequencesToMatch)
                    if (PartialMatch(sequenceToMatch))
                        _results.Add(sequenceToMatch);
            }

            public void AddAllPartialMatchedToResultsAndReadAsElements(IEnumerable<ulong> sequencesToMatch)
            {
                foreach (var sequenceToMatch in sequencesToMatch)
                    if (PartialMatch(sequenceToMatch))
                    {
                        _readAsElements.Add(sequenceToMatch);
                        _results.Add(sequenceToMatch);
                    }
            }
        }

        #endregion
    }
}