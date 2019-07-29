using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Platform.Numbers;
using Platform.Unsafe;
using Platform.Collections.Methods.Trees;
using System.Text;
using Platform.Data.Constants;

namespace Platform.Data.Doublets
{
    partial class ResizableDirectMemoryLinks<TLink>
    {
        private abstract class LinksTreeMethodsBase : SizedAndThreadedAVLBalancedTreeMethods<TLink>
        {
            private readonly ResizableDirectMemoryLinks<TLink> _memory;
            private readonly LinksCombinedConstants<TLink, TLink, int> _constants;
            protected readonly IntPtr Links;
            protected readonly IntPtr Header;

            protected LinksTreeMethodsBase(ResizableDirectMemoryLinks<TLink> memory)
            {
                Links = memory._links;
                Header = memory._header;
                _memory = memory;
                _constants = memory.Constants;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected abstract TLink GetTreeRoot();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected abstract TLink GetBasePartValue(TLink link);

            public TLink this[TLink index]
            {
                get
                {
                    var root = GetTreeRoot();
                    if (GreaterOrEqualThan(index, GetSize(root)))
                    {
                        return GetZero();
                    }
                    while (!EqualToZero(root))
                    {
                        var left = GetLeftOrDefault(root);
                        var leftSize = GetSizeOrZero(left);
                        if (LessThan(index, leftSize))
                        {
                            root = left;
                            continue;
                        }
                        if (IsEquals(index, leftSize))
                        {
                            return root;
                        }
                        root = GetRightOrDefault(root);
                        index = Subtract(index, Increment(leftSize));
                    }
                    return GetZero(); // TODO: Impossible situation exception (only if tree structure broken)
                }
            }

            // TODO: Return indices range instead of references count
            public TLink CalculateReferences(TLink link)
            {
                var root = GetTreeRoot();
                var total = GetSize(root);
                var totalRightIgnore = GetZero();
                while (!EqualToZero(root))
                {
                    var @base = GetBasePartValue(root);
                    if (LessOrEqualThan(@base, link))
                    {
                        root = GetRightOrDefault(root);
                    }
                    else
                    {
                        totalRightIgnore = Add(totalRightIgnore, Increment(GetRightSize(root)));
                        root = GetLeftOrDefault(root);
                    }
                }
                root = GetTreeRoot();
                var totalLeftIgnore = GetZero();
                while (!EqualToZero(root))
                {
                    var @base = GetBasePartValue(root);
                    if (GreaterOrEqualThan(@base, link))
                    {
                        root = GetLeftOrDefault(root);
                    }
                    else
                    {
                        totalLeftIgnore = Add(totalLeftIgnore, Increment(GetLeftSize(root)));

                        root = GetRightOrDefault(root);
                    }
                }
                return Subtract(Subtract(total, totalRightIgnore), totalLeftIgnore);
            }

            public TLink EachReference(TLink link, Func<IList<TLink>, TLink> handler)
            {
                var root = GetTreeRoot();
                if (EqualToZero(root))
                {
                    return _constants.Continue;
                }
                TLink first = GetZero(), current = root;
                while (!EqualToZero(current))
                {
                    var @base = GetBasePartValue(current);
                    if (GreaterOrEqualThan(@base, link))
                    {
                        if (IsEquals(@base, link))
                        {
                            first = current;
                        }
                        current = GetLeftOrDefault(current);
                    }
                    else
                    {
                        current = GetRightOrDefault(current);
                    }
                }
                if (!EqualToZero(first))
                {
                    current = first;
                    while (true)
                    {
                        if (IsEquals(handler(_memory.GetLinkStruct(current)), _constants.Break))
                        {
                            return _constants.Break;
                        }
                        current = GetNext(current);
                        if (EqualToZero(current) || !IsEquals(GetBasePartValue(current), link))
                        {
                            break;
                        }
                    }
                }
                return _constants.Continue;
            }
        }

        private class LinksSourcesTreeMethods : LinksTreeMethodsBase
        {
            public LinksSourcesTreeMethods(ResizableDirectMemoryLinks<TLink> memory)
                : base(memory)
            {
            }

