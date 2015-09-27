using System;
using System.Collections.Generic;
using Platform.Links.DataBase.CoreUnsafe.Exceptions;
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

        public ulong CreateAllVariants(params ulong[] sequence)
        {
            return _sync.ExecuteWriteOperation(() =>
            {
                if (sequence == null || sequence.Length == 0)
                    return Pairs.Links.Null;

                EnsureEachLinkExists(_links, sequence);

                if (sequence.Length == 1)
                    return sequence[0];
                if (sequence.Length == 2)
                    return _links.Create(sequence[0], sequence[1]);

                var innerSequenceLength = sequence.Length - 1;
                var innerSequence = new ulong[innerSequenceLength];
                var innerSequenceLink = Pairs.Links.Null;

                for (var li = 0; li < innerSequenceLength; li++)
                {
                    var link = Create(sequence[li], sequence[li + 1]);

                    for (var isi = 0; isi < innerSequence.Length; isi++)
                    {
                        if (isi < li) innerSequence[isi] = sequence[isi];
                        if (isi == li) innerSequence[isi] = link;
                        if (isi > li) innerSequence[isi] = sequence[isi + 1];
                    }

                    innerSequenceLink = Create(innerSequence);
                    if (innerSequenceLink == Pairs.Links.Null)
                        throw new NotImplementedException("Creation cancellation is not implemented.");
                }

                return innerSequenceLink;
            });
        }

        public ulong CreateBalancedVariant(params ulong[] sequence)
        {
            return _sync.ExecuteWriteOperation(() =>
            {
                if (sequence == null || sequence.Length == 0)
                    return Pairs.Links.Null;

                EnsureEachLinkExists(_links, sequence);

                return CreateBalancedVariantCore(sequence);
            });
        }

        private ulong CreateBalancedVariantCore(params ulong[] sequence)
        {
            if (sequence.Length == 1)
                return sequence[0];
            if (sequence.Length == 2)
                return _links.Create(sequence[0], sequence[1]);

            var innerSequence = new ulong[sequence.Length / 2 + sequence.Length % 2];

            for (var i = 0; i < sequence.Length; i += 2)
                innerSequence[i / 2] = i + 1 == sequence.Length ? sequence[i] : Create(sequence[i], sequence[i + 1]);

            return CreateBalancedVariantCore(innerSequence);
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

        private void EachCore(Func<ulong, bool> handler, params ulong[] sequence)
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
                _links.Each(sequence[0], sequence[1], handler);
            }
            else
            {
                var innerSequenceLength = sequence.Length - 1;
                var innerSequence = new ulong[innerSequenceLength];

                for (var li = 0; li < innerSequenceLength; li++)
                {
                    var left = sequence[li];
                    var right = sequence[li + 1];

                    if (left == 0 && right == 0)
                        continue;

                    for (var isi = 0; isi < innerSequence.Length; isi++)
                    {
                        if (isi < li) innerSequence[isi] = sequence[isi];
                        if (isi > li) innerSequence[isi] = sequence[isi + 1];
                    }

                    var linkIndex = li;

                    _links.Each(left, right, pair =>
                    {
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


            }
            else
            {
                // TODO: Implement other variants
                return;
            }
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
                _links.Delete(ref left);
                ClearGarbage(leftSource, leftTarget);
            }

            var rightSource = _links.GetSource(right);
            var rightTarget = _links.GetTarget(right);

            if (_links.CalculateReferences(right) == 0)
            {
                _links.Delete(ref right);
                ClearGarbage(rightSource, rightTarget);
            }
        }

        public void Delete(params ulong[] sequence)
        {
            _sync.ExecuteWriteOperation(() => {
                foreach (var linkToDelete in Each(sequence))
                {
                    var x = linkToDelete;
                    _links.Delete(ref x);
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
    }
}