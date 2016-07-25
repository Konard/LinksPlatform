using System;
using System.Runtime.CompilerServices;
using System.Text;
using Platform.Data.Core.Collections.Trees;
using Platform.Helpers;

namespace Platform.Data.Core.Pairs
{
    unsafe partial class UInt64LinksMemoryManager
    {
        private abstract class LinksTreeMethodsBase : SizedAndThreadedAVLBalancedTreeMethods<ulong>
        {
            private readonly ILinksCombinedConstants<bool, ulong, int> _constants;
            protected readonly Link* Links;
            protected readonly LinksHeader* Header;

            protected LinksTreeMethodsBase(Link* links, LinksHeader* header, ILinksCombinedConstants<bool, ulong, int> constants)
            {
                Links = links;
                Header = header;
                _constants = constants;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected abstract ulong GetTreeRoot();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected abstract ulong GetBasePartValue(ulong link);

            public ulong this[ulong index]
            {
                get
                {
                    var root = GetTreeRoot();
                    if (index >= GetSize(root))
                        return 0;

                    while (root != 0)
                    {
                        var left = GetLeftOrDefault(root);
                        var leftSize = GetSizeOrZero(left);
                        if (index < leftSize)
                        {
                            root = left;
                            continue;
                        }

                        if (index == leftSize)
                            return root;

                        root = GetRightOrDefault(root);
                        index -= (leftSize + 1);
                    }
                    return 0; // TODO: Impossible situation exception (only if tree structure broken)
                }
            }

            // TODO: Return indices range instead of references count
            public ulong CalculateReferences(ulong link)
            {
                var root = GetTreeRoot();
                var total = GetSize(root);

                var totalRightIgnore = 0UL;

                while (root != 0)
                {
                    var @base = GetBasePartValue(root);

                    if (@base <= link)
                        root = GetRightOrDefault(root);
                    else
                    {
                        totalRightIgnore += GetRightSize(root) + 1;

                        root = GetLeftOrDefault(root);
                    }
                }

                root = GetTreeRoot();

                var totalLeftIgnore = 0UL;

                while (root != 0)
                {
                    var @base = GetBasePartValue(root);

                    if (@base >= link)
                        root = GetLeftOrDefault(root);
                    else
                    {
                        totalLeftIgnore += GetLeftSize(root) + 1;

                        root = GetRightOrDefault(root);
                    }
                }

                return total - totalRightIgnore - totalLeftIgnore;
            }

            public bool EachReference(ulong link, Func<ulong, bool> handler)
            {
                var root = GetTreeRoot();

                if (root == 0)
                    return true;

                ulong first = 0, current = root;
                while (current != 0)
                {
                    var @base = GetBasePartValue(current);
                    if (@base >= link)
                    {
                        if (@base == link)
                            first = current;
                        current = GetLeftOrDefault(current);
                    }
                    else
                        current = GetRightOrDefault(current);
                }

                if (first != 0)
                {
                    var breakConstant = _constants.Break;

                    current = first;
                    while (true)
                    {
                        if (handler(current) == breakConstant)
                            return false;
                        current = GetNext(current);
                        if (current == 0 || GetBasePartValue(current) != link)
                            break;
                    }
                }

                return true;
            }

            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            //public bool EachReference(ulong source, Func<ulong, bool> handler)
            //{
            //    return EachReferenceCore(source, GetTreeRoot(), handler);
            //}

            // TODO: 1. Move target, handler to separate object. 2. Use stack or walker 3. Use low-level MSIL stack.
            //private bool EachReferenceCore(ulong @base, ulong link, Func<ulong, bool> handler)
            //{
            //    if (link == 0)
            //        return true;

            //    var linkBase = GetBasePartValue(link);

            //    var breakConstant = _constants.Break;

            //    if (linkBase > @base)
            //    {
            //        if (EachReferenceCore(@base, GetLeftOrDefault(link), handler) == breakConstant)
            //            return false;
            //    }
            //    else if (linkBase < @base)
            //    {
            //        if (EachReferenceCore(@base, GetRightOrDefault(link), handler) == breakConstant)
            //            return false;
            //    }
            //    else //if (linkSource == source)
            //    {
            //        if (handler(link) == breakConstant)
            //            return false;

            //        if (EachReferenceCore(@base, GetLeftOrDefault(link), handler) == breakConstant)
            //            return false;
            //        if (EachReferenceCore(@base, GetRightOrDefault(link), handler) == breakConstant)
            //            return false;
            //    }

            //    return true;
            //}

            protected override void PrintNodeValue(ulong node, StringBuilder sb)
            {
                sb.Append(' ');
                sb.Append(Links[node].Source);
                sb.Append('-');
                sb.Append('>');
                sb.Append(Links[node].Target);
            }
        }

