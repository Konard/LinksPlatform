using System;
using System.Collections.Generic;
using Platform.Links.DataBase.CoreUnsafe.Exceptions;
using Platform.Links.System.Helpers;
using Platform.Links.System.Helpers.Synchronization;

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

                return CreateBalancedVariantCore(sequence);
            });
        }

        private ulong CreateBalancedVariantCore(params ulong[] sequence)
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

                StepRight(handler, sequence[0], sequence[1]);
            }
            else
            {
                // TODO: Implement other variants
                return;
            }
        }

        private void StepRight(Func<ulong, bool> handler, ulong left, ulong right)
        {
            _links.Each(0, left, pair =>
            {
                _links.Each(pair, 0, rightStep =>
                {
                    var upStep = rightStep;
                    var firstSource = _links.GetTarget(rightStep);
                    while (firstSource != right && firstSource != upStep)
                    {
                        upStep = firstSource;
                        firstSource = _links.GetSource(upStep);
                    }

                    if (firstSource == right)
                        handler(rightStep);

                    return true;
                });

                if (left != pair)
                    StepRight(handler, pair, right);

                return true;
            });
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

        private ulong UpdateCore(ulong[] sequence, ulong[] newSequence)
        {
            var bestVariant = CreateBalancedVariantCore(newSequence);

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

        // Pattern Matching -> Key To Triggers
        // Добавлять ли oneOrMany, anyone?
        public void MatchPattern(ulong zeroOrMany, params ulong[] patternSequence)
        {
            patternSequence = Simplify(zeroOrMany, patternSequence);
        }

        // Найти все возможные связи между указанным списком связей
        // Нужно находить связи между всеми указанными связями в любом порядке?
        // Или можно взять произвольные множества связей из исходного множества и найти для каждого из них все возможные связи?
        public void FindAllConnections(params ulong[] linksToConnect)
        {
        }

        private static ulong[] Simplify(ulong zeroOrMany, ulong[] sequence)
        {
            // Считаем новый размер последовательности
            long newLength = 0;
            bool zeroOrManyStepped = false;
            for (int i = 0; i < sequence.Length; i++)
            {
                if (sequence[i] == zeroOrMany)
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


                if (sequence[i] == zeroOrMany)
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
            var sequence = new ulong[] { 1, 1, 2, 3, 4, 1, 1, 1, 4, 1, 1, 1 };
            const ulong zeroOrMany = 1UL;

            var simplifiedSequence = Simplify(zeroOrMany, sequence);
        }


        public void Prediction()
        {
            //_links
            //sequences
        }

        #region From Triplets

        public const int MaxSequenceFormatSize = 20;

        //public static void DeleteSequence(Link sequence)
        //{
        //}

        //public static FormatSequence(Link sequence)
        //{
        //    int visitedElements = 0;

        //    StringBuilder sb = new StringBuilder();

        //    sb.Append('[');

        //    StopableSequenceWalker walker = new StopableSequenceWalker(sequence, element =>
        //    {
        //        if (visitedElements > 0)
        //            sb.Append(',');

        //        sb.Append(element.ToString());

        //        visitedElements++;

        //        if (visitedElements < MaxSequenceFormatSize)
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            sb.Append(", ...");
        //            return false;
        //        }
        //    });

        //    walker.WalkFromLeftToRight();

        //    sb.Append(']');

        //    return sb.ToString();
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
            var result = new ulong[4];

            TryStepRight(startLink, rightLink, result, 0);

            _links.Each(0, startLink, couple =>
            {
                if (couple != startLink)
                    if (TryStepRight(couple, rightLink, result, 2))
                        return false;

                return true;
            });

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
            var result = new ulong[4];

            TryStepLeft(startLink, leftLink, result, 0);

            _links.Each(startLink, 0, couple =>
            {
                if (couple != startLink)
                    if (TryStepLeft(couple, leftLink, result, 2))
                        return false;

                return true;
            });

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
    }
}