            protected override IntPtr GetLeftPointer(TLink node) => Links.GetElement(LinkSizeInBytes, node) + Link.LeftAsSourceOffset;

            protected override IntPtr GetRightPointer(TLink node) => Links.GetElement(LinkSizeInBytes, node) + Link.RightAsSourceOffset;

            protected override TLink GetLeftValue(TLink node) => (Links.GetElement(LinkSizeInBytes, node) + Link.LeftAsSourceOffset).GetValue<TLink>();

            protected override TLink GetRightValue(TLink node) => (Links.GetElement(LinkSizeInBytes, node) + Link.RightAsSourceOffset).GetValue<TLink>();

            protected override TLink GetSize(TLink node)
            {
                var previousValue = (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsSourceOffset).GetValue<TLink>();
                return BitwiseHelpers.PartialRead(previousValue, 5, -5);
            }

            protected override void SetLeft(TLink node, TLink left) => (Links.GetElement(LinkSizeInBytes, node) + Link.LeftAsSourceOffset).SetValue(left);

            protected override void SetRight(TLink node, TLink right) => (Links.GetElement(LinkSizeInBytes, node) + Link.RightAsSourceOffset).SetValue(right);

            protected override void SetSize(TLink node, TLink size)
            {
                var previousValue = (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsSourceOffset).GetValue<TLink>();
                (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsSourceOffset).SetValue(BitwiseHelpers.PartialWrite(previousValue, size, 5, -5));
            }

            protected override bool GetLeftIsChild(TLink node)
            {
                var previousValue = (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsSourceOffset).GetValue<TLink>();
                return (Integer<TLink>)BitwiseHelpers.PartialRead(previousValue, 4, 1);
            }

            protected override void SetLeftIsChild(TLink node, bool value)
            {
                var previousValue = (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsSourceOffset).GetValue<TLink>();
                var modified = BitwiseHelpers.PartialWrite(previousValue, (TLink)(Integer<TLink>)value, 4, 1);
                (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsSourceOffset).SetValue(modified);
            }

            protected override bool GetRightIsChild(TLink node)
            {
                var previousValue = (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsSourceOffset).GetValue<TLink>();
                return (Integer<TLink>)BitwiseHelpers.PartialRead(previousValue, 3, 1);
            }

            protected override void SetRightIsChild(TLink node, bool value)
            {
                var previousValue = (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsSourceOffset).GetValue<TLink>();
                var modified = BitwiseHelpers.PartialWrite(previousValue, (TLink)(Integer<TLink>)value, 3, 1);
                (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsSourceOffset).SetValue(modified);
            }

            protected override sbyte GetBalance(TLink node)
            {
                var previousValue = (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsSourceOffset).GetValue<TLink>();
                var value = (ulong)(Integer<TLink>)BitwiseHelpers.PartialRead(previousValue, 0, 3);
                var unpackedValue = (sbyte)((value & 4) > 0 ? ((value & 4) << 5) | value & 3 | 124 : value & 3);
                return unpackedValue;
            }

            protected override void SetBalance(TLink node, sbyte value)
            {
                var previousValue = (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsSourceOffset).GetValue<TLink>();
                var packagedValue = (TLink)(Integer<TLink>)((((byte)value >> 5) & 4) | value & 3);
                var modified = BitwiseHelpers.PartialWrite(previousValue, packagedValue, 0, 3);
                (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsSourceOffset).SetValue(modified);
            }

            protected override bool FirstIsToTheLeftOfSecond(TLink first, TLink second)
            {
                var firstSource = (Links.GetElement(LinkSizeInBytes, first) + Link.SourceOffset).GetValue<TLink>();
                var secondSource = (Links.GetElement(LinkSizeInBytes, second) + Link.SourceOffset).GetValue<TLink>();
                return LessThan(firstSource, secondSource) ||
                       (IsEquals(firstSource, secondSource) && LessThan((Links.GetElement(LinkSizeInBytes, first) + Link.TargetOffset).GetValue<TLink>(), (Links.GetElement(LinkSizeInBytes, second) + Link.TargetOffset).GetValue<TLink>()));
            }

