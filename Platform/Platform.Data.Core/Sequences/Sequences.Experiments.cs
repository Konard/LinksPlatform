using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.Data.Core.Collections;
using Platform.Data.Core.Exceptions;
using Platform.Data.Core.Pairs;
using Platform.Helpers;
using Platform.Helpers.Collections;
using LinkIndex = System.UInt64;

namespace Platform.Data.Core.Sequences
{
    partial class Sequences
    {
        #region Create All Variants (Not Practical)

        /// <remarks>
        /// Number of links that is needed to generate all variants for
        /// sequence of length N corresponds to https://oeis.org/A014143/list sequence.
        /// </remarks>
        public ulong[] CreateAllVariants2(ulong[] sequence)
        {
            return Sync.ExecuteWriteOperation(() =>
            {
                if (sequence.IsNullOrEmpty())
                    return new ulong[0];

                Links.EnsureEachLinkExists(sequence);

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
                return new[] { Links.CreateCore(sequence[startAt], sequence[stopAt]) };

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
                        var variant = Links.CreateCore(left[i], right[j]);
                        if (variant == LinksConstants.Null)
                            throw new NotImplementedException("Creation cancellation is not implemented.");
                        variants[last++] = variant;
                    }
                }
            }

            return variants;
        }

        public List<ulong> CreateAllVariants1(params ulong[] sequence)
        {
            return Sync.ExecuteWriteOperation(() =>
            {
                if (sequence.IsNullOrEmpty())
                    return new List<ulong>();

                Links.EnsureEachLinkExists(sequence);

                if (sequence.Length == 1)
                    return new List<ulong> { sequence[0] };

                var results = new List<ulong>((int)MathHelpers.Catalan(sequence.Length));
                return CreateAllVariants1Core(sequence, results);
            });
        }

