using System;
using System.Runtime.CompilerServices;
using Platform.Data.Core.Collections.Trees;
using Platform.Helpers;

namespace Platform.Data.Core.Pairs
{
    partial class LinksMemoryManager<T>
    {
        private abstract class LinksTreeMethodsBase : SizedAndThreadedAVLBalancedTreeMethods<T>
        {
            private readonly ILinksCombinedConstants<bool, T, int> _constants;
            protected readonly IntPtr Links;
            protected readonly IntPtr Header;

            protected LinksTreeMethodsBase(IntPtr links, IntPtr header, ILinksCombinedConstants<bool, T, int> constants)
            {
                Links = links;
                Header = header;
                _constants = constants;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected abstract T GetTreeRoot();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected abstract T GetBasePartValue(T link);

            public T this[T index]
            {
                get
                {
                    var root = GetTreeRoot();
                    if (GreaterOrEqualThan(index, GetSize(root)))
                        return GetZero();

                    while (!EqualToZero(root))
                    {
                        var left = GetLeftOrDefault(root);
                        var leftSize = GetSizeOrZero(left);
                        if (LessThan(index, leftSize))
                        {
                            root = left;
                            continue;
                        }

                        if (Equals(index, leftSize))
                            return root;

                        root = GetRightOrDefault(root);
                        index = Subtract(index, Increment(leftSize));
                    }
                    return GetZero(); // TODO: Impossible situation exception (only if tree structure broken)
                }
            }

            // TODO: Return indices range instead of references count
            public T CalculateReferences(T link)
            {
                var root = GetTreeRoot();
                var total = GetSize(root);

                var totalRightIgnore = GetZero();

                while (!EqualToZero(root))
                {
                    var @base = GetBasePartValue(root);

                    if (LessOrEqualThan(@base, link))
                        root = GetRightOrDefault(root);
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
                        root = GetLeftOrDefault(root);
                    else
                    {
                        totalLeftIgnore = Add(totalLeftIgnore, Increment(GetLeftSize(root)));

                        root = GetRightOrDefault(root);
                    }
                }

                return Subtract(Subtract(total, totalRightIgnore), totalLeftIgnore);
            }

            public bool EachReference(T link, Func<T, bool> handler)
            {
                var root = GetTreeRoot();

                if (EqualToZero(root))
                    return true;

                T first = GetZero(), current = root;
                while (!EqualToZero(current))
                {
                    var @base = GetBasePartValue(current);
                    if (MathHelpers.GreaterOrEqualThan(@base, link))
                    {
                        if (Equals(@base, link))
                            first = current;
                        current = GetLeftOrDefault(current);
                    }
                    else
                        current = GetRightOrDefault(current);
                }

                if (!EqualToZero(first))
                {
                    var breakConstant = _constants.Break;

                    current = first;
                    while (true)
                    {
                        if (handler(current) == breakConstant)
                            return false;
                        current = GetNext(current);
                        if (EqualToZero(current) || !Equals(GetBasePartValue(current), link))
                            break;
                    }
                }

                return true;
            }

            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            //public bool EachReference(T source, Func<T, bool> handler)
            //{
            //    return EachReferenceCore(source, GetTreeRoot(), handler);
            //}

            // TODO: 1. Move target, handler to separate object. 2. Use stack or walker 3. Use low-level MSIL stack.
            //private bool EachReferenceCore(T @base, T link, Func<T, bool> handler)
            //{
            //    if (EqualToZero(link))
            //        return true;

            //    var linkBase = GetBasePartValue(link);

            //    var breakConstant = Constants.Break;

            //    if (GreaterThan(linkBase, @base))
            //    {
            //        if (EachReferenceCore(@base, GetLeftOrDefault(link), handler) == breakConstant)
            //            return false;
            //    }
            //    else if (LessThan(linkBase, @base))
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
        }

        private class LinksSourcesTreeMethods : LinksTreeMethodsBase
        {
            public LinksSourcesTreeMethods(IntPtr links, IntPtr header, ILinksCombinedConstants<bool, T, int> constants)
                : base(links, header, constants)
            {
            }

            //protected override IntPtr GetLeft(T node) => Links.GetElement(LinkSizeInBytes, node) + Link.LeftAsSourceOffset;

            //protected override IntPtr GetRight(T node) => Links.GetElement(LinkSizeInBytes, node) + Link.RightAsSourceOffset;

