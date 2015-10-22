using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.Links.DataBase.CoreUnsafe.Exceptions;
using Platform.Helpers;
using Platform.Helpers.Synchronization;

using LinkIndex = System.UInt64;

namespace Platform.Links.DataBase.CoreUnsafe.Sequences
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
    public sealed partial class Sequences // IList<string>, IList<ulong[]> (после завершения реализации Sequences)
    {
        /// <summary>Возвращает значение ulong, обозначающее любую одну связь.</summary>
        public const ulong Any = Pairs.Links.Any;

        /// <summary>Возвращает значение ulong, обозначающее любое количество связей.</summary>
        public const ulong ZeroOrMany = ulong.MaxValue;

        private readonly Pairs.Links _links;
        private readonly ISyncronization _sync = new SafeSynchronization();

        public Sequences(Pairs.Links links)
        {
            _links = links;
        }

        public ulong Create(params ulong[] sequence)
        {
            //return Compact(sequence);
            return CreateBalancedVariant(sequence);
        }

        #region Create All Variants (Not Practical)

        /// <remarks>
        /// Number of links that is needed to generate all variants for
        /// sequence of length N corresponds to https://oeis.org/A014143/list sequence.
        /// </remarks>
        public ulong[] CreateAllVariants2(ulong[] sequence)
        {
            return _sync.ExecuteWriteOperation(() =>
            {
                if (sequence == null || sequence.Length == 0)
                    return new ulong[0];

                EnsureEachLinkExists(_links, sequence);

                if (sequence.Length == 1)
                    return sequence;

                return CreateAllVariants2Core(sequence, 0, sequence.Length - 1);
            });
        }

        private ulong[] CreateAllVariants2Core(ulong[] sequence, long startAt, long stopAt)
        {
#if DEBUG
            if ((stopAt - startAt) < 0)
                throw new ArgumentOutOfRangeException("startAt", "startAt должен быть меньше или равен stopAt");
#endif
            if ((stopAt - startAt) == 0)
                return new[] { sequence[startAt] };
            if ((stopAt - startAt) == 1)
                return new[] { _links.Create(sequence[startAt], sequence[stopAt]) };

            var variants = new ulong[(ulong)MathHelpers.Catalan(stopAt - startAt)];
            var last = 0;

            for (var splitter = startAt; splitter < stopAt; splitter++)
            {
                var left = CreateAllVariants2Core(sequence, startAt, splitter);
                var right = CreateAllVariants2Core(sequence, splitter + 1, stopAt);

                for (var i = 0; i < left.Length; i++)
                {
                    for (var j = 0; j < right.Length; j++)
                    {
                        var variant = _links.Create(left[i], right[j]);
                        if (variant == Pairs.Links.Null)
                            throw new NotImplementedException("Creation cancellation is not implemented.");
                        variants[last++] = variant;
                    }
                }
            }

            return variants;
        }

        public List<ulong> CreateAllVariants1(params ulong[] sequence)
        {
            return _sync.ExecuteWriteOperation(() =>
            {
                if (sequence == null || sequence.Length == 0)
                    return new List<ulong>();

                EnsureEachLinkExists(_links, sequence);

                if (sequence.Length == 1)
                    return new List<ulong>() { sequence[0] };

                var results = new List<ulong>((int)MathHelpers.Catalan(sequence.Length));
                return CreateAllVariants1Core(sequence, results);
            });
        }

        private List<ulong> CreateAllVariants1Core(ulong[] sequence, List<ulong> results)
        {
            if (sequence.Length == 2)
            {
                var link = _links.Create(sequence[0], sequence[1]);
                if (link == Pairs.Links.Null)
                    throw new NotImplementedException("Creation cancellation is not implemented.");
                results.Add(link);
                return results;
            }

            var innerSequenceLength = sequence.Length - 1;
            var innerSequence = new ulong[innerSequenceLength];

            for (var li = 0; li < innerSequenceLength; li++)
            {
                var link = _links.Create(sequence[li], sequence[li + 1]);
                if (link == Pairs.Links.Null)
                    throw new NotImplementedException("Creation cancellation is not implemented.");

                for (var isi = 0; isi < li; isi++)
                    innerSequence[isi] = sequence[isi];
                innerSequence[li] = link;
                for (var isi = li + 1; isi < innerSequenceLength; isi++)
                    innerSequence[isi] = sequence[isi + 1];

                CreateAllVariants1Core(innerSequence, results);
            }

            return results;
        }

        #endregion

        public ulong CreateBalancedVariant(params ulong[] sequence)
        {
            return _sync.ExecuteWriteOperation(() =>
            {
                if (sequence == null || sequence.Length == 0)
                    return Pairs.Links.Null;

                EnsureEachLinkExists(_links, sequence);

                if (sequence.Length == 1)
                    return sequence[0];

                return CreateBalancedVariantCore1(sequence);
            });
        }

        private ulong CreateBalancedVariantCore0(params ulong[] sequence)
        {
            do
            {
                if (sequence.Length == 2)
                    return _links.Create(sequence[0], sequence[1]);

                var innerSequence = new ulong[sequence.Length / 2 + sequence.Length % 2];

                for (var i = 0; i < sequence.Length; i += 2)
                    innerSequence[i / 2] = i + 1 == sequence.Length ? sequence[i] : _links.Create(sequence[i], sequence[i + 1]);

                sequence = innerSequence;
            } while (true);
        }

        private ulong CreateBalancedVariantCore1(params ulong[] sequence)
        {
            var length = sequence.Length;

            // Needed only if we not allowed to change sequence itself (so it makes copy)
            // Нужно только если исходный массив последовательности изменять нельзя (тогда делается его копия)
            if (length > 2)
            {
                var innerSequence = new ulong[length / 2 + length % 2];

                for (var i = 0; i < length; i += 2)
                    innerSequence[i / 2] = i + 1 == length ? sequence[i] : _links.Create(sequence[i], sequence[i + 1]);

                sequence = innerSequence;
                length = innerSequence.Length;
            }

            while (length > 2)
            {
                for (var i = 0; i < length; i += 2)
                    sequence[i / 2] = i + 1 == length ? sequence[i] : _links.Create(sequence[i], sequence[i + 1]);

                length = length / 2 + length % 2;
            }

            return _links.Create(sequence[0], sequence[1]);
        }

        /// <remarks>
        /// bestVariant можно выбирать по максимальному числу использований,
        /// но балансированный позволяет гарантировать уникальность (если есть возможность,
        /// гарантировать его использование в других местах).
        /// </remarks>
        public ulong Compact(params ulong[] sequence)
        {
            return _sync.ExecuteWriteOperation(() =>
            {
                if (sequence == null || sequence.Length == 0)
                    return Pairs.Links.Null;

                EnsureEachLinkIsAnyOrExists(_links, sequence);

                return UpdateCore(sequence, sequence);
            });
        }

        public HashSet<ulong> Each(params ulong[] sequence)
        {
            var visitedLinks = new HashSet<ulong>(); // Заменить на bitstring

            EachCore(link =>
            {
                if (!visitedLinks.Contains(link)) visitedLinks.Add(link); // изучить почему случаются повторы
                return true;
            }, sequence);

            return visitedLinks;
        }

        public void Each(Func<ulong, bool> handler, params ulong[] sequence)
        {
            if (sequence == null || sequence.Length == 0)
                return;

            EnsureEachLinkIsAnyOrExists(_links, sequence);

            if (sequence.Length == 1)
            {
                var link = sequence[0];

                if (link > 0)
                    handler(link);
                else
                    _links.Each(0, 0, handler);
            }
            else
            {
                var visitedLinks = new HashSet<ulong>(); // Заменить на bitstring
                EachCore(link =>
                {
                    if (!visitedLinks.Contains(link))
                    {
                        visitedLinks.Add(link); // изучить почему случаются повторы
                        return handler(link);
                    }
                    return true;
                }, sequence);
            }
        }

        private void EachCore(Func<ulong, bool> handler, params ulong[] sequence)
        {
            if (sequence.Length == 2)
            {
                _links.Each(sequence[0], sequence[1], handler);
            }
            else
            {
                var innerSequenceLength = sequence.Length - 1;
                for (var li = 0; li < innerSequenceLength; li++)
                {
                    var left = sequence[li];
                    var right = sequence[li + 1];

                    if (left == 0 && right == 0)
                        continue;

                    var linkIndex = li;
                    ulong[] innerSequence = null;

                    _links.Each(left, right, pair =>
                    {
                        if (innerSequence == null)
                        {
                            innerSequence = new ulong[innerSequenceLength];

                            for (var isi = 0; isi < linkIndex; isi++)
                                innerSequence[isi] = sequence[isi];

                            for (var isi = linkIndex + 1; isi < innerSequenceLength; isi++)
                                innerSequence[isi] = sequence[isi + 1];
                        }

                        innerSequence[linkIndex] = pair;

                        EachCore(handler, innerSequence);

                        return Pairs.Links.Continue;
                    });
                }
            }
        }

        public HashSet<ulong> EachPart(params ulong[] sequence)
        {
            var visitedLinks = new HashSet<ulong>(); // Заменить на bitstring

            EachPartCore(link =>
            {
                if (!visitedLinks.Contains(link)) visitedLinks.Add(link); // изучить почему случаются повторы
                return true;
            }, sequence);

            return visitedLinks;
        }

        public void EachPart(Func<ulong, bool> handler, params ulong[] sequence)
        {
            var visitedLinks = new HashSet<ulong>(); // Заменить на bitstring

            EachPartCore(link =>
            {
                if (!visitedLinks.Contains(link))
                {
                    visitedLinks.Add(link); // изучить почему случаются повторы
                    return handler(link);
                }

                return true;
            }, sequence);
        }

        private void EachPartCore(Func<ulong, bool> handler, params ulong[] sequence)
        {
            if (sequence == null || sequence.Length == 0)
                return;

            EnsureEachLinkIsAnyOrExists(_links, sequence);

            if (sequence.Length == 1)
            {
                var link = sequence[0];

                if (link > 0)
                    handler(link);
                else
                    _links.Each(0, 0, handler);
            }
            else if (sequence.Length == 2)
            {
                //_links.Each(sequence[0], sequence[1], handler);

                //  o_|      x_o ... 
                // x_|        |___|

                _links.Each(sequence[1], 0, pair =>
                {
                    var match = _links.Search(sequence[0], pair);
                    if (match != 0)
                        handler(match);
                    return true;
                });

                // |_x      ... x_o
                //  |_o      |___|

                _links.Each(0, sequence[0], pair =>
                {
                    var match = _links.Search(pair, sequence[1]);
                    if (match != 0)
                        handler(match);
                    return true;
                });

                //          ._x o_.
                //           |___|

                PartialStepRight(x => handler(x), sequence[0], sequence[1]);
            }
            else
            {
                // TODO: Implement other variants
                return;
            }
        }

        private void PartialStepRight(Action<ulong> handler, ulong left, ulong right)
        {
            _links.Each(0, left, pair =>
            {
                StepRight(handler, pair, right);

                if (left != pair)
                    PartialStepRight(handler, pair, right);

                return true;
            });
        }

        private void StepRight(Action<ulong> handler, ulong left, ulong right)
        {
            _links.Each(left, 0, rightStep =>
            {
                TryStepRightUp(handler, right, rightStep);
                return true;
            });
        }

        private void TryStepRightUp(Action<ulong> handler, ulong right, ulong stepFrom)
        {
            var upStep = stepFrom;
            var firstSource = _links.GetTarget(upStep);
            while (firstSource != right && firstSource != upStep)
            {
                upStep = firstSource;
                firstSource = _links.GetSource(upStep);
            }

            if (firstSource == right)
                handler(stepFrom);
        }

        // TODO: Test
        private void PartialStepLeft(Action<ulong> handler, ulong left, ulong right)
        {
            _links.Each(right, 0, pair =>
            {
                StepLeft(handler, left, pair);

                if (right != pair)
                    PartialStepLeft(handler, left, pair);

                return true;
            });
        }

        private void StepLeft(Action<ulong> handler, ulong left, ulong right)
        {
            _links.Each(0, right, leftStep =>
            {
                TryStepLeftUp(handler, left, leftStep);
                return true;
            });
        }

        private void TryStepLeftUp(Action<ulong> handler, ulong left, ulong stepFrom)
        {
            var upStep = stepFrom;
            var firstTarget = _links.GetSource(upStep);
            while (firstTarget != left && firstTarget != upStep)
            {
                upStep = firstTarget;
                firstTarget = _links.GetTarget(upStep);
            }

            if (firstTarget == left)
                handler(stepFrom);
        }

        private bool StartsWith(ulong sequence, ulong link)
        {
            var upStep = sequence;
            var firstSource = _links.GetSource(upStep);
            while (firstSource != link && firstSource != upStep)
            {
                upStep = firstSource;
                firstSource = _links.GetSource(upStep);
            }
            return firstSource == link;
        }

        private bool EndsWith(ulong sequence, ulong link)
        {
            var upStep = sequence;
            var lastTarget = _links.GetTarget(upStep);
            while (lastTarget != link && lastTarget != upStep)
            {
                upStep = lastTarget;
                lastTarget = _links.GetTarget(upStep);
            }
            return lastTarget == link;
        }

        public ulong Update(ulong[] sequence, ulong[] newSequence)
        {
            return _sync.ExecuteWriteOperation(() =>
            {
                if (sequence == null || sequence.Length == 0)
                    ; // -> Create
                else
                    EnsureEachLinkIsAnyOrExists(_links, sequence);

                if (newSequence == null || newSequence.Length == 0)
                    ; // -> Delete
                else
                    EnsureEachLinkIsAnyOrExists(_links, newSequence);

                return UpdateCore(sequence, newSequence);
            });
        }

        public List<ulong> GetAllMatchingSequences0(params ulong[] sequence)
        {
            return _sync.ExecuteReadOperation(() =>
            {
                var results = new List<ulong>();

                if (sequence.Length > 0)
                {
                    EnsureEachLinkExists(_links, sequence);

                    var firstElement = sequence[0];

                    if (sequence.Length == 1)
                    {
                        results.Add(firstElement);
                        return results;
                    }
                    if (sequence.Length == 2)
                    {
                        var pair = _links.Search(firstElement, sequence[1]);
                        if (pair != Pairs.Links.Null)
                            results.Add(pair);
                        return results;
                    }

                    var linksInSequence = new HashSet<ulong>(sequence);

                    Action<ulong> handler = result =>
                    {
                        var filterPosition = 0;

                        StopableSequenceWalker.WalkRight(result, _links.GetSourceCore, _links.GetTargetCore,
                            x => linksInSequence.Contains(x) || _links.GetTargetCore(x) == x, x =>
                            {
                                if (filterPosition == sequence.Length)
                                {
                                    filterPosition = -2; // Длиннее чем нужно
                                    return false;
                                }

                                if (x != sequence[filterPosition])
                                {
                                    filterPosition = -1;
                                    return false; // Начинается иначе
                                }

                                filterPosition++;

                                return true;
                            });

                        if (filterPosition == sequence.Length)
                        {
                            results.Add(result);
                        }
                    };

                    if (sequence.Length >= 2)
                        StepRight(handler, sequence[0], sequence[1]);

                    var last = sequence.Length - 2;
                    for (var i = 1; i < last; i++)
                        PartialStepRight(handler, sequence[i], sequence[i + 1]);

                    if (sequence.Length >= 3)
                        StepLeft(handler, sequence[sequence.Length - 2], sequence[sequence.Length - 1]);
                }

                return results;
            });
        }

        public HashSet<ulong> GetAllMatchingSequences1(params ulong[] sequence)
        {
            return _sync.ExecuteReadOperation(() =>
            {
                var results = new HashSet<ulong>();

                if (sequence.Length > 0)
                {
                    EnsureEachLinkExists(_links, sequence);

                    var firstElement = sequence[0];

                    if (sequence.Length == 1)
                    {
                        results.Add(firstElement);
                        return results;
                    }
                    if (sequence.Length == 2)
                    {
                        var pair = _links.Search(firstElement, sequence[1]);
                        if (pair != Pairs.Links.Null)
                            results.Add(pair);
                        return results;
                    }

                    var matcher = new Matcher(this, sequence, results);

                    if (sequence.Length >= 2)
                        StepRight(matcher.AddFullMatchedToResults, sequence[0], sequence[1]);

                    var last = sequence.Length - 2;
                    for (var i = 1; i < last; i++)
                        PartialStepRight(matcher.AddFullMatchedToResults, sequence[i], sequence[i + 1]);

                    if (sequence.Length >= 3)
                        StepLeft(matcher.AddFullMatchedToResults, sequence[sequence.Length - 2], sequence[sequence.Length - 1]);
                }

                return results;
            });
        }

        public const int MaxSequenceFormatSize = 200;

        public string FormatSequence(LinkIndex sequenceLink, params LinkIndex[] knownElements)
        {
            return FormatSequence(sequenceLink, x => x.ToString(), true, knownElements);
        }

        public string FormatSequence(LinkIndex sequenceLink, Func<LinkIndex, string> elementToString, bool insertComma, params LinkIndex[] knownElements)
        {
            int visitedElements = 0;

            var linksInSequence = new HashSet<ulong>(knownElements);

            var sb = new StringBuilder();

            sb.Append('[');

            if (_links.Exists(sequenceLink))
            {
                StopableSequenceWalker.WalkRight(sequenceLink, _links.GetSourceCore, _links.GetTargetCore,
                    x => linksInSequence.Contains(x) || _links.GetTargetCore(x) == x, element =>
                    {
                        if (insertComma && visitedElements > 0)
                            sb.Append(',');

                        sb.Append(elementToString(element));

                        visitedElements++;

                        if (visitedElements < MaxSequenceFormatSize)
                        {
                            return true;
                        }
                        else
                        {
                            sb.Append(insertComma ? ", ..." : "...");
                            return false;
                        }
                    });
            }

            sb.Append(']');

            return sb.ToString();
        }

        public List<ulong> GetAllPartiallyMatchingSequences0(params ulong[] sequence)
        {
            return _sync.ExecuteReadOperation(() =>
            {
                if (sequence.Length > 0)
                {
                    EnsureEachLinkExists(_links, sequence);

                    var results = new HashSet<ulong>();

                    for (int i = 0; i < sequence.Length; i++)
                        AllUsagesCore(sequence[i], results);

                    var filteredResults = new List<ulong>();

                    var linksInSequence = new HashSet<ulong>(sequence);

                    foreach (var result in results)
                    {
                        var filterPosition = -1;

                        StopableSequenceWalker.WalkRight(result, _links.GetSourceCore, _links.GetTargetCore,
                            x => linksInSequence.Contains(x) || _links.GetTargetCore(x) == x, x =>
                            {
                                if (filterPosition == (sequence.Length - 1))
                                    return false;

                                if (filterPosition >= 0)
                                {
                                    if (x == sequence[filterPosition + 1])
                                        filterPosition++;
                                    else
                                        return false;
                                }

                                if (filterPosition < 0)
                                {
                                    if (x == sequence[0])
                                        filterPosition = 0;
                                }

                                return true;
                            });

                        if (filterPosition == (sequence.Length - 1))
                        {
                            filteredResults.Add(result);
                        }
                    }

                    return filteredResults;
                }

                return new List<ulong>();
            });
        }

        public HashSet<ulong> GetAllPartiallyMatchingSequences1(params ulong[] sequence)
        {
            return _sync.ExecuteReadOperation(() =>
            {
                if (sequence.Length > 0)
                {
                    EnsureEachLinkExists(_links, sequence);

                    var results = new HashSet<ulong>();
                    for (int i = 0; i < sequence.Length; i++)
                        AllUsagesCore(sequence[i], results);

                    var filteredResults = new HashSet<ulong>();
                    var matcher = new Matcher(this, sequence, filteredResults);
                    matcher.AddAllPartialMatchedToResults(results);
                    return filteredResults;
                }

                return new HashSet<ulong>();
            });
        }

        public List<ulong> GetAllPartiallyMatchingSequences(params ulong[] sequence)
        {
            return _sync.ExecuteReadOperation(() =>
            {
                if (sequence.Length > 0)
                {
                    EnsureEachLinkExists(_links, sequence);

                    //var firstElement = sequence[0];

                    //if (sequence.Length == 1)
                    //{
                    //    //results.Add(firstElement);
                    //    return results;
                    //}
                    //if (sequence.Length == 2)
                    //{
                    //    //var pair = _links.Search(firstElement, sequence[1]);
                    //    //if (pair != Pairs.Links.Null)
                    //    //    results.Add(pair);
                    //    return results;
                    //}

                    //var lastElement = sequence[sequence.Length - 1];

                    //Func<ulong, bool> handler = x =>
                    //{
                    //    if (StartsWith(x, firstElement) && EndsWith(x, lastElement)) results.Add(x);
                    //    return true;
                    //};

                    //if (sequence.Length >= 2)
                    //    StepRight(handler, sequence[0], sequence[1]);

                    //var last = sequence.Length - 2;
                    //for (var i = 1; i < last; i++)
                    //    PartialStepRight(handler, sequence[i], sequence[i + 1]);

                    //if (sequence.Length >= 3)
                    //    StepLeft(handler, sequence[sequence.Length - 2], sequence[sequence.Length - 1]);

                    //////if (sequence.Length == 1)
                    //////{
                    //////    throw new NotImplementedException(); // all sequences, containing this element?
                    //////}
                    //////if (sequence.Length == 2)
                    //////{
                    //////    var results = new List<ulong>();
                    //////    PartialStepRight(results.Add, sequence[0], sequence[1]);
                    //////    return results;
                    //////}

                    //////var matches = new List<List<ulong>>();

                    //////var last = sequence.Length - 1;
                    //////for (var i = 0; i < last; i++)
                    //////{
                    //////    var results = new List<ulong>();
                    //////    //StepRight(results.Add, sequence[i], sequence[i + 1]);
                    //////    PartialStepRight(results.Add, sequence[i], sequence[i + 1]);

                    //////    if (results.Count > 0)
                    //////        matches.Add(results);
                    //////    else
                    //////        return results;

                    //////    if (matches.Count == 2)
                    //////    {
                    //////        var merged = new List<ulong>();

                    //////        for (var j = 0; j < matches[0].Count; j++)
                    //////            for (var k = 0; k < matches[1].Count; k++)
                    //////                CloseInnerConnections(merged.Add, matches[0][j], matches[1][k]);

                    //////        if (merged.Count > 0)
                    //////            matches = new List<List<ulong>> { merged };
                    //////        else
                    //////            return new List<ulong>();
                    //////    }
                    //////}

                    //////if (matches.Count > 0)
                    //////{
                    //////    var usages = new HashSet<ulong>();

                    //////    for (int i = 0; i < sequence.Length; i++)
                    //////    {
                    //////        AllUsagesCore(sequence[i], usages);
                    //////    }

                    //////    //for (int i = 0; i < matches[0].Count; i++)
                    //////    //    AllUsagesCore(matches[0][i], usages);

                    //////    //usages.UnionWith(matches[0]);

                    //////    return usages.ToList();
                    //////}

                    var firstLinkUsages = new HashSet<ulong>();
                    AllUsagesCore(sequence[0], firstLinkUsages);
                    firstLinkUsages.Add(sequence[0]);
                    //var previousMatchings = firstLinkUsages.ToList(); //new List<ulong>() { sequence[0] }; // or all sequences, containing this element?
                    //return GetAllPartiallyMatchingSequencesCore(sequence, firstLinkUsages, 1).ToList();

                    var results = new HashSet<ulong>();
                    foreach (var match in GetAllPartiallyMatchingSequencesCore(sequence, firstLinkUsages, 1))
                        AllUsagesCore(match, results);
                    return results.ToList();
                }

                return new List<ulong>();
            });
        }

        public HashSet<ulong> AllUsages(ulong link)
        {
            return _sync.ExecuteReadOperation(() =>
            {
                var usages = new HashSet<ulong>();
                AllUsagesCore(link, usages);
                return usages;
            });
        }

        // При сборе всех использований (последовательностей) можно сохранять обратный путь к той связи с которой начинался поиск (STTTSSSTT),
        // причём достаточно одного бита для хранения перехода влево или вправо
        private void AllUsagesCore(ulong link, HashSet<ulong> usages)
        {
            Func<ulong, bool> handler = pair =>
            {
                if (usages.Add(pair)) AllUsagesCore(pair, usages);
                return true;
            };
            _links.Each(link, 0, handler);
            _links.Each(0, link, handler);
        }

        private class AllUsagesCollector
        {
            private readonly Pairs.Links _links;
            private readonly HashSet<ulong> _usages;

            public AllUsagesCollector(Pairs.Links links, HashSet<ulong> usages)
            {
                _links = links;
                _usages = usages;
            }

            public bool Collect(ulong link)
            {
                if (_usages.Add(link))
                {
                    _links.Each(link, 0, Collect);
                    _links.Each(0, link, Collect);
                }
                return true;
            }
        }

        private class AllUsagesIntersectingCollector
        {
            private readonly Pairs.Links _links;
            private readonly HashSet<ulong> _intersectWith;
            private readonly HashSet<ulong> _usages;
            private readonly HashSet<ulong> _enter;

            public AllUsagesIntersectingCollector(Pairs.Links links, HashSet<ulong> intersectWith, HashSet<ulong> usages)
            {
                _links = links;
                _intersectWith = intersectWith;
                _usages = usages;
                _enter = new HashSet<ulong>(); // защита от зацикливания
            }

            public bool Collect(ulong link)
            {
                if (_enter.Add(link))
                {
                    if (_intersectWith.Contains(link))
                        _usages.Add(link);

                    _links.Each(link, 0, Collect);
                    _links.Each(0, link, Collect);
                }
                return true;
            }
        }

        private void CloseInnerConnections(Action<ulong> handler, ulong left, ulong right)
        {
            TryStepLeftUp(handler, left, right);
            TryStepRightUp(handler, right, left);
        }

        private void AllCloseConnections(Action<ulong> handler, ulong left, ulong right)
        {
            // Direct

            if (left == right)
                handler(left);
            var pair = _links.Search(left, right);
            if (pair != Pairs.Links.Null)
                handler(pair);

            // Inner

            CloseInnerConnections(handler, left, right);

            // Outer

            StepLeft(handler, left, right);
            StepRight(handler, left, right);

            PartialStepRight(handler, left, right);
            PartialStepLeft(handler, left, right);
        }

        private HashSet<ulong> GetAllPartiallyMatchingSequencesCore(ulong[] sequence, HashSet<ulong> previousMatchings, long startAt)
        {
            if (startAt >= sequence.Length) // ?
                return previousMatchings;

            var secondLinkUsages = new HashSet<ulong>();
            AllUsagesCore(sequence[startAt], secondLinkUsages);
            secondLinkUsages.Add(sequence[startAt]);

            var matchings = new HashSet<ulong>();

            //for (var i = 0; i < previousMatchings.Count; i++)

            foreach (var secondLinkUsage in secondLinkUsages)
            {
                foreach (var previousMatching in previousMatchings)
                {
                    //AllCloseConnections(matchings.AddAndReturnVoid, previousMatching, secondLinkUsage);

                    StepRight(matchings.AddAndReturnVoid, previousMatching, secondLinkUsage);
                    TryStepRightUp(matchings.AddAndReturnVoid, secondLinkUsage, previousMatching);
                    //PartialStepRight(matchings.AddAndReturnVoid, secondLinkUsage, sequence[startAt]); // почему-то эта ошибочная запись приводит к желаемым результам.
                    PartialStepRight(matchings.AddAndReturnVoid, previousMatching, secondLinkUsage);
                }
            }

            if (matchings.Count == 0)
                return matchings;

            return GetAllPartiallyMatchingSequencesCore(sequence, matchings, startAt + 1); // ??
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

            if (_links.CalculateReferences(left) == 0)
            {
                _links.Delete(left);
                ClearGarbage(leftSource, leftTarget);
            }

            var rightSource = _links.GetSource(right);
            var rightTarget = _links.GetTarget(right);

            if (_links.CalculateReferences(right) == 0)
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

        private static void EnsureEachLinkExists(Pairs.Links links, params ulong[] sequence)
        {
            if (sequence == null)
                return;

            for (var i = 0; i < sequence.Length; i++)
                if (!links.Exists(sequence[i]))
                    throw new ArgumentLinkDoesNotExistsException<ulong>(sequence[i],
                        string.Format("patternSequence[{0}]", i));
        }

        private static void EnsureEachLinkIsAnyOrExists(Pairs.Links links, params ulong[] sequence)
        {
            if (sequence == null)
                return;

            for (var i = 0; i < sequence.Length; i++)
                if (sequence[i] != Pairs.Links.Null && !links.Exists(sequence[i]))
                    throw new ArgumentLinkDoesNotExistsException<ulong>(sequence[i],
                        string.Format("patternSequence[{0}]", i));
        }

        private static void EnsureEachLinkIsAnyOrZeroOrManyOrExists(Pairs.Links links, params ulong[] sequence)
        {
            if (sequence == null)
                return;

            for (var i = 0; i < sequence.Length; i++)
                if (sequence[i] != Pairs.Links.Null && sequence[i] != ZeroOrMany && !links.Exists(sequence[i]))
                    throw new ArgumentLinkDoesNotExistsException<ulong>(sequence[i],
                        string.Format("patternSequence[{0}]", i));
        }

        // Pattern Matching -> Key To Triggers
        public HashSet<ulong> MatchPattern(params ulong[] patternSequence)
        {
            return _sync.ExecuteReadOperation(() =>
            {
                patternSequence = Simplify(patternSequence);

                if (patternSequence.Length > 0)
                {
                    EnsureEachLinkIsAnyOrZeroOrManyOrExists(_links, patternSequence);

                    var uniqueSequenceElements = new HashSet<ulong>();
                    for (var i = 0; i < patternSequence.Length; i++)
                        if (patternSequence[i] != Pairs.Links.Null && patternSequence[i] != ZeroOrMany)
                            uniqueSequenceElements.Add(patternSequence[i]);

                    var results = new HashSet<ulong>();

                    foreach (var uniqueSequenceElement in uniqueSequenceElements)
                        AllUsagesCore(uniqueSequenceElement, results);

                    var filteredResults = new HashSet<ulong>();
                    var matcher = new PatternMatcher(this, patternSequence, filteredResults);
                    matcher.AddAllPatternMatchedToResults(results);
                    return filteredResults;
                }

                return new HashSet<ulong>();
            });
        }

        // Найти все возможные связи между указанным списком связей.
        // Находит связи между всеми указанными связями в любом порядке.
        // TODO: решить что делать с повторами (когда одни и те же элементы встречаются несколько раз в последовательности)
        public HashSet<ulong> GetAllConnections(params ulong[] linksToConnect)
        {
            return _sync.ExecuteReadOperation(() =>
            {
                var results = new HashSet<ulong>();

                if (linksToConnect.Length > 0)
                {
                    EnsureEachLinkExists(_links, linksToConnect);

                    AllUsagesCore(linksToConnect[0], results);

                    for (int i = 1; i < linksToConnect.Length; i++)
                    {
                        var next = new HashSet<ulong>();
                        AllUsagesCore(linksToConnect[i], next);

                        results.IntersectWith(next);
                    }
                }

                return results;
            });
        }

        public HashSet<ulong> GetAllConnections1(params ulong[] linksToConnect)
        {
            return _sync.ExecuteReadOperation(() =>
            {
                var results = new HashSet<ulong>();

                if (linksToConnect.Length > 0)
                {
                    EnsureEachLinkExists(_links, linksToConnect);

                    var collector1 = new AllUsagesCollector(_links, results);
                    collector1.Collect(linksToConnect[0]);

                    for (int i = 1; i < linksToConnect.Length; i++)
                    {
                        var next = new HashSet<ulong>();
                        var collector = new AllUsagesCollector(_links, next);
                        collector.Collect(linksToConnect[i]);

                        results.IntersectWith(next);
                    }
                }

                return results;
            });
        }

        public HashSet<ulong> GetAllConnections2(params ulong[] linksToConnect)
        {
            return _sync.ExecuteReadOperation(() =>
            {
                var results = new HashSet<ulong>();

                if (linksToConnect.Length > 0)
                {
                    EnsureEachLinkExists(_links, linksToConnect);

                    var collector1 = new AllUsagesCollector(_links, results);
                    collector1.Collect(linksToConnect[0]);

                    //AllUsagesCore(linksToConnect[0], results);

                    for (int i = 1; i < linksToConnect.Length; i++)
                    {
                        var next = new HashSet<ulong>();
                        var collector = new AllUsagesIntersectingCollector(_links, results, next);
                        collector.Collect(linksToConnect[i]);

                        //AllUsagesCore(linksToConnect[i], next);

                        //results.IntersectWith(next);

                        results = next;
                    }
                }

                return results;
            });
        }

        private static ulong[] Simplify(ulong[] sequence)
        {
            // Считаем новый размер последовательности
            long newLength = 0;
            bool zeroOrManyStepped = false;
            for (int i = 0; i < sequence.Length; i++)
            {
                if (sequence[i] == ZeroOrMany)
                {
                    if (zeroOrManyStepped)
                        continue;

                    zeroOrManyStepped = true;
                }
                else
                {
                    //if (zeroOrManyStepped) Is it efficient?
                    zeroOrManyStepped = false;
                }

                newLength++;
            }


            // Строим новую последовательность
            zeroOrManyStepped = false;
            var newSequence = new ulong[newLength];
            long j = 0;
            for (int i = 0; i < sequence.Length; i++)
            {
                //var current = zeroOrManyStepped;
                //zeroOrManyStepped = patternSequence[i] == zeroOrMany;
                //if (current && zeroOrManyStepped)
                //    continue;

                //var newZeroOrManyStepped = patternSequence[i] == zeroOrMany;
                //if (zeroOrManyStepped && newZeroOrManyStepped)
                //    continue;
                //zeroOrManyStepped = newZeroOrManyStepped;


                if (sequence[i] == ZeroOrMany)
                {
                    if (zeroOrManyStepped)
                        continue;

                    zeroOrManyStepped = true;
                }
                else
                {
                    //if (zeroOrManyStepped) Is it efficient?

                    zeroOrManyStepped = false;
                }

                newSequence[j++] = sequence[i];
            }

            return newSequence;
        }

        public static void TestSimplify()
        {
            var sequence = new ulong[] { ZeroOrMany, ZeroOrMany, 2, 3, 4, ZeroOrMany, ZeroOrMany, ZeroOrMany, 4, ZeroOrMany, ZeroOrMany, ZeroOrMany };
            const ulong zeroOrMany = 1UL;

            var simplifiedSequence = Simplify(sequence);
        }

        public List<ulong> GetSimilarSequences()
        {
            return new List<ulong>();
        }

        public void Prediction()
        {
            //_links
            //sequences
        }

        #region From Triplets

        //public static void DeleteSequence(Link sequence)
        //{
        //}

        public List<ulong> CollectMatchingSequences(ulong[] links)
        {
            if (links.Length == 1)
            {
                throw new Exception("Подпоследовательности с одним элементом не поддерживаются.");
            }

            int leftBound = 0;
            int rightBound = links.Length - 1;

            ulong left = links[leftBound++];
            ulong right = links[rightBound--];

            var results = new List<ulong>();
            CollectMatchingSequences(left, leftBound, links, right, rightBound, ref results);
            return results;
        }

        private void CollectMatchingSequences(ulong leftLink, int leftBound, ulong[] middleLinks, ulong rightLink, int rightBound, ref List<ulong> results)
        {
            ulong leftLinkTotalReferers = _links.CalculateReferences(leftLink);
            ulong rightLinkTotalReferers = _links.CalculateReferences(rightLink);

            if (leftLinkTotalReferers <= rightLinkTotalReferers)
            {
                var nextLeftLink = middleLinks[leftBound];

                ulong[] elements = GetRightElements(leftLink, nextLeftLink);
                if (leftBound <= rightBound)
                {
                    for (int i = elements.Length - 1; i >= 0; i--)
                    {
                        var element = elements[i];
                        if (element != 0)
                        {
                            CollectMatchingSequences(element, leftBound + 1, middleLinks, rightLink, rightBound, ref results);
                        }
                    }
                }
                else
                {
                    for (int i = elements.Length - 1; i >= 0; i--)
                    {
                        var element = elements[i];
                        if (element != 0)
                        {
                            results.Add(element);
                        }
                    }
                }
            }
            else
            {
                var nextRightLink = middleLinks[rightBound];

                ulong[] elements = GetLeftElements(rightLink, nextRightLink);

                if (leftBound <= rightBound)
                {
                    for (int i = elements.Length - 1; i >= 0; i--)
                    {
                        var element = elements[i];
                        if (element != 0)
                        {
                            CollectMatchingSequences(leftLink, leftBound, middleLinks, elements[i], rightBound - 1, ref results);
                        }
                    }
                }
                else
                {
                    for (int i = elements.Length - 1; i >= 0; i--)
                    {
                        var element = elements[i];
                        if (element != 0)
                        {
                            results.Add(element);
                        }
                    }
                }
            }
        }

        public ulong[] GetRightElements(ulong startLink, ulong rightLink)
        {
            var result = new ulong[5];

            TryStepRight(startLink, rightLink, result, 0);

            _links.Each(0, startLink, couple =>
            {
                if (couple != startLink)
                    if (TryStepRight(couple, rightLink, result, 2))
                        return false;

                return true;
            });

            if (_links.GetTarget(_links.GetTarget(startLink)) == rightLink)
                result[4] = startLink;

            return result;
        }

        public bool TryStepRight(ulong startLink, ulong rightLink, ulong[] result, int offset)
        {
            int added = 0;

            _links.Each(startLink, 0, couple =>
            {
                if (couple != startLink)
                {
                    var coupleTarget = _links.GetTarget(couple);
                    if (coupleTarget == rightLink)
                    {
                        result[offset] = couple;
                        if (++added == 2)
                            return false;
                    }
                    else if (_links.GetSource(coupleTarget) == rightLink) // coupleTarget.Linker == Net.And &&
                    {
                        result[offset + 1] = couple;
                        if (++added == 2)
                            return false;
                    }
                }

                return true;
            });

            return added > 0;
        }

        public ulong[] GetLeftElements(ulong startLink, ulong leftLink)
        {
            var result = new ulong[5];

            TryStepLeft(startLink, leftLink, result, 0);

            _links.Each(startLink, 0, couple =>
            {
                if (couple != startLink)
                    if (TryStepLeft(couple, leftLink, result, 2))
                        return false;

                return true;
            });

            if (_links.GetSource(_links.GetSource(leftLink)) == startLink)
                result[4] = leftLink;

            return result;
        }

        public bool TryStepLeft(ulong startLink, ulong leftLink, ulong[] result, int offset)
        {
            int added = 0;

            _links.Each(0, startLink, couple =>
            {
                if (couple != startLink)
                {
                    var coupleSource = _links.GetSource(couple);
                    if (coupleSource == leftLink)
                    {
                        result[offset] = couple;
                        if (++added == 2)
                            return false;
                    }
                    else if (_links.GetTarget(coupleSource) == leftLink) // coupleSource.Linker == Net.And &&
                    {
                        result[offset + 1] = couple;
                        if (++added == 2)
                            return false;
                    }
                }

                return true;
            });

            return added > 0;
        }

        #endregion


        #region Walkers

        public class Walker
        {
            protected readonly Pairs.Links Links;

            public Walker(Sequences sequences)
            {
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
            private long _filterPosition;

            public Matcher(Sequences sequences, LinkIndex[] patternSequence, HashSet<LinkIndex> results)
                : base(sequences)
            {
                _patternSequence = patternSequence;
                _linksInSequence = new HashSet<LinkIndex>(patternSequence.Where(x => x != Pairs.Links.Null && x != ZeroOrMany));
                _results = results;
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

        public class PatternMatcher : Walker
        {
            private readonly ulong[] _patternSequence;
            private readonly HashSet<LinkIndex> _linksInSequence;
            private readonly HashSet<LinkIndex> _results;

            #region Pattern Match

            enum PatternBlockType
            {
                Undefined,
                Gap,
                Elements
            }

            struct PatternBlock
            {
                public PatternBlockType Type;
                public long Start;
                public long Stop;
            }

            private List<PatternBlock> _pattern;
            private int _patternPosition;
            private long _sequencePosition;

            #endregion

            public PatternMatcher(Sequences sequences, LinkIndex[] patternSequence, HashSet<LinkIndex> results)
                : base(sequences)
            {
                _patternSequence = patternSequence;
                _linksInSequence = new HashSet<LinkIndex>(patternSequence.Where(x => x != Pairs.Links.Null && x != ZeroOrMany));
                _results = results;

                // TODO: Переместить в PatternMatcher
                _pattern = CreateDetailedPattern();
            }

            protected override bool IsElement(ulong link)
            {
                return _linksInSequence.Contains(link) || Links.GetTargetCore(link) == link || Links.GetSourceCore(link) == link;
            }

            public bool PatternMatch(LinkIndex sequenceToMatch)
            {
                _patternPosition = 0;
                _sequencePosition = 0;

                WalkRight(sequenceToMatch, (Func<ulong, bool>)PatternMatchCore);

                return _patternPosition == _pattern.Count || (_patternPosition == _pattern.Count - 1 && _pattern[_patternPosition].Start == 0);
            }

            private List<PatternBlock> CreateDetailedPattern()
            {
                var pattern = new List<PatternBlock>();

                var patternBlock = new PatternBlock();

                for (var i = 0; i < _patternSequence.Length; i++)
                {
                    if (patternBlock.Type == PatternBlockType.Undefined)
                    {
                        if (_patternSequence[i] == Any)
                        {
                            patternBlock.Type = PatternBlockType.Gap;
                            patternBlock.Start = 1;
                            patternBlock.Stop = 1;
                        }
                        else if (_patternSequence[i] == ZeroOrMany)
                        {
                            patternBlock.Type = PatternBlockType.Gap;
                            patternBlock.Start = 0;
                            patternBlock.Stop = long.MaxValue;
                        }
                        else
                        {
                            patternBlock.Type = PatternBlockType.Elements;
                            patternBlock.Start = i;
                            patternBlock.Stop = i;
                        }
                    }
                    else if (patternBlock.Type == PatternBlockType.Elements)
                    {
                        if (_patternSequence[i] == Any)
                        {
                            pattern.Add(patternBlock);

                            patternBlock = new PatternBlock
                            {
                                Type = PatternBlockType.Gap,
                                Start = 1,
                                Stop = 1
                            };
                        }
                        else if (_patternSequence[i] == ZeroOrMany)
                        {
                            pattern.Add(patternBlock);

                            patternBlock = new PatternBlock
                            {
                                Type = PatternBlockType.Gap,
                                Start = 0,
                                Stop = long.MaxValue
                            };
                        }
                        else
                        {
                            patternBlock.Stop = i;
                        }
                    }
                    else // patternBlock.Type == PatternBlockType.Gap
                    {
                        if (_patternSequence[i] == Any)
                        {
                            patternBlock.Start++;

                            if (patternBlock.Stop < patternBlock.Start)
                                patternBlock.Stop = patternBlock.Start;
                        }
                        else if (_patternSequence[i] == ZeroOrMany)
                        {
                            patternBlock.Stop = long.MaxValue;
                        }
                        else
                        {
                            pattern.Add(patternBlock);

                            patternBlock = new PatternBlock
                            {
                                Type = PatternBlockType.Elements,
                                Start = i,
                                Stop = i
                            };
                        }
                    }
                }

                if (patternBlock.Type != PatternBlockType.Undefined)
                    pattern.Add(patternBlock);

                return pattern;
            }

            ///* match: search for regexp anywhere in text */
            //int match(char* regexp, char* text)
            //{
            //    do
            //    {   
            //    } while (*text++ != '\0');
            //    return 0;
            //}

            ///* matchhere: search for regexp at beginning of text */
            //int matchhere(char* regexp, char* text)
            //{
            //    if (regexp[0] == '\0')
            //        return 1;
            //    if (regexp[1] == '*')
            //        return matchstar(regexp[0], regexp + 2, text);
            //    if (regexp[0] == '$' && regexp[1] == '\0')
            //        return *text == '\0';
            //    if (*text != '\0' && (regexp[0] == '.' || regexp[0] == *text))
            //        return matchhere(regexp + 1, text + 1);
            //    return 0;
            //}

            ///* matchstar: search for c*regexp at beginning of text */
            //int matchstar(int c, char* regexp, char* text)
            //{
            //    do
            //    {    /* a * matches zero or more instances */
            //        if (matchhere(regexp, text))
            //            return 1;
            //    } while (*text != '\0' && (*text++ == c || c == '.'));
            //    return 0;
            //}

            //private void GetNextPatternElement(out LinkIndex element, out long mininumGap, out long maximumGap)
            //{
            //    mininumGap = 0;
            //    maximumGap = 0;
            //    element = 0;
            //    for (; _patternPosition < _patternSequence.Length; _patternPosition++)
            //    {
            //        if (_patternSequence[_patternPosition] == Pairs.Links.Null)
            //            mininumGap++;
            //        else if (_patternSequence[_patternPosition] == ZeroOrMany)
            //            maximumGap = long.MaxValue;
            //        else
            //            break;
            //    }

            //    if (maximumGap < mininumGap)
            //        maximumGap = mininumGap;
            //}

            private bool PatternMatchCore(LinkIndex element)
            {
                if (_patternPosition >= _pattern.Count)
                {
                    _patternPosition = -2;
                    return false;
                }

                var currentPatternBlock = _pattern[_patternPosition];

                if (currentPatternBlock.Type == PatternBlockType.Gap)
                {
                    //var currentMatchingBlockLength = (_sequencePosition - _lastMatchedBlockPosition);

                    if (_sequencePosition < currentPatternBlock.Start)
                    {
                        _sequencePosition++;
                        return true; // Двигаемся дальше
                    }

                    // Это последний блок
                    if (_pattern.Count == _patternPosition + 1)
                    {
                        _patternPosition++;
                        _sequencePosition = 0;
                        return false; // Полное соответствие
                    }
                    else
                    {
                        if (_sequencePosition > currentPatternBlock.Stop)
                            return false; // Соответствие невозможно

                        var nextPatternBlock = _pattern[_patternPosition + 1];

                        if (_patternSequence[nextPatternBlock.Start] == element)
                        {
                            if (nextPatternBlock.Start < nextPatternBlock.Stop)
                            {
                                _patternPosition++;
                                _sequencePosition = 1;
                            }
                            else
                            {
                                _patternPosition += 2;
                                _sequencePosition = 0;
                            }
                        }
                    }
                }
                else // currentPatternBlock.Type == PatternBlockType.Elements
                {
                    var patternElementPosition = currentPatternBlock.Start + _sequencePosition;

                    if (_patternSequence[patternElementPosition] != element)
                        return false; // Соответствие невозможно

                    if (patternElementPosition == currentPatternBlock.Stop)
                    {
                        _patternPosition++;
                        _sequencePosition = 0;
                    }
                    else
                    {
                        _sequencePosition++;
                    }
                }

                return true;

                //if (_patternSequence[_patternPosition] != element)
                //    return false;
                //else
                //{
                //    _sequencePosition++;
                //    _patternPosition++;

                //    return true;
                //}

                ////////

                //if (_filterPosition == _patternSequence.Length)
                //{
                //    _filterPosition = -2; // Длиннее чем нужно
                //    return false;
                //}

                //if (element != _patternSequence[_filterPosition])
                //{
                //    _filterPosition = -1;
                //    return false; // Начинается иначе
                //}

                //_filterPosition++;

                //if (_filterPosition == (_patternSequence.Length - 1))
                //    return false;

                //if (_filterPosition >= 0)
                //{
                //    if (element == _patternSequence[_filterPosition + 1])
                //        _filterPosition++;
                //    else
                //        return false;
                //}

                //if (_filterPosition < 0)
                //{
                //    if (element == _patternSequence[0])
                //        _filterPosition = 0;
                //}
            }

            public void AddAllPatternMatchedToResults(IEnumerable<ulong> sequencesToMatch)
            {
                foreach (var sequenceToMatch in sequencesToMatch)
                    if (PatternMatch(sequenceToMatch))
                        _results.Add(sequenceToMatch);
            }
        }

        #endregion
    }
}