        private List<ulong> CreateAllVariants1Core(ulong[] sequence, List<ulong> results)
        {
            if (sequence.Length == 2)
            {
                var link = Links.CreateCore(sequence[0], sequence[1]);
                if (link == LinksConstants.Null)
                    throw new NotImplementedException("Creation cancellation is not implemented.");
                results.Add(link);
                return results;
            }

            var innerSequenceLength = sequence.Length - 1;
            var innerSequence = new ulong[innerSequenceLength];

            for (var li = 0; li < innerSequenceLength; li++)
            {
                var link = Links.CreateCore(sequence[li], sequence[li + 1]);
                if (link == LinksConstants.Null)
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

        private ulong CreateBalancedVariantCore0(params ulong[] sequence)
        {
            do
            {
                if (sequence.Length == 2)
                    return Links.CreateCore(sequence[0], sequence[1]);

                var innerSequence = new ulong[sequence.Length / 2 + sequence.Length % 2];

                for (var i = 0; i < sequence.Length; i += 2)
                    innerSequence[i / 2] = i + 1 == sequence.Length ? sequence[i] : Links.CreateCore(sequence[i], sequence[i + 1]);

                sequence = innerSequence;
            } while (true);
        }

        public HashSet<ulong> Each1(params ulong[] sequence)
        {
            var visitedLinks = new HashSet<ulong>(); // Заменить на bitstring

            EachCore1(link =>
            {
                if (!visitedLinks.Contains(link)) visitedLinks.Add(link); // изучить почему случаются повторы
                return true;
            }, sequence);

            return visitedLinks;
        }

        private void EachCore1(Func<ulong, bool> handler, params ulong[] sequence)
        {
            if (sequence.Length == 2)
            {
                Links.EachCore(sequence[0], sequence[1], handler);
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

                    Links.EachCore(left, right, pair =>
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

                        EachCore1(handler, innerSequence);

                        return LinksConstants.Continue;
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
            if (sequence.IsNullOrEmpty())
                return;

            Links.EnsureEachLinkIsAnyOrExists(sequence);

            if (sequence.Length == 1)
            {
                var link = sequence[0];

                if (link > 0)
                    handler(link);
                else
                    Links.Each(0, 0, handler);
            }
            else if (sequence.Length == 2)
            {
                //_links.Each(sequence[0], sequence[1], handler);

                //  o_|      x_o ... 
                // x_|        |___|

                Links.Each(sequence[1], 0, pair =>
                {
                    var match = Links.SearchCore(sequence[0], pair);
                    if (match != 0)
                        handler(match);
                    return true;
                });

                // |_x      ... x_o
                //  |_o      |___|

                Links.Each(0, sequence[0], pair =>
                {
                    var match = Links.SearchCore(pair, sequence[1]);
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
            Links.EachCore(0, left, pair =>
            {
                StepRight(handler, pair, right);

                if (left != pair)
                    PartialStepRight(handler, pair, right);

                return true;
            });
        }

        private void StepRight(Action<ulong> handler, ulong left, ulong right)
        {
            Links.EachCore(left, 0, rightStep =>
            {
                TryStepRightUp(handler, right, rightStep);
                return true;
            });
        }

        private void TryStepRightUp(Action<ulong> handler, ulong right, ulong stepFrom)
        {
            var upStep = stepFrom;
            var firstSource = Links.GetTargetCore(upStep);
            while (firstSource != right && firstSource != upStep)
            {
                upStep = firstSource;
                firstSource = Links.GetSourceCore(upStep);
            }

            if (firstSource == right)
                handler(stepFrom);
        }

        // TODO: Test
        private void PartialStepLeft(Action<ulong> handler, ulong left, ulong right)
        {
            Links.EachCore(right, 0, pair =>
            {
                StepLeft(handler, left, pair);

                if (right != pair)
                    PartialStepLeft(handler, left, pair);

                return true;
            });
        }

        private void StepLeft(Action<ulong> handler, ulong left, ulong right)
        {
            Links.EachCore(0, right, leftStep =>
            {
                TryStepLeftUp(handler, left, leftStep);
                return true;
            });
        }

        private void TryStepLeftUp(Action<ulong> handler, ulong left, ulong stepFrom)
        {
            var upStep = stepFrom;
            var firstTarget = Links.GetSourceCore(upStep);
            while (firstTarget != left && firstTarget != upStep)
            {
                upStep = firstTarget;
                firstTarget = Links.GetTargetCore(upStep);
            }

            if (firstTarget == left)
                handler(stepFrom);
        }

        private bool StartsWith(ulong sequence, ulong link)
        {
            var upStep = sequence;
            var firstSource = Links.GetSourceCore(upStep);
            while (firstSource != link && firstSource != upStep)
            {
                upStep = firstSource;
                firstSource = Links.GetSourceCore(upStep);
            }
            return firstSource == link;
        }

        private bool EndsWith(ulong sequence, ulong link)
        {
            var upStep = sequence;
            var lastTarget = Links.GetTargetCore(upStep);
            while (lastTarget != link && lastTarget != upStep)
            {
                upStep = lastTarget;
                lastTarget = Links.GetTargetCore(upStep);
            }
            return lastTarget == link;
        }

        public List<ulong> GetAllMatchingSequences0(params ulong[] sequence)
        {
            return Sync.ExecuteReadOperation(() =>
            {
                var results = new List<ulong>();

                if (sequence.Length > 0)
                {
                    Links.EnsureEachLinkExists(sequence);

                    var firstElement = sequence[0];

                    if (sequence.Length == 1)
                    {
                        results.Add(firstElement);
                        return results;
                    }
                    if (sequence.Length == 2)
                    {
                        var pair = Links.SearchCore(firstElement, sequence[1]);
                        if (pair != LinksConstants.Null)
                            results.Add(pair);
                        return results;
                    }

                    var linksInSequence = new HashSet<ulong>(sequence);

                    Action<ulong> handler = result =>
                    {
                        var filterPosition = 0;

                        StopableSequenceWalker.WalkRight(result, Links.GetSourceCore, Links.GetTargetCore,
                            x => linksInSequence.Contains(x) || Links.GetTargetCore(x) == x, x =>
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
            return Sync.ExecuteReadOperation(() =>
            {
                var results = new HashSet<ulong>();

                if (sequence.Length > 0)
                {
                    Links.EnsureEachLinkExists(sequence);

                    var firstElement = sequence[0];

                    if (sequence.Length == 1)
                    {
                        results.Add(firstElement);
                        return results;
                    }
                    if (sequence.Length == 2)
                    {
                        var pair = Links.SearchCore(firstElement, sequence[1]);
                        if (pair != LinksConstants.Null)
                            results.Add(pair);
                        return results;
                    }

                    var matcher = new Matcher(this, sequence, results, null);

                    if (sequence.Length >= 2)
                        StepRight((Action<ulong>)matcher.AddFullMatchedToResults, sequence[0], sequence[1]);

                    var last = sequence.Length - 2;
                    for (var i = 1; i < last; i++)
                        PartialStepRight((Action<ulong>)matcher.AddFullMatchedToResults, sequence[i], sequence[i + 1]);

                    if (sequence.Length >= 3)
                        StepLeft((Action<ulong>)matcher.AddFullMatchedToResults, sequence[sequence.Length - 2], sequence[sequence.Length - 1]);
                }

                return results;
            });
        }

        public const int MaxSequenceFormatSize = 200;

        public string FormatSequence(LinkIndex sequenceLink, params LinkIndex[] knownElements)
        {
            return FormatSequence(sequenceLink, (sb, x) => sb.Append(x), true, knownElements);
        }

        public string FormatSequence(LinkIndex sequenceLink, Action<StringBuilder, LinkIndex> elementToString, bool insertComma, params LinkIndex[] knownElements)
        {
            var visitedElements = 0;

            var linksInSequence = new HashSet<ulong>(knownElements);

            var sb = new StringBuilder();

            sb.Append('{');

            if (Links.Exists(sequenceLink))
            {
                StopableSequenceWalker.WalkRight(sequenceLink, Links.GetSourceCore, Links.GetTargetCore,
                    x => linksInSequence.Contains(x) || Links.GetTargetCore(x) == x, element =>
                    {
                        if (insertComma && visitedElements > 0)
                            sb.Append(',');

                        elementToString(sb, element);

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

            sb.Append('}');

            return sb.ToString();
        }

        public List<ulong> GetAllPartiallyMatchingSequences0(params ulong[] sequence)
        {
            return Sync.ExecuteReadOperation(() =>
            {
                if (sequence.Length > 0)
                {
                    Links.EnsureEachLinkExists(sequence);

                    var results = new HashSet<ulong>();

                    for (var i = 0; i < sequence.Length; i++)
                        AllUsagesCore(sequence[i], results);

                    var filteredResults = new List<ulong>();

                    var linksInSequence = new HashSet<ulong>(sequence);

                    foreach (var result in results)
                    {
                        var filterPosition = -1;

                        StopableSequenceWalker.WalkRight(result, Links.GetSourceCore, Links.GetTargetCore,
                            x => linksInSequence.Contains(x) || Links.GetTargetCore(x) == x, x =>
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
            return Sync.ExecuteReadOperation(() =>
            {
                if (sequence.Length > 0)
                {
                    Links.EnsureEachLinkExists(sequence);

                    var results = new HashSet<ulong>();
                    for (var i = 0; i < sequence.Length; i++)
                        AllUsagesCore(sequence[i], results);

                    var filteredResults = new HashSet<ulong>();
                    var matcher = new Matcher(this, sequence, filteredResults, null);
                    matcher.AddAllPartialMatchedToResults(results);
                    return filteredResults;
                }

                return new HashSet<ulong>();
            });
        }

        public bool GetAllPartiallyMatchingSequences2(Func<ulong, bool> handler, params ulong[] sequence)
        {
            return Sync.ExecuteReadOperation(() =>
            {
                if (sequence.Length > 0)
                {
                    Links.EnsureEachLinkExists(sequence);

                    var results = new HashSet<ulong>();
                    var filteredResults = new HashSet<ulong>();
                    var matcher = new Matcher(this, sequence, filteredResults, handler);
                    for (var i = 0; i < sequence.Length; i++)
                        if (!AllUsagesCore1(sequence[i], results, matcher.HandlePartialMatched))
                            return false;

                    return true;
                }

                return true;
            });
        }

        //public HashSet<ulong> GetAllPartiallyMatchingSequences3(params ulong[] sequence)
        //{
        //    return Sync.ExecuteReadOperation(() =>
        //    {
        //        if (sequence.Length > 0)
        //        {
        //            _links.EnsureEachLinkIsAnyOrExists(sequence);

        //            var firstResults = new HashSet<ulong>();
        //            var lastResults = new HashSet<ulong>();

        //            var first = sequence.First(x => x != LinksConstants.Any);
        //            var last = sequence.Last(x => x != LinksConstants.Any);

        //            AllUsagesCore(first, firstResults);
        //            AllUsagesCore(last, lastResults);

        //            firstResults.IntersectWith(lastResults);

        //            //for (var i = 0; i < sequence.Length; i++)
        //            //    AllUsagesCore(sequence[i], results);

        //            var filteredResults = new HashSet<ulong>();
        //            var matcher = new Matcher(this, sequence, filteredResults, null);
        //            matcher.AddAllPartialMatchedToResults(firstResults);
        //            return filteredResults;
        //        }

        //        return new HashSet<ulong>();
        //    });
        //}

        public HashSet<ulong> GetAllPartiallyMatchingSequences3(params ulong[] sequence)
        {
            return Sync.ExecuteReadOperation(() =>
            {
                if (sequence.Length > 0)
                {
                    Links.EnsureEachLinkIsAnyOrExists(sequence);

                    var firstResults = new HashSet<ulong>();
                    var lastResults = new HashSet<ulong>();

                    var first = sequence.First(x => x != LinksConstants.Any);
                    var last = sequence.Last(x => x != LinksConstants.Any);

                    AllUsagesCore(first, firstResults);
                    AllUsagesCore(last, lastResults);

                    firstResults.IntersectWith(lastResults);

                    //for (var i = 0; i < sequence.Length; i++)
                    //    AllUsagesCore(sequence[i], results);

                    var filteredResults = new HashSet<ulong>();
                    var matcher = new Matcher(this, sequence, filteredResults, null);
                    matcher.AddAllPartialMatchedToResults(firstResults);
                    return filteredResults;
                }

                return new HashSet<ulong>();
            });
        }

        public List<ulong> GetAllPartiallyMatchingSequences(params ulong[] sequence)
        {
            return Sync.ExecuteReadOperation(() =>
            {
                if (sequence.Length > 0)
                {
                    Links.EnsureEachLinkExists(sequence);

                    //var firstElement = sequence[0];

                    //if (sequence.Length == 1)
                    //{
                    //    //results.Add(firstElement);
                    //    return results;
                    //}
                    //if (sequence.Length == 2)
                    //{
                    //    //var pair = _links.SearchCore(firstElement, sequence[1]);
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

        /// <remarks>
        /// TODO: Может потробоваться ограничение на уровень глубины рекурсии
        /// </remarks>
        public HashSet<ulong> AllUsages(ulong link)
        {
            return Sync.ExecuteReadOperation(() =>
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
            Links.EachCore(link, 0, handler);
            Links.EachCore(0, link, handler);
        }

        private bool AllUsagesCore1(ulong link, HashSet<ulong> usages, Func<ulong, bool> outerHandler)
        {
            Func<ulong, bool> handler = pair =>
            {
                if (usages.Add(pair))
                {
                    if (!outerHandler(pair))
                        return false;

                    if (!AllUsagesCore1(pair, usages, outerHandler))
                        return false;
                }
                return true;
            };
            return Links.EachCore(link, 0, handler)
                && Links.EachCore(0, link, handler);
        }

        public void CalculateAllUsages(ulong[] totals)
        {
            var calculator = new AllUsagesCalculator(Links, totals);
            calculator.Calculate();
        }

        public void CalculateAllUsages2(ulong[] totals)
        {
            var calculator = new AllUsagesCalculator2(Links, totals);
            calculator.Calculate();
        }

        private class AllUsagesCalculator
        {
            private readonly Links _links;
            private readonly ulong[] _totals;

            public AllUsagesCalculator(Links links, ulong[] totals)
            {
                _links = links;
                _totals = totals;
            }

            public void Calculate()
            {
                _links.Each(0, 0, CalculateCore);
            }

            private bool CalculateCore(ulong link)
            {
                if (_totals[link] == 0)
                {
                    var total = 1UL;
                    _totals[link] = total;

                    var visitedChildren = new HashSet<ulong>();

                    Func<ulong, bool> linkCalculator = child =>
                    {
                        if (link != child && visitedChildren.Add(child))
                            total += _totals[child] == 0 ? 1 : _totals[child];
                        return true;
                    };

                    _links.EachCore(link, 0, linkCalculator);
                    _links.EachCore(0, link, linkCalculator);

                    _totals[link] = total;
                }
                return true;
            }
        }

        private class AllUsagesCalculator2
        {
            private readonly Links _links;
            private readonly ulong[] _totals;

            public AllUsagesCalculator2(Links links, ulong[] totals)
            {
                _links = links;
                _totals = totals;
            }

            public void Calculate()
            {
                _links.Each(0, 0, CalculateCore);
            }

            private bool IsElement(ulong link)
            {
                //_linksInSequence.Contains(link) || 

                return _links.GetTargetCore(link) == link || _links.GetSourceCore(link) == link;
            }

            private bool CalculateCore(ulong link)
            {
                // TODO: Проработать защиту от зацикливания

                // Основано на SequenceWalker.WalkLeft

                Func<ulong, ulong> getSource = _links.GetSourceCore;
                Func<ulong, ulong> getTarget = _links.GetTargetCore;
                Func<ulong, bool> isElement = IsElement;
                Action<ulong> visitLeaf = (parent) =>
                {
                    if (link != parent)
                        _totals[parent]++;
                };
                Action<ulong> visitNode = (parent) =>
                {
                    if (link != parent)
                        _totals[parent]++;
                };

                var stack = new Stack<ulong>();
                var element = link;

                if (isElement(element))
                    visitLeaf(element);
                else
                    while (true)
                    {
                        if (isElement(element))
                        {
                            if (stack.Count == 0)
                                break;

                            element = stack.Pop();

                            var source = getSource(element);
                            var target = getTarget(element);

                            // Обработка элемента
                            if (isElement(target)) visitLeaf(target);
                            if (isElement(source)) visitLeaf(source);

                            element = source;
                        }
                        else
                        {
                            stack.Push(element);

                            visitNode(element);

                            element = getTarget(element);
                        }
                    }

                _totals[link]++;

                return true;
            }
        }

        private class AllUsagesCollector
        {
            private readonly Links _links;
            private readonly HashSet<ulong> _usages;

            public AllUsagesCollector(Links links, HashSet<ulong> usages)
            {
                _links = links;
                _usages = usages;
            }

            public bool Collect(ulong link)
            {
                if (_usages.Add(link))
                {
                    _links.EachCore(link, 0, Collect);
                    _links.EachCore(0, link, Collect);
                }
                return true;
            }
        }

        private class AllUsagesCollector2
        {
            private readonly Links _links;
            private readonly BitString _usages;

            public AllUsagesCollector2(Links links, BitString usages)
            {
                _links = links;
                _usages = usages;
            }

            public bool Collect(ulong link)
            {
                if (!_usages.GetCore((long)link))
                {
                    _usages.SetCore((long)link);

                    _links.EachCore(link, 0, Collect);
                    _links.EachCore(0, link, Collect);
                }
                return true;
            }
        }

        private class AllUsagesIntersectingCollector
        {
            private readonly Links _links;
            private readonly HashSet<ulong> _intersectWith;
            private readonly HashSet<ulong> _usages;
            private readonly HashSet<ulong> _enter;

            public AllUsagesIntersectingCollector(Links links, HashSet<ulong> intersectWith, HashSet<ulong> usages)
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

                    _links.EachCore(link, 0, Collect);
                    _links.EachCore(0, link, Collect);
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
            var pair = Links.SearchCore(left, right);
            if (pair != LinksConstants.Null)
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

                    StepRight((Action<ulong>)matchings.AddAndReturnVoid, previousMatching, secondLinkUsage);
                    TryStepRightUp((Action<ulong>)matchings.AddAndReturnVoid, secondLinkUsage, previousMatching);
                    //PartialStepRight(matchings.AddAndReturnVoid, secondLinkUsage, sequence[startAt]); // почему-то эта ошибочная запись приводит к желаемым результам.
                    PartialStepRight((Action<ulong>)matchings.AddAndReturnVoid, previousMatching, secondLinkUsage);
                }
            }

            if (matchings.Count == 0)
                return matchings;

            return GetAllPartiallyMatchingSequencesCore(sequence, matchings, startAt + 1); // ??
        }

        private static void EnsureEachLinkIsAnyOrZeroOrManyOrExists(Links links, params ulong[] sequence)
        {
            if (sequence == null)
                return;

            for (var i = 0; i < sequence.Length; i++)
                if (sequence[i] != LinksConstants.Null && sequence[i] != ZeroOrMany && !links.Exists(sequence[i]))
                    throw new ArgumentLinkDoesNotExistsException<ulong>(sequence[i],
                        string.Format("patternSequence[{0}]", i));
        }

        // Pattern Matching -> Key To Triggers
        public HashSet<ulong> MatchPattern(params ulong[] patternSequence)
        {
            return Sync.ExecuteReadOperation(() =>
            {
                patternSequence = Simplify(patternSequence);

                if (patternSequence.Length > 0)
                {
                    EnsureEachLinkIsAnyOrZeroOrManyOrExists(Links, patternSequence);

                    var uniqueSequenceElements = new HashSet<ulong>();
                    for (var i = 0; i < patternSequence.Length; i++)
                        if (patternSequence[i] != LinksConstants.Null && patternSequence[i] != ZeroOrMany)
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
            return Sync.ExecuteReadOperation(() =>
            {
                var results = new HashSet<ulong>();

                if (linksToConnect.Length > 0)
                {
                    Links.EnsureEachLinkExists(linksToConnect);

                    AllUsagesCore(linksToConnect[0], results);

                    for (var i = 1; i < linksToConnect.Length; i++)
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
            return Sync.ExecuteReadOperation(() =>
            {
                var results = new HashSet<ulong>();

                if (linksToConnect.Length > 0)
                {
                    Links.EnsureEachLinkExists(linksToConnect);

                    var collector1 = new AllUsagesCollector(Links, results);
                    collector1.Collect(linksToConnect[0]);

                    for (var i = 1; i < linksToConnect.Length; i++)
                    {
                        var next = new HashSet<ulong>();
                        var collector = new AllUsagesCollector(Links, next);
                        collector.Collect(linksToConnect[i]);

                        results.IntersectWith(next);
                    }
                }

                return results;
            });
        }

        public HashSet<ulong> GetAllConnections2(params ulong[] linksToConnect)
        {
            return Sync.ExecuteReadOperation(() =>
            {
                var results = new HashSet<ulong>();

                if (linksToConnect.Length > 0)
                {
                    Links.EnsureEachLinkExists(linksToConnect);

                    var collector1 = new AllUsagesCollector(Links, results);
                    collector1.Collect(linksToConnect[0]);

                    //AllUsagesCore(linksToConnect[0], results);

                    for (var i = 1; i < linksToConnect.Length; i++)
                    {
                        var next = new HashSet<ulong>();
                        var collector = new AllUsagesIntersectingCollector(Links, results, next);
                        collector.Collect(linksToConnect[i]);

                        //AllUsagesCore(linksToConnect[i], next);

                        //results.IntersectWith(next);

                        results = next;
                    }
                }

                return results;
            });
        }

        public List<ulong> GetAllConnections3(params ulong[] linksToConnect)
        {
            return Sync.ExecuteReadOperation(() =>
            {
                var results = new BitString((long)Links.CountCore() + 1); // new BitArray((int)_links.Total + 1);

                if (linksToConnect.Length > 0)
                {
                    Links.EnsureEachLinkExists(linksToConnect);

                    var collector1 = new AllUsagesCollector2(Links, results);
                    collector1.Collect(linksToConnect[0]);

                    for (var i = 1; i < linksToConnect.Length; i++)
                    {
                        var next = new BitString((long)Links.CountCore() + 1); //new BitArray((int)_links.Total + 1);
                        var collector = new AllUsagesCollector2(Links, next);
                        collector.Collect(linksToConnect[i]);

                        results = results.And(next);
                    }
                }

                return results.GetSetUInt64Indices();
            });
        }

        private static ulong[] Simplify(ulong[] sequence)
        {
            // Считаем новый размер последовательности
            long newLength = 0;
            var zeroOrManyStepped = false;
            for (var i = 0; i < sequence.Length; i++)
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
            for (var i = 0; i < sequence.Length; i++)
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

            var leftBound = 0;
            var rightBound = links.Length - 1;

            var left = links[leftBound++];
            var right = links[rightBound--];

            var results = new List<ulong>();
            CollectMatchingSequences(left, leftBound, links, right, rightBound, ref results);
            return results;
        }

        private void CollectMatchingSequences(ulong leftLink, int leftBound, ulong[] middleLinks, ulong rightLink, int rightBound, ref List<ulong> results)
        {
            var leftLinkTotalReferers = Links.CountCore(leftLink);
            var rightLinkTotalReferers = Links.CountCore(rightLink);

            if (leftLinkTotalReferers <= rightLinkTotalReferers)
            {
                var nextLeftLink = middleLinks[leftBound];

                var elements = GetRightElements(leftLink, nextLeftLink);
                if (leftBound <= rightBound)
                {
                    for (var i = elements.Length - 1; i >= 0; i--)
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
                    for (var i = elements.Length - 1; i >= 0; i--)
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

                var elements = GetLeftElements(rightLink, nextRightLink);

                if (leftBound <= rightBound)
                {
                    for (var i = elements.Length - 1; i >= 0; i--)
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
                    for (var i = elements.Length - 1; i >= 0; i--)
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

            Links.Each(0, startLink, couple =>
            {
                if (couple != startLink)
                    if (TryStepRight(couple, rightLink, result, 2))
                        return false;

                return true;
            });

            if (Links.GetTarget(Links.GetTarget(startLink)) == rightLink)
                result[4] = startLink;

            return result;
        }

        public bool TryStepRight(ulong startLink, ulong rightLink, ulong[] result, int offset)
        {
            var added = 0;

            Links.Each(startLink, 0, couple =>
            {
                if (couple != startLink)
                {
                    var coupleTarget = Links.GetTarget(couple);
                    if (coupleTarget == rightLink)
                    {
                        result[offset] = couple;
                        if (++added == 2)
                            return false;
                    }
                    else if (Links.GetSource(coupleTarget) == rightLink) // coupleTarget.Linker == Net.And &&
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

            Links.Each(startLink, 0, couple =>
            {
                if (couple != startLink)
                    if (TryStepLeft(couple, leftLink, result, 2))
                        return false;

                return true;
            });

            if (Links.GetSource(Links.GetSource(leftLink)) == startLink)
                result[4] = leftLink;

            return result;
        }

        public bool TryStepLeft(ulong startLink, ulong leftLink, ulong[] result, int offset)
        {
            var added = 0;

            Links.Each(0, startLink, couple =>
            {
                if (couple != startLink)
                {
                    var coupleSource = Links.GetSource(couple);
                    if (coupleSource == leftLink)
                    {
                        result[offset] = couple;
                        if (++added == 2)
                            return false;
                    }
                    else if (Links.GetTarget(coupleSource) == leftLink) // coupleSource.Linker == Net.And &&
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
                _linksInSequence = new HashSet<LinkIndex>(patternSequence.Where(x => x != LinksConstants.Null && x != ZeroOrMany));
                _results = results;

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
                        if (_patternSequence[i] == LinksConstants.Any)
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
                        if (_patternSequence[i] == LinksConstants.Any)
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
                        if (_patternSequence[i] == LinksConstants.Any)
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
