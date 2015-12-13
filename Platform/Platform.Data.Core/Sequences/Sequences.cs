using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data.Core.Pairs;
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
    public sealed partial class Sequences : ISequences<ulong> // IList<string>, IList<ulong[]> (после завершения реализации Sequences)
    {
        /// <summary>Возвращает значение ulong, обозначающее любое количество связей.</summary>
        public const ulong ZeroOrMany = ulong.MaxValue;

        private readonly Links _links;
        public SequencesOptions<ulong> Options;
        private readonly ISyncronization _sync = new SafeSynchronization();
        private readonly Compressor _compressor;

        public Sequences(Links links)
            : this(links, new SequencesOptions<ulong>())
        {
        }

        public Sequences(Links links, SequencesOptions<ulong> options)
        {
            _links = links;
            Options = options;
            _compressor = new Compressor(links, this);

            InitOptions();
        }

        private void InitOptions()
        {
            if (Options.SequenceMarkerLink == LinksConstants.Null)
                Options.SequenceMarkerLink = _links.Create(LinksConstants.Itself, LinksConstants.Itself);
        }

        private bool IsSequence(ulong sequence)
        {
            return _links.Search(Options.SequenceMarkerLink, sequence) != LinksConstants.Null;
        }

        public ulong Count(params ulong[] sequence)
        {
            if (sequence.Length == 0)
                return _links.Count(Options.SequenceMarkerLink, LinksConstants.Any);

            if (sequence.Length == 1) // Первая связь это адрес
                return sequence[0] == LinksConstants.Null ? 0 : _links.Count(Options.SequenceMarkerLink, sequence[0]);

            throw new NotImplementedException();
        }

        public bool EachPart(Func<ulong, bool> handler, ulong sequence)
        {
            return (new Walker(this)).WalkRight(sequence, handler);
        }

        #region Create

        public ulong Create(params ulong[] sequence)
        {
            return _sync.ExecuteWriteOperation(() =>
            {
                if (sequence.IsNullOrEmpty())
                    return LinksConstants.Null;

                _links.EnsureEachLinkExists(sequence);

                ulong sequenceRoot;

                if (Options.UseCompression)
                    sequenceRoot = _compressor.Compress(sequence);
                else
                    sequenceRoot = CreateBalancedVariant(sequence);

                if (Options.EnforceSingleSequenceVersionOnWrite)
                    sequenceRoot = CompactCore(sequence);

                return _links.Create(Options.SequenceMarkerLink, sequenceRoot);
            });
        }

        public ulong CreateBalancedVariant(params ulong[] sequence)
        {
            return _sync.ExecuteWriteOperation(() =>
            {
                if (sequence.IsNullOrEmpty())
                    return LinksConstants.Null;

                _links.EnsureEachLinkExists(sequence);

                if (sequence.Length == 1)
                    return sequence[0];

                if (sequence.Length == 2)
                    return _links.CreateCore(sequence[0], sequence[1]);

                return CreateBalancedVariantCore(sequence);
            });
        }

        private ulong CreateBalancedVariantCore(params ulong[] sequence)
        {
            var length = sequence.Length;

            // Needed only if we not allowed to change sequence itself (so it makes copy)
            // Нужно только если исходный массив последовательности изменять нельзя (тогда делается его копия)
            if (length > 2)
            {
                var innerSequence = new ulong[length / 2 + length % 2];

                for (var i = 0; i < length; i += 2)
                    innerSequence[i / 2] = i + 1 == length ? sequence[i] : _links.CreateCore(sequence[i], sequence[i + 1]);

                sequence = innerSequence;
                length = innerSequence.Length;
            }

            while (length > 2)
            {
                for (var i = 0; i < length; i += 2)
                    sequence[i / 2] = i + 1 == length ? sequence[i] : _links.CreateCore(sequence[i], sequence[i + 1]);

                length = length / 2 + length % 2;
            }

            return _links.CreateCore(sequence[0], sequence[1]);
        }

        /// <remarks>
        /// bestVariant можно выбирать по максимальному числу использований,
        /// но балансированный позволяет гарантировать уникальность (если есть возможность,
        /// гарантировать его использование в других местах).
        /// 
        /// Получается этот метод должен игнорировать Options.EnforceSingleSequenceVersionOnWrite
        /// </remarks>
        public ulong Compact(params ulong[] sequence)
        {
            return _sync.ExecuteWriteOperation(() =>
            {
                if (sequence.IsNullOrEmpty())
                    return LinksConstants.Null;

                _links.EnsureEachLinkIsAnyOrExists(sequence);

                return CompactCore(sequence);
            });
        }

        private ulong CompactCore(params ulong[] sequence)
        {
            return UpdateCore(sequence, sequence);
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
            return _sync.ExecuteReadOperation(() =>
            {
                if (sequence.IsNullOrEmpty())
                    return true;

                _links.EnsureEachLinkIsAnyOrExists(sequence);

                if (sequence.Length == 1)
                {
                    var link = sequence[0];

                    if (link == LinksConstants.Any)
                        return _links.Each(LinksConstants.Any, LinksConstants.Any, handler);
                    else
                        return handler(link);
                }
                else if (sequence.Length == 2)
                {
                    return _links.Each(sequence[0], sequence[1], handler);
                }
                else
                {
                    if (_links.AnyLinkIsAny(sequence))
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        return EachCore(handler, sequence);
                    }
                }
            });
        }

        private bool EachCore(Func<ulong, bool> handler, params ulong[] sequence)
        {
            if (sequence.Length > 0)
            {
                var results = new HashSet<ulong>();
                var matcher = new Matcher(this, sequence, results, handler);

                if (sequence.Length >= 2)
                    if (!StepRight((Func<ulong, bool>)matcher.HandleFullMatchedSequence, sequence[0], sequence[1]))
                        return false;

                var last = sequence.Length - 2;
                for (var i = 1; i < last; i++)
                    if (!PartialStepRight((Func<ulong, bool>)matcher.HandleFullMatchedSequence, sequence[i], sequence[i + 1]))
                        return false;

                if (sequence.Length >= 3)
                    if (!StepLeft((Func<ulong, bool>)matcher.HandleFullMatchedSequence, sequence[sequence.Length - 2], sequence[sequence.Length - 1]))
                        return false;
            }

            return true;
        }

        private bool PartialStepRight(Func<ulong, bool> handler, ulong left, ulong right)
        {
            return _links.EachCore(0, left, pair =>
            {
                if (!StepRight(handler, pair, right))
                    return false;

                if (left != pair)
                    return PartialStepRight(handler, pair, right);

                return true;
            });
        }

        private bool StepRight(Func<ulong, bool> handler, ulong left, ulong right)
        {
            return _links.EachCore(left, 0, rightStep => TryStepRightUp(handler, right, rightStep));
        }

        private bool TryStepRightUp(Func<ulong, bool> handler, ulong right, ulong stepFrom)
        {
            var upStep = stepFrom;
            var firstSource = _links.GetTarget(upStep);
            while (firstSource != right && firstSource != upStep)
            {
                upStep = firstSource;
                firstSource = _links.GetSource(upStep);
            }

            if (firstSource == right)
                return handler(stepFrom);

            return true;
        }

        private bool StepLeft(Func<ulong, bool> handler, ulong left, ulong right)
        {
            return _links.EachCore(0, right, leftStep => TryStepLeftUp(handler, left, leftStep));
        }

        private bool TryStepLeftUp(Func<ulong, bool> handler, ulong left, ulong stepFrom)
        {
            var upStep = stepFrom;
            var firstTarget = _links.GetSource(upStep);
            while (firstTarget != left && firstTarget != upStep)
            {
                upStep = firstTarget;
                firstTarget = _links.GetTarget(upStep);
            }

            if (firstTarget == left)
                return handler(stepFrom);

            return true;
        }

        #endregion

        public ulong Update(ulong[] sequence, ulong[] newSequence)
        {
            return _sync.ExecuteWriteOperation(() =>
            {
                if (sequence.IsNullOrEmpty())
                    ; // -> Create
                else
                    _links.EnsureEachLinkIsAnyOrExists(sequence);

                if (newSequence.IsNullOrEmpty())
                    ; // -> Delete
                else
                    _links.EnsureEachLinkIsAnyOrExists(newSequence);

                return UpdateCore(sequence, newSequence);
            });
        }

        private ulong UpdateCore(ulong[] sequence, ulong[] newSequence)
        {
            var bestVariant = CreateBalancedVariantCore0(newSequence);

            foreach (var variant in Each(sequence))
                if (variant != bestVariant)
                {
                    var source = _links.GetSource(variant);
                    var target = _links.GetTarget(variant);

                    bestVariant = _links.Update(variant, bestVariant);

                    ClearGarbage(source, target);
                }

            return bestVariant;
        }

        private void ClearGarbage(ulong left, ulong right)
        {
            var leftSource = _links.GetSource(left);
            var leftTarget = _links.GetTarget(left);

            if (_links.Count(left) == 0)
            {
                _links.Delete(left);
                ClearGarbage(leftSource, leftTarget);
            }

            var rightSource = _links.GetSource(right);
            var rightTarget = _links.GetTarget(right);

            if (_links.Count(right) == 0)
            {
                _links.Delete(right);
                ClearGarbage(rightSource, rightTarget);
            }
        }

        public void Delete(params ulong[] sequence)
        {
            _sync.ExecuteWriteOperation(() =>
            {
                foreach (var linkToDelete in Each(sequence))
                {
                    _links.Delete(linkToDelete);
                }
            });
        }

        #region Walkers

        public class Walker
        {
            protected readonly Links Links;
            protected readonly Sequences Sequences;

            public Walker(Sequences sequences)
            {
                Sequences = sequences;
                Links = sequences._links;
            }

            protected virtual bool IsElement(LinkIndex link)
            {
                return Links.GetTargetCore(link) == link || Links.GetSourceCore(link) == link;
            }

            public void WalkRight(LinkIndex sequence, Action<LinkIndex> visit)
            {
                SequenceWalker.WalkRight(sequence, Links.GetSourceCore, Links.GetTargetCore, IsElement, visit);
            }

            public void WalkLeft(LinkIndex sequence, Action<LinkIndex> visit)
            {
                SequenceWalker.WalkLeft(sequence, Links.GetSourceCore, Links.GetTargetCore, IsElement, visit);
            }

            public bool WalkRight(LinkIndex sequence, Func<LinkIndex, bool> visit)
            {
                return StopableSequenceWalker.WalkRight(sequence, Links.GetSourceCore, Links.GetTargetCore, IsElement, visit);
            }

            public bool WalkLeft(LinkIndex sequence, Func<LinkIndex, bool> visit)
            {
                return StopableSequenceWalker.WalkLeft(sequence, Links.GetSourceCore, Links.GetTargetCore, IsElement, visit);
            }
        }

        public class Matcher : Walker
        {
            private readonly ulong[] _patternSequence;
            private readonly HashSet<LinkIndex> _linksInSequence;
            private readonly HashSet<LinkIndex> _results;
            private readonly Func<ulong, bool> _stopableHandler;
            private long _filterPosition;

            public Matcher(Sequences sequences, LinkIndex[] patternSequence, HashSet<LinkIndex> results, Func<LinkIndex, bool> stopableHandler)
                : base(sequences)
            {
                _patternSequence = patternSequence;
                _linksInSequence = new HashSet<LinkIndex>(patternSequence.Where(x => x != LinksConstants.Null && x != ZeroOrMany));
                _results = results;
                _stopableHandler = stopableHandler;
            }

            protected override bool IsElement(ulong link)
            {
                return _linksInSequence.Contains(link) || Links.GetTargetCore(link) == link || Links.GetSourceCore(link) == link;
            }

            public bool FullMatch(LinkIndex sequenceToMatch)
            {
                _filterPosition = 0;

                WalkRight(sequenceToMatch, (Func<ulong, bool>)FullMatchCore);

                return _filterPosition == _patternSequence.Length;
            }

            private bool FullMatchCore(LinkIndex element)
            {
                if (_filterPosition == _patternSequence.Length)
                {
                    _filterPosition = -2; // Длиннее чем нужно
                    return false;
                }

                if (element != _patternSequence[_filterPosition])
                {
                    _filterPosition = -1;
                    return false; // Начинается иначе
                }

                _filterPosition++;

                return true;
            }

            public void AddFullMatchedToResults(ulong sequenceToMatch)
            {
                if (FullMatch(sequenceToMatch))
                    _results.Add(sequenceToMatch);
            }

            public bool HandleFullMatchedSequence(ulong sequenceToMatch)
            {
                if (FullMatch(sequenceToMatch) && Sequences.IsSequence(sequenceToMatch))
                    return _stopableHandler(sequenceToMatch);
                return true;
            }

            public bool PartialMatch(LinkIndex sequenceToMatch)
            {
                _filterPosition = -1;

                WalkRight(sequenceToMatch, (Func<ulong, bool>)PartialMatchCore);

                return _filterPosition == _patternSequence.Length - 1;
            }

            private bool PartialMatchCore(LinkIndex element)
            {
                if (_filterPosition == (_patternSequence.Length - 1))
                    return false;

                if (_filterPosition >= 0)
                {
                    if (element == _patternSequence[_filterPosition + 1])
                        _filterPosition++;
                    else
                        return false;
                }

                if (_filterPosition < 0)
                {
                    if (element == _patternSequence[0])
                        _filterPosition = 0;
                }

                return true;
            }

            public void AddPartialMatchedToResults(ulong sequenceToMatch)
            {
                if (PartialMatch(sequenceToMatch))
                    _results.Add(sequenceToMatch);
            }

            public void AddAllPartialMatchedToResults(IEnumerable<ulong> sequencesToMatch)
            {
                foreach (var sequenceToMatch in sequencesToMatch)
                    if (PartialMatch(sequenceToMatch))
                        _results.Add(sequenceToMatch);
            }
        }

        #endregion
    }
}