        private class LinksSourcesTreeMethods : LinksTreeMethodsBase
        {
            public LinksSourcesTreeMethods(Link* links, LinksHeader* header, ILinksCombinedConstants<bool, ulong, int> constants)
                : base(links, header, constants)
            {
            }

            //protected override IntPtr GetLeft(ulong node) => new IntPtr(&Links[node].LeftAsSource);

            //protected override IntPtr GetRight(ulong node) => new IntPtr(&Links[node].RightAsSource);

            //protected override ulong GetSize(ulong node) => Links[node].SizeAsSource;

            //protected override void SetLeft(ulong node, ulong left) => Links[node].LeftAsSource = left;

            //protected override void SetRight(ulong node, ulong right) => Links[node].RightAsSource = right;

            //protected override void SetSize(ulong node, ulong size) => Links[node].SizeAsSource = size;

            protected override IntPtr GetLeftPointer(ulong node) => new IntPtr(&Links[node].LeftAsSource);

            protected override IntPtr GetRightPointer(ulong node) => new IntPtr(&Links[node].RightAsSource);

            protected override ulong GetLeftValue(ulong node) => Links[node].LeftAsSource;

            protected override ulong GetRightValue(ulong node) => Links[node].RightAsSource;

            protected override ulong GetSize(ulong node)
            {
                var previousValue = Links[node].SizeAsSource;
                //return MathHelpers.PartialRead(previousValue, 5, -5);
                return (previousValue & 4294967264) >> 5;
            }

            protected override void SetLeft(ulong node, ulong left) => Links[node].LeftAsSource = left;

            protected override void SetRight(ulong node, ulong right) => Links[node].RightAsSource = right;

            protected override void SetSize(ulong node, ulong size)
            {
                var previousValue = Links[node].SizeAsSource;
                //var modified = MathHelpers.PartialWrite(previousValue, size, 5, -5);
                var modified = (previousValue & 31) | ((size & 134217727) << 5);
                Links[node].SizeAsSource = modified;
            }

            protected override bool GetLeftIsChild(ulong node)
            {
                var previousValue = Links[node].SizeAsSource;
                //return (Integer)MathHelpers.PartialRead(previousValue, 4, 1);
                return (previousValue & 16) >> 4 == 1UL;
            }

            protected override void SetLeftIsChild(ulong node, bool value)
            {
                var previousValue = Links[node].SizeAsSource;
                //var modified = MathHelpers.PartialWrite(previousValue, (ulong)(Integer)value, 4, 1);
                var modified = (previousValue & 4294967279) | ((value ? 1UL : 0UL) << 4);
                Links[node].SizeAsSource = modified;
            }

            protected override bool GetRightIsChild(ulong node)
            {
                var previousValue = Links[node].SizeAsSource;
                //return (Integer)MathHelpers.PartialRead(previousValue, 3, 1);
                return (previousValue & 8) >> 3 == 1UL;
            }

            protected override void SetRightIsChild(ulong node, bool value)
            {
                var previousValue = Links[node].SizeAsSource;
                //var modified = MathHelpers.PartialWrite(previousValue, (ulong)(Integer)value, 3, 1);
                var modified = (previousValue & 4294967287) | ((value ? 1UL : 0UL) << 3);
                Links[node].SizeAsSource = modified;
            }

            protected override sbyte GetBalance(ulong node)
            {
                var previousValue = Links[node].SizeAsSource;
                //var value = MathHelpers.PartialRead(previousValue, 0, 3);
                var value = previousValue & 7;
                var unpackedValue = (sbyte)((value & 4) > 0 ? ((value & 4) << 5) | value & 3 | 124 : value & 3);
                return unpackedValue;
            }