            protected override bool FirstIsToTheRightOfSecond(TLink first, TLink second)
            {
                var firstSource = (Links.GetElement(LinkSizeInBytes, first) + Link.SourceOffset).GetValue<TLink>();
                var secondSource = (Links.GetElement(LinkSizeInBytes, second) + Link.SourceOffset).GetValue<TLink>();
                return GreaterThan(firstSource, secondSource) ||
                       (IsEquals(firstSource, secondSource) && GreaterThan((Links.GetElement(LinkSizeInBytes, first) + Link.TargetOffset).GetValue<TLink>(), (Links.GetElement(LinkSizeInBytes, second) + Link.TargetOffset).GetValue<TLink>()));
            }

            protected override TLink GetTreeRoot() => (Header + LinksHeader.FirstAsSourceOffset).GetValue<TLink>();

            protected override TLink GetBasePartValue(TLink link) => (Links.GetElement(LinkSizeInBytes, link) + Link.SourceOffset).GetValue<TLink>();

            /// <summary>
            /// Выполняет поиск и возвращает индекс связи с указанными Source (началом) и Target (концом)
            /// по дереву (индексу) связей, отсортированному по Source, а затем по Target.
            /// </summary>
            /// <param name="source">Индекс связи, которая является началом на искомой связи.</param>
            /// <param name="target">Индекс связи, которая является концом на искомой связи.</param>
            /// <returns>Индекс искомой связи.</returns>
            public TLink Search(TLink source, TLink target)
            {
                var root = GetTreeRoot();
                while (!EqualToZero(root))
                {
                    var rootSource = (Links.GetElement(LinkSizeInBytes, root) + Link.SourceOffset).GetValue<TLink>();
                    var rootTarget = (Links.GetElement(LinkSizeInBytes, root) + Link.TargetOffset).GetValue<TLink>();
                    if (FirstIsToTheLeftOfSecond(source, target, rootSource, rootTarget)) // node.Key < root.Key
                    {
                        root = GetLeftOrDefault(root);
                    }
                    else if (FirstIsToTheRightOfSecond(source, target, rootSource, rootTarget)) // node.Key > root.Key
                    {
                        root = GetRightOrDefault(root);
                    }
                    else // node.Key == root.Key
                    {
                        return root;
                    }
                }
                return GetZero();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool FirstIsToTheLeftOfSecond(TLink firstSource, TLink firstTarget, TLink secondSource, TLink secondTarget) => LessThan(firstSource, secondSource) || (IsEquals(firstSource, secondSource) && LessThan(firstTarget, secondTarget));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool FirstIsToTheRightOfSecond(TLink firstSource, TLink firstTarget, TLink secondSource, TLink secondTarget) => GreaterThan(firstSource, secondSource) || (IsEquals(firstSource, secondSource) && GreaterThan(firstTarget, secondTarget));

            protected override void PrintNodeValue(TLink node, StringBuilder sb) => throw new NotImplementedException();
        }

        private class LinksTargetsTreeMethods : LinksTreeMethodsBase
        {
            public LinksTargetsTreeMethods(ResizableDirectMemoryLinks<TLink> memory)
                : base(memory)
            {
            }

            protected override IntPtr GetLeftPointer(TLink node) => Links.GetElement(LinkSizeInBytes, node) + Link.LeftAsTargetOffset;

            protected override IntPtr GetRightPointer(TLink node) => Links.GetElement(LinkSizeInBytes, node) + Link.RightAsTargetOffset;

            protected override TLink GetLeftValue(TLink node) => (Links.GetElement(LinkSizeInBytes, node) + Link.LeftAsTargetOffset).GetValue<TLink>();

            protected override TLink GetRightValue(TLink node) => (Links.GetElement(LinkSizeInBytes, node) + Link.RightAsTargetOffset).GetValue<TLink>();

            protected override TLink GetSize(TLink node)
            {
                var previousValue = (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsTargetOffset).GetValue<TLink>();
                return BitwiseHelpers.PartialRead(previousValue, 5, -5);
            }

            protected override void SetLeft(TLink node, TLink left) => (Links.GetElement(LinkSizeInBytes, node) + Link.LeftAsTargetOffset).SetValue(left);

