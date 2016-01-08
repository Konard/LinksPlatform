using System;
using System.Runtime.CompilerServices;
using Platform.Data.Core.Collections.Trees;

namespace Platform.Data.Core.Pairs
{
    unsafe partial class LinksMemoryManager
    {
        private abstract class LinksTreeMethodsBase : SizeBalancedTreeMethods2
        {
            protected readonly Link* Links;
            protected readonly LinksHeader* Header;

            protected LinksTreeMethodsBase(LinksMemoryManager links, LinksHeader* header)
            {
                Links = links._links;
                Header = header;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected abstract ulong GetTreeRoot();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected abstract ulong GetBasePartValue(ulong link);

            public ulong CalculateReferences(ulong link)
            {
                var root = GetTreeRoot();
                var total = GetSize(root);

                var totalRightIgnore = 0UL;

                while (root != 0)
                {
                    var @base = GetBasePartValue(root);

                    if (@base <= link)
                        root = *GetRight(root);
                    else
                    {
                        var rootRight = *GetRight(root);
                        totalRightIgnore += (rootRight != 0 ? GetSize(rootRight) : 0) + 1;

                        root = *GetLeft(root);
                    }
                }

                root = GetTreeRoot();

                var totalLeftIgnore = 0UL;

                while (root != 0)
                {
                    var @base = GetBasePartValue(root);

                    if (@base >= link)
                        root = *GetLeft(root);
                    else
                    {
                        var rootLeft = *GetLeft(root);
                        totalLeftIgnore += (rootLeft != 0 ? GetSize(rootLeft) : 0) + 1;

                        root = *GetRight(root);
                    }
                }

                return total - totalRightIgnore - totalLeftIgnore;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool EachReference(ulong source, Func<ulong, bool> handler)
            {
                return EachReferenceCore(source, GetTreeRoot(), handler);
            }

            // TODO: 1. Move target, handler to separate object. 2. Use stack or walker
            private bool EachReferenceCore(ulong @base, ulong link, Func<ulong, bool> handler)
            {
                if (link == 0)
                    return true;

                var linkBase = GetBasePartValue(link);

                if (linkBase > @base)
                {
                    if (EachReferenceCore(@base, *GetLeft(link), handler) == LinksConstants.Break)
                        return false;
                }
                else if (linkBase < @base)
                {
                    if (EachReferenceCore(@base, *GetRight(link), handler) == LinksConstants.Break)
                        return false;
                }
                else //if (linkSource == source)
                {
                    if (handler(link) == LinksConstants.Break)
                        return false;

                    if (EachReferenceCore(@base, *GetLeft(link), handler) == LinksConstants.Break)
                        return false;
                    if (EachReferenceCore(@base, *GetRight(link), handler) == LinksConstants.Break)
                        return false;
                }

                return true;
            }
        }

        private class LinksSourcesTreeMethods : LinksTreeMethodsBase
        {
            public LinksSourcesTreeMethods(LinksMemoryManager links, LinksHeader* header)
                : base(links, header)
            {
            }

            protected override ulong* GetLeft(ulong node)
            {
                return &Links[node].LeftAsSource;
            }

            protected override ulong* GetRight(ulong node)
            {
                return &Links[node].RightAsSource;
            }

            protected override ulong GetSize(ulong node)
            {
                return Links[node].SizeAsSource;
            }

            protected override void SetLeft(ulong node, ulong left)
            {
                Links[node].LeftAsSource = left;
            }

            protected override void SetRight(ulong node, ulong right)
            {
                Links[node].RightAsSource = right;
            }

            protected override void SetSize(ulong node, ulong size)
            {
                Links[node].SizeAsSource = size;
            }

            protected override bool FirstIsToTheLeftOfSecond(ulong first, ulong second)
            {
                return Links[first].Source < Links[second].Source ||
                       (Links[first].Source == Links[second].Source && Links[first].Target < Links[second].Target);
            }

            protected override bool FirstIsToTheRightOfSecond(ulong first, ulong second)
            {
                return Links[first].Source > Links[second].Source ||
                       (Links[first].Source == Links[second].Source && Links[first].Target > Links[second].Target);
            }

            protected override ulong GetTreeRoot()
            {
                return Header->FirstAsSource;
            }

            protected override ulong GetBasePartValue(ulong link)
            {
                return Links[link].Source;
            }

            /// <summary>
            /// Выполняет поиск и возвращает индекс связи с указанными Source (началом) и Target (концом)
            /// по дереву (индексу) связей, отсортированному по Source, а затем по Target.
            /// </summary>
            /// <param name="source">Индекс связи, которая является началом на искомой связи.</param>
            /// <param name="target">Индекс связи, которая является концом на искомой связи.</param>
            /// <returns>Индекс искомой связи.</returns>
            public ulong Search(ulong source, ulong target)
            {
                var root = Header->FirstAsSource;

                while (root != 0)
                {
                    var rootSource = Links[root].Source;
                    var rootTarget = Links[root].Target;

                    if (FirstIsToTheLeftOfSecond(source, target, rootSource, rootTarget)) // node.Key < root.Key
                        root = *GetLeft(root);
                    else if (FirstIsToTheRightOfSecond(source, target, rootSource, rootTarget)) // node.Key > root.Key
                        root = *GetRight(root);
                    else // node.Key == root.Key
                        return root;
                }

                return 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool FirstIsToTheLeftOfSecond(ulong firstSource, ulong firstTarget, ulong secondSource,
                ulong secondTarget)
            {
                return firstSource < secondSource || (firstSource == secondSource && firstTarget < secondTarget);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool FirstIsToTheRightOfSecond(ulong firstSource, ulong firstTarget, ulong secondSource,
                ulong secondTarget)
            {
                return firstSource > secondSource || (firstSource == secondSource && firstTarget > secondTarget);
            }
        }

        private class LinksTargetsTreeMethods : LinksTreeMethodsBase
        {
            public LinksTargetsTreeMethods(LinksMemoryManager links, LinksHeader* header)
                : base(links, header)
            {
            }

            protected override ulong* GetLeft(ulong node)
            {
                return &Links[node].LeftAsTarget;
            }

            protected override ulong* GetRight(ulong node)
            {
                return &Links[node].RightAsTarget;
            }

            protected override ulong GetSize(ulong node)
            {
                return Links[node].SizeAsTarget;
            }

            protected override void SetLeft(ulong node, ulong left)
            {
                Links[node].LeftAsTarget = left;
            }

            protected override void SetRight(ulong node, ulong right)
            {
                Links[node].RightAsTarget = right;
            }

            protected override void SetSize(ulong node, ulong size)
            {
                Links[node].SizeAsTarget = size;
            }

            protected override bool FirstIsToTheLeftOfSecond(ulong first, ulong second)
            {
                return Links[first].Target < Links[second].Target ||
                       (Links[first].Target == Links[second].Target && Links[first].Source < Links[second].Source);
            }

            protected override bool FirstIsToTheRightOfSecond(ulong first, ulong second)
            {
                return Links[first].Target > Links[second].Target ||
                       (Links[first].Target == Links[second].Target && Links[first].Source > Links[second].Source);
            }

            protected override ulong GetTreeRoot()
            {
                return Header->FirstAsTarget;
            }

            protected override ulong GetBasePartValue(ulong link)
            {
                return Links[link].Target;
            }
        }
    }
}