            protected override void SetBalance(ulong node, sbyte value)
            {
                var previousValue = Links[node].SizeAsSource;
                var packagedValue = (ulong)((((byte)value >> 5) & 4) | value & 3);
                //var modified = MathHelpers.PartialWrite(previousValue, packagedValue, 0, 3);
                var modified = (previousValue & 4294967288) | (packagedValue & 7);
                Links[node].SizeAsSource = modified;
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
                        root = GetLeftOrDefault(root);
                    else if (FirstIsToTheRightOfSecond(source, target, rootSource, rootTarget)) // node.Key > root.Key
                        root = GetRightOrDefault(root);
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
            public LinksTargetsTreeMethods(Link* links, LinksHeader* header, ILinksCombinedConstants<bool, ulong, int> constants)
                : base(links, header, constants)
            {
            }

            //protected override IntPtr GetLeft(ulong node) => new IntPtr(&Links[node].LeftAsTarget);

            //protected override IntPtr GetRight(ulong node) => new IntPtr(&Links[node].RightAsTarget);

            //protected override ulong GetSize(ulong node) => Links[node].SizeAsTarget;

            //protected override void SetLeft(ulong node, ulong left) => Links[node].LeftAsTarget = left;

            //protected override void SetRight(ulong node, ulong right) => Links[node].RightAsTarget = right;

            //protected override void SetSize(ulong node, ulong size) => Links[node].SizeAsTarget = size;

            protected override IntPtr GetLeftPointer(ulong node) => new IntPtr(&Links[node].LeftAsTarget);

            protected override IntPtr GetRightPointer(ulong node) => new IntPtr(&Links[node].RightAsTarget);

            protected override ulong GetLeftValue(ulong node) => Links[node].LeftAsTarget;

            protected override ulong GetRightValue(ulong node) => Links[node].RightAsTarget;

            protected override ulong GetSize(ulong node)
            {
                var previousValue = Links[node].SizeAsTarget;
                //return MathHelpers.PartialRead(previousValue, 5, -5);
                return (previousValue & 4294967264) >> 5;
            }

            protected override void SetLeft(ulong node, ulong left) => Links[node].LeftAsTarget = left;

            protected override void SetRight(ulong node, ulong right) => Links[node].RightAsTarget = right;

            protected override void SetSize(ulong node, ulong size)
            {
                var previousValue = Links[node].SizeAsTarget;
                //var modified = MathHelpers.PartialWrite(previousValue, size, 5, -5);
                var modified = (previousValue & 31) | ((size & 134217727) << 5);
                Links[node].SizeAsTarget = modified;
            }

            protected override bool GetLeftIsChild(ulong node)
            {
                var previousValue = Links[node].SizeAsTarget;
                //return (Integer)MathHelpers.PartialRead(previousValue, 4, 1);
                return (previousValue & 16) >> 4 == 1UL;

                // TODO: Check if this is possible to use
                //var nodeSize = GetSize(node);
                //var left = GetLeftValue(node);
                //var leftSize = GetSizeOrZero(left);
                //return leftSize > 0 && nodeSize > leftSize;
            }

            protected override void SetLeftIsChild(ulong node, bool value)
            {
                var previousValue = Links[node].SizeAsTarget;
                //var modified = MathHelpers.PartialWrite(previousValue, (ulong)(Integer)value, 4, 1);
                var modified = (previousValue & 4294967279) | ((value ? 1UL : 0UL) << 4);
                Links[node].SizeAsTarget = modified;
            }

            protected override bool GetRightIsChild(ulong node)
            {
                var previousValue = Links[node].SizeAsTarget;
                //return (Integer)MathHelpers.PartialRead(previousValue, 3, 1);
                return (previousValue & 8) >> 3 == 1UL;

                // TODO: Check if this is possible to use
                //var nodeSize = GetSize(node);
                //var right = GetRightValue(node);
                //var rightSize = GetSizeOrZero(right);
                //return rightSize > 0 && nodeSize > rightSize;
            }

            protected override void SetRightIsChild(ulong node, bool value)
            {
                var previousValue = Links[node].SizeAsTarget;
                //var modified = MathHelpers.PartialWrite(previousValue, (ulong)(Integer)value, 3, 1);
                var modified = (previousValue & 4294967287) | ((value ? 1UL : 0UL) << 3);
                Links[node].SizeAsTarget = modified;
            }

            protected override sbyte GetBalance(ulong node)
            {
                var previousValue = Links[node].SizeAsTarget;
                //var value = MathHelpers.PartialRead(previousValue, 0, 3);
                var value = previousValue & 7;
                var unpackedValue = (sbyte)((value & 4) > 0 ? ((value & 4) << 5) | value & 3 | 124 : value & 3);
                return unpackedValue;
            }

            protected override void SetBalance(ulong node, sbyte value)
            {
                var previousValue = Links[node].SizeAsTarget;
                var packagedValue = (ulong)((((byte)value >> 5) & 4) | value & 3);
                //var modified = MathHelpers.PartialWrite(previousValue, packagedValue, 0, 3);
                var modified = (previousValue & 4294967288) | (packagedValue & 7);
                Links[node].SizeAsTarget = modified;
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