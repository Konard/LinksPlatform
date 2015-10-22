using System;
using System.Collections.Generic;
using System.Text;
using Platform.Data.Core.Sequences;

namespace Platform.Data.Core.Triplets
{
    public class SequenceHelpers
    {
        public const int MaxSequenceFormatSize = 20;

        //public static void DeleteSequence(Link sequence)
        //{
        //}

        public static string FormatSequence(Link sequence)
        {
            int visitedElements = 0;

            StringBuilder sb = new StringBuilder();

            sb.Append('[');

            var walker = StopableSequenceWalker.WalkRight(sequence, x => x.Source, x => x.Target, x => x.Linker != Net.And, element =>
            {
                if (visitedElements > 0)
                    sb.Append(',');

                sb.Append(element.ToString());

                visitedElements++;

                if (visitedElements < MaxSequenceFormatSize)
                {
                    return true;
                }
                else
                {
                    sb.Append(", ...");
                    return false;
                }
            });

            sb.Append(']');

            return sb.ToString();
        }

        public static List<Link> CollectMatchingSequences(Link[] links)
        {
            if (links.Length == 1)
            {
                throw new Exception("Подпоследовательности с одним элементом не поддерживаются.");
            }

            int leftBound = 0;
            int rightBound = links.Length - 1;

            Link left = links[leftBound++];
            Link right = links[rightBound--];

            List<Link> results = new List<Link>();
            CollectMatchingSequences(left, leftBound, links, right, rightBound, ref results);
            return results;
        }

        private static void CollectMatchingSequences(Link leftLink, int leftBound, Link[] middleLinks, Link rightLink, int rightBound, ref List<Link> results)
        {
            long leftLinkTotalReferers = leftLink.ReferersBySourceCount + leftLink.ReferersByTargetCount;
            long rightLinkTotalReferers = rightLink.ReferersBySourceCount + rightLink.ReferersByTargetCount;

            if (leftLinkTotalReferers <= rightLinkTotalReferers)
            {
                var nextLeftLink = middleLinks[leftBound];

                Link[] elements = GetRightElements(leftLink, nextLeftLink);
                if (leftBound <= rightBound)
                {
                    for (int i = elements.Length - 1; i >= 0; i--)
                    {
                        var element = elements[i];
                        if (element != null)
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
                        if (element != null)
                        {
                            results.Add(element);
                        }
                    }
                }
            }
            else
            {
                var nextRightLink = middleLinks[rightBound];

                Link[] elements = GetLeftElements(rightLink, nextRightLink);

                if (leftBound <= rightBound)
                {
                    for (int i = elements.Length - 1; i >= 0; i--)
                    {
                        var element = elements[i];
                        if (element != null)
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
                        if (element != null)
                        {
                            results.Add(element);
                        }
                    }
                }
            }
        }

        public static Link[] GetRightElements(Link startLink, Link rightLink)
        {
            var result = new Link[4];

            TryStepRight(startLink, rightLink, result, 0);

            startLink.WalkThroughReferersByTarget(couple =>
                {
                    if (couple.Linker == Net.And)
                        if (TryStepRight(couple, rightLink, result, 2))
                            return Link.Stop;

                    return Link.Continue;
                });

            return result;
        }

        public static bool TryStepRight(Link startLink, Link rightLink, Link[] result, int offset)
        {
            int added = 0;

            startLink.WalkThroughReferersBySource(couple =>
                {
                    if (couple.Linker == Net.And)
                    {
                        var coupleTarget = couple.Target;
                        if (coupleTarget == rightLink)
                        {
                            result[offset] = couple;
                            if (++added == 2)
                                return Link.Stop;
                        }
                        else if (coupleTarget.Linker == Net.And && coupleTarget.Source == rightLink)
                        {
                            result[offset + 1] = couple;
                            if (++added == 2)
                                return Link.Stop;
                        }
                    }

                    return Link.Continue;
                });

            return added > 0;
        }

        public static Link[] GetLeftElements(Link startLink, Link leftLink)
        {
            var result = new Link[4];

            TryStepLeft(startLink, leftLink, result, 0);

            startLink.WalkThroughReferersBySource(couple =>
                {
                    if (couple.Linker == Net.And)
                        if (TryStepLeft(couple, leftLink, result, 2))
                            return Link.Stop;

                    return Link.Continue;
                });

            return result;
        }

        public static bool TryStepLeft(Link startLink, Link leftLink, Link[] result, int offset)
        {
            int added = 0;

            startLink.WalkThroughReferersByTarget(couple =>
                {
                    if (couple.Linker == Net.And)
                    {
                        var coupleSource = couple.Source;
                        if (coupleSource == leftLink)
                        {
                            result[offset] = couple;
                            if (++added == 2)
                                return Link.Stop;
                        }
                        else if (coupleSource.Linker == Net.And && coupleSource.Target == leftLink)
                        {
                            result[offset + 1] = couple;
                            if (++added == 2)
                                return Link.Stop;
                        }
                    }

                    return Link.Continue;
                });

            return added > 0;
        }
    }
}