            //protected override T GetSize(T node) => (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsSourceOffset).GetValue<T>();

            //protected override void SetLeft(T node, T left) => (Links.GetElement(LinkSizeInBytes, node) + Link.LeftAsSourceOffset).SetValue(left);

            //protected override void SetRight(T node, T right) => (Links.GetElement(LinkSizeInBytes, node) + Link.RightAsSourceOffset).SetValue(right);

            //protected override void SetSize(T node, T size) => (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsSourceOffset).SetValue(size);

            protected override IntPtr GetLeftPointer(T node) => Links.GetElement(LinkSizeInBytes, node) + Link.LeftAsSourceOffset;

            protected override IntPtr GetRightPointer(T node) => Links.GetElement(LinkSizeInBytes, node) + Link.RightAsSourceOffset;

            protected override T GetLeftValue(T node) => (Links.GetElement(LinkSizeInBytes, node) + Link.LeftAsSourceOffset).GetValue<T>();

            protected override T GetRightValue(T node) => (Links.GetElement(LinkSizeInBytes, node) + Link.RightAsSourceOffset).GetValue<T>();

            protected override T GetSize(T node)
            {
                var previousValue = (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsSourceOffset).GetValue<T>();
                return MathHelpers.PartialRead(previousValue, 5, -5);
            }

            protected override void SetLeft(T node, T left) => (Links.GetElement(LinkSizeInBytes, node) + Link.LeftAsSourceOffset).SetValue(left);

            protected override void SetRight(T node, T right) => (Links.GetElement(LinkSizeInBytes, node) + Link.RightAsSourceOffset).SetValue(right);

            protected override void SetSize(T node, T size)
            {
                var previousValue = (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsSourceOffset).GetValue<T>();
                (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsSourceOffset).SetValue(MathHelpers.PartialWrite(previousValue, size, 5, -5));
            }

            protected override bool GetLeftIsChild(T node)
            {
                var previousValue = (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsSourceOffset).GetValue<T>();
                return (Integer<T>)MathHelpers.PartialRead(previousValue, 4, 1);
            }

            protected override void SetLeftIsChild(T node, bool value)
            {
                var previousValue = (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsSourceOffset).GetValue<T>();
                var modified = MathHelpers.PartialWrite(previousValue, (T)(Integer<T>)value, 4, 1);
                (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsSourceOffset).SetValue(modified);
            }

            protected override bool GetRightIsChild(T node)
            {
                var previousValue = (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsSourceOffset).GetValue<T>();
                return (Integer<T>)MathHelpers.PartialRead(previousValue, 3, 1);
            }

            protected override void SetRightIsChild(T node, bool value)
            {
                var previousValue = (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsSourceOffset).GetValue<T>();
                var modified = MathHelpers.PartialWrite(previousValue, (T)(Integer<T>)value, 3, 1);
                (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsSourceOffset).SetValue(modified);
            }

            protected override sbyte GetBalance(T node)
            {
                var previousValue = (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsSourceOffset).GetValue<T>();
                var value = (ulong)(Integer<T>)MathHelpers.PartialRead(previousValue, 0, 3);
                var unpackedValue = (sbyte)((value & 4) > 0 ? ((value & 4) << 5) | value & 3 | 124 : value & 3);
                return unpackedValue;
            }

            protected override void SetBalance(T node, sbyte value)
            {
                var previousValue = (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsSourceOffset).GetValue<T>();
                var packagedValue = (T)(Integer<T>)((((byte)value >> 5) & 4) | value & 3);
                var modified = MathHelpers.PartialWrite(previousValue, packagedValue, 0, 3);
                (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsSourceOffset).SetValue(modified);
            }

            protected override bool FirstIsToTheLeftOfSecond(T first, T second)
            {
                var firstSource = (Links.GetElement(LinkSizeInBytes, first) + Link.SourceOffset).GetValue<T>();
                var secondSource = (Links.GetElement(LinkSizeInBytes, second) + Link.SourceOffset).GetValue<T>();

                return LessThan(firstSource, secondSource) ||
                       (Equals(firstSource, secondSource) && LessThan((Links.GetElement(LinkSizeInBytes, first) + Link.TargetOffset).GetValue<T>(), (Links.GetElement(LinkSizeInBytes, second) + Link.TargetOffset).GetValue<T>()));
            }