            protected override void SetRight(TLink node, TLink right) => (Links.GetElement(LinkSizeInBytes, node) + Link.RightAsTargetOffset).SetValue(right);

            protected override void SetSize(TLink node, TLink size)
            {
                var previousValue = (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsTargetOffset).GetValue<TLink>();
                (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsTargetOffset).SetValue(BitwiseHelpers.PartialWrite(previousValue, size, 5, -5));
            }

            protected override bool GetLeftIsChild(TLink node)
            {
                var previousValue = (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsTargetOffset).GetValue<TLink>();
                return (Integer<TLink>)BitwiseHelpers.PartialRead(previousValue, 4, 1);
            }

            protected override void SetLeftIsChild(TLink node, bool value)
            {
                var previousValue = (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsTargetOffset).GetValue<TLink>();
                var modified = BitwiseHelpers.PartialWrite(previousValue, (TLink)(Integer<TLink>)value, 4, 1);
                (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsTargetOffset).SetValue(modified);
            }

            protected override bool GetRightIsChild(TLink node)
            {
                var previousValue = (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsTargetOffset).GetValue<TLink>();
                return (Integer<TLink>)BitwiseHelpers.PartialRead(previousValue, 3, 1);
            }

            protected override void SetRightIsChild(TLink node, bool value)
            {
                var previousValue = (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsTargetOffset).GetValue<TLink>();
                var modified = BitwiseHelpers.PartialWrite(previousValue, (TLink)(Integer<TLink>)value, 3, 1);
                (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsTargetOffset).SetValue(modified);
            }

            protected override sbyte GetBalance(TLink node)
            {
                var previousValue = (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsTargetOffset).GetValue<TLink>();
                var value = (ulong)(Integer<TLink>)BitwiseHelpers.PartialRead(previousValue, 0, 3);
                var unpackedValue = (sbyte)((value & 4) > 0 ? ((value & 4) << 5) | value & 3 | 124 : value & 3);
                return unpackedValue;
            }

            protected override void SetBalance(TLink node, sbyte value)
            {
                var previousValue = (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsTargetOffset).GetValue<TLink>();
                var packagedValue = (TLink)(Integer<TLink>)((((byte)value >> 5) & 4) | value & 3);
                var modified = BitwiseHelpers.PartialWrite(previousValue, packagedValue, 0, 3);
                (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsTargetOffset).SetValue(modified);
            }

            protected override bool FirstIsToTheLeftOfSecond(TLink first, TLink second)
            {
                var firstTarget = (Links.GetElement(LinkSizeInBytes, first) + Link.TargetOffset).GetValue<TLink>();
                var secondTarget = (Links.GetElement(LinkSizeInBytes, second) + Link.TargetOffset).GetValue<TLink>();
                return LessThan(firstTarget, secondTarget) ||
                       (IsEquals(firstTarget, secondTarget) && LessThan((Links.GetElement(LinkSizeInBytes, first) + Link.SourceOffset).GetValue<TLink>(), (Links.GetElement(LinkSizeInBytes, second) + Link.SourceOffset).GetValue<TLink>()));
            }

            protected override bool FirstIsToTheRightOfSecond(TLink first, TLink second)
            {
                var firstTarget = (Links.GetElement(LinkSizeInBytes, first) + Link.TargetOffset).GetValue<TLink>();
                var secondTarget = (Links.GetElement(LinkSizeInBytes, second) + Link.TargetOffset).GetValue<TLink>();
                return GreaterThan(firstTarget, secondTarget) ||
                       (IsEquals(firstTarget, secondTarget) && GreaterThan((Links.GetElement(LinkSizeInBytes, first) + Link.SourceOffset).GetValue<TLink>(), (Links.GetElement(LinkSizeInBytes, second) + Link.SourceOffset).GetValue<TLink>()));
            }

            protected override TLink GetTreeRoot() => (Header + LinksHeader.FirstAsTargetOffset).GetValue<TLink>();

            protected override TLink GetBasePartValue(TLink link) => (Links.GetElement(LinkSizeInBytes, link) + Link.TargetOffset).GetValue<TLink>();

            protected override void PrintNodeValue(TLink node, StringBuilder sb) => throw new NotImplementedException();
        }
    }
}