            protected override bool FirstIsToTheRightOfSecond(T first, T second)
            {
                var firstSource = (Links.GetElement(LinkSizeInBytes, first) + Link.SourceOffset).GetValue<T>();
                var secondSource = (Links.GetElement(LinkSizeInBytes, second) + Link.SourceOffset).GetValue<T>();

                return GreaterThan(firstSource, secondSource) ||
                       (Equals(firstSource, secondSource) && GreaterThan((Links.GetElement(LinkSizeInBytes, first) + Link.TargetOffset).GetValue<T>(), (Links.GetElement(LinkSizeInBytes, second) + Link.TargetOffset).GetValue<T>()));
            }

            protected override T GetTreeRoot()
            {
                return (Header + LinksHeader.FirstAsSourceOffset).GetValue<T>();
            }

            protected override T GetBasePartValue(T link)
            {
                return (Links.GetElement(LinkSizeInBytes, link) + Link.SourceOffset).GetValue<T>();
            }

            /// <summary>
            /// Выполняет поиск и возвращает индекс связи с указанными Source (началом) и Target (концом)
            /// по дереву (индексу) связей, отсортированному по Source, а затем по Target.
            /// </summary>
            /// <param name="source">Индекс связи, которая является началом на искомой связи.</param>
            /// <param name="target">Индекс связи, которая является концом на искомой связи.</param>
            /// <returns>Индекс искомой связи.</returns>
            public T Search(T source, T target)
            {
                var root = GetTreeRoot();

                while (!EqualToZero(root))
                {
                    var rootSource = (Links.GetElement(LinkSizeInBytes, root) + Link.SourceOffset).GetValue<T>();
                    var rootTarget = (Links.GetElement(LinkSizeInBytes, root) + Link.TargetOffset).GetValue<T>();

                    if (FirstIsToTheLeftOfSecond(source, target, rootSource, rootTarget)) // node.Key < root.Key
                        root = GetLeftOrDefault(root);
                    else if (FirstIsToTheRightOfSecond(source, target, rootSource, rootTarget)) // node.Key > root.Key
                        root = GetRightOrDefault(root);
                    else // node.Key == root.Key
                        return root;
                }

                return GetZero();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool FirstIsToTheLeftOfSecond(T firstSource, T firstTarget, T secondSource, T secondTarget)
            {
                return LessThan(firstSource, secondSource) || (Equals(firstSource, secondSource) && LessThan(firstTarget, secondTarget));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool FirstIsToTheRightOfSecond(T firstSource, T firstTarget, T secondSource, T secondTarget)
            {
                return GreaterThan(firstSource, secondSource) || (Equals(firstSource, secondSource) && GreaterThan(firstTarget, secondTarget));
            }
        }

        private class LinksTargetsTreeMethods : LinksTreeMethodsBase
        {
            public LinksTargetsTreeMethods(IntPtr links, IntPtr header, ILinksCombinedConstants<bool, T, int> constants)
                : base(links, header, constants)
            {
            }

            //protected override IntPtr GetLeft(T node) => Links.GetElement(LinkSizeInBytes, node) + Link.LeftAsTargetOffset;

            //protected override IntPtr GetRight(T node) => Links.GetElement(LinkSizeInBytes, node) + Link.RightAsTargetOffset;

            //protected override T GetSize(T node) => (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsTargetOffset).GetValue<T>();

            //protected override void SetLeft(T node, T left) => (Links.GetElement(LinkSizeInBytes, node) + Link.LeftAsTargetOffset).SetValue(left);

            //protected override void SetRight(T node, T right) => (Links.GetElement(LinkSizeInBytes, node) + Link.RightAsTargetOffset).SetValue(right);

            //protected override void SetSize(T node, T size) => (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsTargetOffset).SetValue(size);

            protected override IntPtr GetLeftPointer(T node) => Links.GetElement(LinkSizeInBytes, node) + Link.LeftAsTargetOffset;

            protected override IntPtr GetRightPointer(T node) => Links.GetElement(LinkSizeInBytes, node) + Link.RightAsTargetOffset;

            protected override T GetLeftValue(T node) => (Links.GetElement(LinkSizeInBytes, node) + Link.LeftAsTargetOffset).GetValue<T>();

            protected override T GetRightValue(T node) => (Links.GetElement(LinkSizeInBytes, node) + Link.RightAsTargetOffset).GetValue<T>();

            protected override T GetSize(T node)
            {
                var previousValue = (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsTargetOffset).GetValue<T>();
                return MathHelpers.PartialRead(previousValue, 5, -5);
            }

            protected override void SetLeft(T node, T left) => (Links.GetElement(LinkSizeInBytes, node) + Link.LeftAsTargetOffset).SetValue(left);

            protected override void SetRight(T node, T right) => (Links.GetElement(LinkSizeInBytes, node) + Link.RightAsTargetOffset).SetValue(right);

            protected override void SetSize(T node, T size)
            {
                var previousValue = (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsTargetOffset).GetValue<T>();
                (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsTargetOffset).SetValue(MathHelpers.PartialWrite(previousValue, size, 5, -5));
            }

            protected override bool GetLeftIsChild(T node)
            {
                var previousValue = (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsTargetOffset).GetValue<T>();
                return (Integer<T>)MathHelpers.PartialRead(previousValue, 4, 1);
            }

            protected override void SetLeftIsChild(T node, bool value)
            {
                var previousValue = (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsTargetOffset).GetValue<T>();
                var modified = MathHelpers.PartialWrite(previousValue, (T)(Integer<T>)value, 4, 1);
                (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsTargetOffset).SetValue(modified);
            }

            protected override bool GetRightIsChild(T node)
            {
                var previousValue = (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsTargetOffset).GetValue<T>();
                return (Integer<T>)MathHelpers.PartialRead(previousValue, 3, 1);
            }

            protected override void SetRightIsChild(T node, bool value)
            {
                var previousValue = (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsTargetOffset).GetValue<T>();
                var modified = MathHelpers.PartialWrite(previousValue, (T)(Integer<T>)value, 3, 1);
                (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsTargetOffset).SetValue(modified);
            }

            protected override sbyte GetBalance(T node)
            {
                var previousValue = (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsTargetOffset).GetValue<T>();
                var value = (ulong)(Integer<T>)MathHelpers.PartialRead(previousValue, 0, 3);
                var unpackedValue = (sbyte)((value & 4) > 0 ? ((value & 4) << 5) | value & 3 | 124 : value & 3);
                return unpackedValue;
            }

            protected override void SetBalance(T node, sbyte value)
            {
                var previousValue = (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsTargetOffset).GetValue<T>();
                var packagedValue = (T)(Integer<T>)((((byte)value >> 5) & 4) | value & 3);
                var modified = MathHelpers.PartialWrite(previousValue, packagedValue, 0, 3);
                (Links.GetElement(LinkSizeInBytes, node) + Link.SizeAsTargetOffset).SetValue(modified);
            }

            protected override bool FirstIsToTheLeftOfSecond(T first, T second)
            {
                var firstTarget = (Links.GetElement(LinkSizeInBytes, first) + Link.TargetOffset).GetValue<T>();
                var secondTarget = (Links.GetElement(LinkSizeInBytes, second) + Link.TargetOffset).GetValue<T>();

                return LessThan(firstTarget, secondTarget) ||
                       (Equals(firstTarget, secondTarget) && LessThan((Links.GetElement(LinkSizeInBytes, first) + Link.SourceOffset).GetValue<T>(), (Links.GetElement(LinkSizeInBytes, second) + Link.SourceOffset).GetValue<T>()));
            }

            protected override bool FirstIsToTheRightOfSecond(T first, T second)
            {
                var firstTarget = (Links.GetElement(LinkSizeInBytes, first) + Link.TargetOffset).GetValue<T>();
                var secondTarget = (Links.GetElement(LinkSizeInBytes, second) + Link.TargetOffset).GetValue<T>();

                return GreaterThan(firstTarget, secondTarget) ||
                       (Equals(firstTarget, secondTarget) && GreaterThan((Links.GetElement(LinkSizeInBytes, first) + Link.SourceOffset).GetValue<T>(), (Links.GetElement(LinkSizeInBytes, second) + Link.SourceOffset).GetValue<T>()));
            }

            protected override T GetTreeRoot()
            {
                return (Header + LinksHeader.FirstAsTargetOffset).GetValue<T>();
            }

            protected override T GetBasePartValue(T link)
            {
                return (Links.GetElement(LinkSizeInBytes, link) + Link.TargetOffset).GetValue<T>();
            }
        }
    }
}