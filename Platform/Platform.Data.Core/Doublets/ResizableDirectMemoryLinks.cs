using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Platform.Disposables;
using Platform.Helpers;
using Platform.Collections.Arrays;
using Platform.Helpers.Numbers;
using static Platform.Helpers.Numbers.MathHelpers;
using Platform.Helpers.Unsafe;
using Platform.Memory;
using Platform.Data.Core.Exceptions;

#pragma warning disable 0649
#pragma warning disable 169
#pragma warning disable 618

// ReSharper disable StaticMemberInGenericType
// ReSharper disable BuiltInTypeReferenceStyle
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable UnusedMember.Local

namespace Platform.Data.Core.Doublets
{
    /// <remarks>
    /// TODO: Вместо address и size принимать IMemory (возможно потребуется добавить Step и StepSize).
    /// </remarks>
    public partial class ResizableDirectMemoryLinks<TLink> : DisposableBase, ILinks<TLink>
    {
        private static readonly EqualityComparer<TLink> EqualityComparer = EqualityComparer<TLink>.Default;
        private static readonly Comparer<TLink> Comparer = Comparer<TLink>.Default;

        /// <summary>Возвращает размер одной связи в байтах.</summary>
        public static readonly int LinkSizeInBytes = StructureHelpers.SizeOf<Link>();

        public static readonly int LinkHeaderSizeInBytes = StructureHelpers.SizeOf<LinksHeader>();

        public static readonly long DefaultLinksSizeStep = LinkSizeInBytes * 1024 * 1024;

        private struct Link
        {
            public static readonly int SourceOffset = Marshal.OffsetOf(typeof(Link), nameof(Source)).ToInt32();
            public static readonly int TargetOffset = Marshal.OffsetOf(typeof(Link), nameof(Target)).ToInt32();
            public static readonly int LeftAsSourceOffset = Marshal.OffsetOf(typeof(Link), nameof(LeftAsSource)).ToInt32();
            public static readonly int RightAsSourceOffset = Marshal.OffsetOf(typeof(Link), nameof(RightAsSource)).ToInt32();
            public static readonly int SizeAsSourceOffset = Marshal.OffsetOf(typeof(Link), nameof(SizeAsSource)).ToInt32();
            public static readonly int LeftAsTargetOffset = Marshal.OffsetOf(typeof(Link), nameof(LeftAsTarget)).ToInt32();
            public static readonly int RightAsTargetOffset = Marshal.OffsetOf(typeof(Link), nameof(RightAsTarget)).ToInt32();
            public static readonly int SizeAsTargetOffset = Marshal.OffsetOf(typeof(Link), nameof(SizeAsTarget)).ToInt32();

            public TLink Source;
            public TLink Target;
            public TLink LeftAsSource;
            public TLink RightAsSource;
            public TLink SizeAsSource;
            public TLink LeftAsTarget;
            public TLink RightAsTarget;
            public TLink SizeAsTarget;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static TLink GetSource(IntPtr pointer) => (pointer + SourceOffset).GetValue<TLink>();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static TLink GetTarget(IntPtr pointer) => (pointer + TargetOffset).GetValue<TLink>();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static TLink GetLeftAsSource(IntPtr pointer) => (pointer + LeftAsSourceOffset).GetValue<TLink>();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static TLink GetRightAsSource(IntPtr pointer) => (pointer + RightAsSourceOffset).GetValue<TLink>();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static TLink GetSizeAsSource(IntPtr pointer) => (pointer + SizeAsSourceOffset).GetValue<TLink>();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static TLink GetLeftAsTarget(IntPtr pointer) => (pointer + LeftAsTargetOffset).GetValue<TLink>();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static TLink GetRightAsTarget(IntPtr pointer) => (pointer + RightAsTargetOffset).GetValue<TLink>();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static TLink GetSizeAsTarget(IntPtr pointer) => (pointer + SizeAsTargetOffset).GetValue<TLink>();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetSource(IntPtr pointer, TLink value) => (pointer + SourceOffset).SetValue(value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetTarget(IntPtr pointer, TLink value) => (pointer + TargetOffset).SetValue(value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetLeftAsSource(IntPtr pointer, TLink value) => (pointer + LeftAsSourceOffset).SetValue(value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetRightAsSource(IntPtr pointer, TLink value) => (pointer + RightAsSourceOffset).SetValue(value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetSizeAsSource(IntPtr pointer, TLink value) => (pointer + SizeAsSourceOffset).SetValue(value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetLeftAsTarget(IntPtr pointer, TLink value) => (pointer + LeftAsTargetOffset).SetValue(value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetRightAsTarget(IntPtr pointer, TLink value) => (pointer + RightAsTargetOffset).SetValue(value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetSizeAsTarget(IntPtr pointer, TLink value) => (pointer + SizeAsTargetOffset).SetValue(value);
        }

        private struct LinksHeader
        {
            public static readonly int AllocatedLinksOffset = Marshal.OffsetOf(typeof(LinksHeader), nameof(AllocatedLinks)).ToInt32();
            public static readonly int ReservedLinksOffset = Marshal.OffsetOf(typeof(LinksHeader), nameof(ReservedLinks)).ToInt32();
            public static readonly int FreeLinksOffset = Marshal.OffsetOf(typeof(LinksHeader), nameof(FreeLinks)).ToInt32();
            public static readonly int FirstFreeLinkOffset = Marshal.OffsetOf(typeof(LinksHeader), nameof(FirstFreeLink)).ToInt32();
            public static readonly int FirstAsSourceOffset = Marshal.OffsetOf(typeof(LinksHeader), nameof(FirstAsSource)).ToInt32();
            public static readonly int FirstAsTargetOffset = Marshal.OffsetOf(typeof(LinksHeader), nameof(FirstAsTarget)).ToInt32();
            public static readonly int LastFreeLinkOffset = Marshal.OffsetOf(typeof(LinksHeader), nameof(LastFreeLink)).ToInt32();

            public TLink AllocatedLinks;
            public TLink ReservedLinks;
            public TLink FreeLinks;
            public TLink FirstFreeLink;
            public TLink FirstAsSource;
            public TLink FirstAsTarget;
            public TLink LastFreeLink;
            public TLink Reserved8;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static TLink GetAllocatedLinks(IntPtr pointer) => (pointer + AllocatedLinksOffset).GetValue<TLink>();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static TLink GetReservedLinks(IntPtr pointer) => (pointer + ReservedLinksOffset).GetValue<TLink>();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static TLink GetFreeLinks(IntPtr pointer) => (pointer + FreeLinksOffset).GetValue<TLink>();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static TLink GetFirstFreeLink(IntPtr pointer) => (pointer + FirstFreeLinkOffset).GetValue<TLink>();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static TLink GetFirstAsSource(IntPtr pointer) => (pointer + FirstAsSourceOffset).GetValue<TLink>();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static TLink GetFirstAsTarget(IntPtr pointer) => (pointer + FirstAsTargetOffset).GetValue<TLink>();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static TLink GetLastFreeLink(IntPtr pointer) => (pointer + LastFreeLinkOffset).GetValue<TLink>();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static IntPtr GetFirstAsSourcePointer(IntPtr pointer) => pointer + FirstAsSourceOffset;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static IntPtr GetFirstAsTargetPointer(IntPtr pointer) => pointer + FirstAsTargetOffset;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetAllocatedLinks(IntPtr pointer, TLink value) => (pointer + AllocatedLinksOffset).SetValue(value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetReservedLinks(IntPtr pointer, TLink value) => (pointer + ReservedLinksOffset).SetValue(value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetFreeLinks(IntPtr pointer, TLink value) => (pointer + FreeLinksOffset).SetValue(value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetFirstFreeLink(IntPtr pointer, TLink value) => (pointer + FirstFreeLinkOffset).SetValue(value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetFirstAsSource(IntPtr pointer, TLink value) => (pointer + FirstAsSourceOffset).SetValue(value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetFirstAsTarget(IntPtr pointer, TLink value) => (pointer + FirstAsTargetOffset).SetValue(value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetLastFreeLink(IntPtr pointer, TLink value) => (pointer + LastFreeLinkOffset).SetValue(value);
        }

        private readonly long _memoryReservationStep;

        private readonly IResizableDirectMemory _memory;
        private IntPtr _header;
        private IntPtr _links;

        private LinksTargetsTreeMethods _targetsTreeMethods;
        private LinksSourcesTreeMethods _sourcesTreeMethods;

        // TODO: Возможно чтобы гарантированно проверять на то, является ли связь удалённой, нужно использовать не список а дерево, так как так можно быстрее проверить на наличие связи внутри
        private UnusedLinksListMethods _unusedLinksListMethods;

        /// <summary>
        /// Возвращает общее число связей находящихся в хранилище.
        /// </summary>
        private TLink Total => Subtract(LinksHeader.GetAllocatedLinks(_header), LinksHeader.GetFreeLinks(_header));

        public ResizableDirectMemoryLinks(string address)
            : this(address, DefaultLinksSizeStep)
        {
        }

        /// <summary>
        /// Создаёт экземпляр базы данных Links в файле по указанному адресу, с указанным минимальным шагом расширения базы данных.
        /// </summary>
        /// <param name="address">Полный пусть к файлу базы данных.</param>
        /// <param name="memoryReservationStep">Минимальный шаг расширения базы данных в байтах.</param>
        public ResizableDirectMemoryLinks(string address, long memoryReservationStep)
            : this(new FileMappedResizableDirectMemory(address, memoryReservationStep), memoryReservationStep)
        {
        }

        public ResizableDirectMemoryLinks(IResizableDirectMemory memory)
            : this(memory, DefaultLinksSizeStep)
        {
        }

        public ResizableDirectMemoryLinks(IResizableDirectMemory memory, long memoryReservationStep)
        {
            Constants = Default<LinksConstants<TLink, TLink, int>>.Instance;

            _memory = memory;
            _memoryReservationStep = memoryReservationStep;

            if (memory.ReservedCapacity < memoryReservationStep)
                memory.ReservedCapacity = memoryReservationStep;

            SetPointers(_memory);

            // Гарантия корректности _memory.UsedCapacity относительно _header->AllocatedLinks
            _memory.UsedCapacity = (long)(Integer<TLink>)LinksHeader.GetAllocatedLinks(_header) * LinkSizeInBytes + LinkHeaderSizeInBytes;

            // Гарантия корректности _header->ReservedLinks относительно _memory.ReservedCapacity
            LinksHeader.SetReservedLinks(_header, (Integer<TLink>)((_memory.ReservedCapacity - LinkHeaderSizeInBytes) / LinkSizeInBytes));
        }

        public ILinksCombinedConstants<TLink, TLink, int> Constants { get; private set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLink Count(IList<TLink> restrictions)
        {
            // Если нет ограничений, тогда возвращаем общее число связей находящихся в хранилище.
            if (restrictions.Count == 0)
                return Total;
            if (restrictions.Count == 1)
            {
                var index = restrictions[Constants.IndexPart];

                if (EqualityComparer.Equals(index, Constants.Any))
                    return Total;

                return Exists(index) ? Integer<TLink>.One : Integer<TLink>.Zero;
            }
            if (restrictions.Count == 2)
            {
                var index = restrictions[Constants.IndexPart];
                var value = restrictions[1];

                if (EqualityComparer.Equals(index, Constants.Any))
                {
                    if (EqualityComparer.Equals(value, Constants.Any))
                        return Total; // Any - как отсутствие ограничения

                    return Add(_sourcesTreeMethods.CalculateReferences(value), _targetsTreeMethods.CalculateReferences(value));
                }
                else
                {
                    if (!Exists(index))
                        return Integer<TLink>.Zero;

                    if (EqualityComparer.Equals(value, Constants.Any))
                        return Integer<TLink>.One;

                    var storedLinkValue = GetLinkUnsafe(index);
                    if (EqualityComparer.Equals(Link.GetSource(storedLinkValue), value) ||
                        EqualityComparer.Equals(Link.GetTarget(storedLinkValue), value))
                        return Integer<TLink>.One;
                    return Integer<TLink>.Zero;
                }
            }
            if (restrictions.Count == 3)
            {
                var index = restrictions[Constants.IndexPart];
                var source = restrictions[Constants.SourcePart];
                var target = restrictions[Constants.TargetPart];

                if (EqualityComparer.Equals(index, Constants.Any))
                {
                    if (EqualityComparer.Equals(source, Constants.Any) && EqualityComparer.Equals(target, Constants.Any))
                        return Total;
                    else if (EqualityComparer.Equals(source, Constants.Any))
                        return _targetsTreeMethods.CalculateReferences(target);
                    else if (EqualityComparer.Equals(target, Constants.Any))
                        return _sourcesTreeMethods.CalculateReferences(source);
                    else //if(source != Any && target != Any)
                    {
                        // Эквивалент Exists(source, target) => Count(Any, source, target) > 0
                        var link = _sourcesTreeMethods.Search(source, target);
                        return EqualityComparer.Equals(link, Constants.Null) ? Integer<TLink>.Zero : Integer<TLink>.One;
                    }
                }
                else
                {
                    if (!Exists(index))
                        return Integer<TLink>.Zero;

                    if (EqualityComparer.Equals(source, Constants.Any) && EqualityComparer.Equals(target, Constants.Any))
                        return Integer<TLink>.One;

                    var storedLinkValue = GetLinkUnsafe(index);

                    if (!EqualityComparer.Equals(source, Constants.Any) && !EqualityComparer.Equals(target, Constants.Any))
                    {
                        if (EqualityComparer.Equals(Link.GetSource(storedLinkValue), source) &&
                            EqualityComparer.Equals(Link.GetTarget(storedLinkValue), target))
                            return Integer<TLink>.One;
                        return Integer<TLink>.Zero;
                    }

                    var value = default(TLink);
                    if (EqualityComparer.Equals(source, Constants.Any)) value = target;
                    if (EqualityComparer.Equals(target, Constants.Any)) value = source;

                    if (EqualityComparer.Equals(Link.GetSource(storedLinkValue), value) ||
                        EqualityComparer.Equals(Link.GetTarget(storedLinkValue), value))
                        return Integer<TLink>.One;
                    return Integer<TLink>.Zero;
                }
            }
            throw new NotSupportedException("Другие размеры и способы ограничений не поддерживаются.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLink Each(Func<IList<TLink>, TLink> handler, IList<TLink> restrictions)
        {
            if (restrictions.Count == 0)
            {
                for (TLink link = Integer<TLink>.One; Comparer.Compare(link, (TLink)(Integer<TLink>)LinksHeader.GetAllocatedLinks(_header)) <= 0; link = Increment(link))
                    if (Exists(link))
                        if (EqualityComparer.Equals(handler(GetLinkStruct(link)), Constants.Break))
                            return Constants.Break;

                return Constants.Continue;
            }
            if (restrictions.Count == 1)
            {
                var index = restrictions[Constants.IndexPart];

                if (EqualityComparer.Equals(index, Constants.Any))
                    return Each(handler, ArrayPool<TLink>.Empty);

                if (!Exists(index))
                    return Constants.Continue;

                return handler(GetLinkStruct(index));
            }
            if (restrictions.Count == 2)
            {
                var index = restrictions[Constants.IndexPart];
                var value = restrictions[1];

                if (EqualityComparer.Equals(index, Constants.Any))
                {
                    if (EqualityComparer.Equals(value, Constants.Any))
                        return Each(handler, ArrayPool<TLink>.Empty);

                    if (EqualityComparer.Equals(Each(handler, new[] { index, value, Constants.Any }), Constants.Break))
                        return Constants.Break;

                    return Each(handler, new[] { index, Constants.Any, value });
                }
                else
                {
                    if (!Exists(index))
                        return Constants.Continue;

                    if (EqualityComparer.Equals(value, Constants.Any))
                        return handler(GetLinkStruct(index));

                    var storedLinkValue = GetLinkUnsafe(index);
                    if (EqualityComparer.Equals(Link.GetSource(storedLinkValue), value) ||
                        EqualityComparer.Equals(Link.GetTarget(storedLinkValue), value))
                        return handler(GetLinkStruct(index));
                    return Constants.Continue;
                }
            }
            if (restrictions.Count == 3)
            {
                var index = restrictions[Constants.IndexPart];
                var source = restrictions[Constants.SourcePart];
                var target = restrictions[Constants.TargetPart];

                if (EqualityComparer.Equals(index, Constants.Any))
                {
                    if (EqualityComparer.Equals(source, Constants.Any) && EqualityComparer.Equals(target, Constants.Any))
                        return Each(handler, ArrayPool<TLink>.Empty);
                    else if (EqualityComparer.Equals(source, Constants.Any))
                        return _targetsTreeMethods.EachReference(target, handler);
                    else if (EqualityComparer.Equals(target, Constants.Any))
                        return _sourcesTreeMethods.EachReference(source, handler);
                    else //if(source != Any && target != Any)
                    {
                        var link = _sourcesTreeMethods.Search(source, target);

                        return EqualityComparer.Equals(link, Constants.Null) ? Constants.Continue : handler(GetLinkStruct(link));
                    }
                }
                else
                {
                    if (!Exists(index))
                        return Constants.Continue;

                    if (EqualityComparer.Equals(source, Constants.Any) && EqualityComparer.Equals(target, Constants.Any))
                        return handler(GetLinkStruct(index));

                    var storedLinkValue = GetLinkUnsafe(index);

                    if (!EqualityComparer.Equals(source, Constants.Any) && !EqualityComparer.Equals(target, Constants.Any))
                    {
                        if (EqualityComparer.Equals(Link.GetSource(storedLinkValue), source) &&
                            EqualityComparer.Equals(Link.GetTarget(storedLinkValue), target))
                            return handler(GetLinkStruct(index));
                        return Constants.Continue;
                    }

                    var value = default(TLink);
                    if (EqualityComparer.Equals(source, Constants.Any)) value = target;
                    if (EqualityComparer.Equals(target, Constants.Any)) value = source;

                    if (EqualityComparer.Equals(Link.GetSource(storedLinkValue), value) ||
                        EqualityComparer.Equals(Link.GetTarget(storedLinkValue), value))
                        return handler(GetLinkStruct(index));
                    return Constants.Continue;
                }
            }
            throw new NotSupportedException("Другие размеры и способы ограничений не поддерживаются.");
        }

        /// <remarks>
        /// TODO: Возможно можно перемещать значения, если указан индекс, но значение существует в другом месте (но не в менеджере памяти, а в логике Links)
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLink Update(IList<TLink> values)
        {
            var linkIndex = values[Constants.IndexPart];
            var link = GetLinkUnsafe(linkIndex);

            // Будет корректно работать только в том случае, если пространство выделенной связи предварительно заполнено нулями
            if (!EqualityComparer.Equals(Link.GetSource(link), Constants.Null)) _sourcesTreeMethods.Detach(LinksHeader.GetFirstAsSourcePointer(_header), linkIndex);
            if (!EqualityComparer.Equals(Link.GetTarget(link), Constants.Null)) _targetsTreeMethods.Detach(LinksHeader.GetFirstAsTargetPointer(_header), linkIndex);

            Link.SetSource(link, values[Constants.SourcePart]);
            Link.SetTarget(link, values[Constants.TargetPart]);

            if (!EqualityComparer.Equals(Link.GetSource(link), Constants.Null)) _sourcesTreeMethods.Attach(LinksHeader.GetFirstAsSourcePointer(_header), linkIndex);
            if (!EqualityComparer.Equals(Link.GetTarget(link), Constants.Null)) _targetsTreeMethods.Attach(LinksHeader.GetFirstAsTargetPointer(_header), linkIndex);

            return linkIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Link<TLink> GetLinkStruct(TLink linkIndex)
        {
            var link = GetLinkUnsafe(linkIndex);
            return new Link<TLink>(linkIndex, Link.GetSource(link), Link.GetTarget(link));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IntPtr GetLinkUnsafe(TLink linkIndex)
        {
            return _links.GetElement(LinkSizeInBytes, linkIndex);
        }

        /// <remarks>
        /// TODO: Возможно нужно будет заполнение нулями, если внешнее API ими не заполняет пространство
        /// </remarks>
        public TLink Create()
        {
            var freeLink = LinksHeader.GetFirstFreeLink(_header);

            if (!EqualityComparer.Equals(freeLink, Constants.Null))
                _unusedLinksListMethods.Detach(freeLink);
            else
            {
                if (Comparer.Compare(LinksHeader.GetAllocatedLinks(_header), Constants.MaxPossibleIndex) > 0)
                    throw new LinksLimitReachedException((Integer<TLink>)Constants.MaxPossibleIndex);

                if (Comparer.Compare(LinksHeader.GetAllocatedLinks(_header), Decrement(LinksHeader.GetReservedLinks(_header))) >= 0)
                {
                    _memory.ReservedCapacity += _memoryReservationStep;
                    SetPointers(_memory);
                    LinksHeader.SetReservedLinks(_header, (Integer<TLink>)(_memory.ReservedCapacity / LinkSizeInBytes));
                }

                LinksHeader.SetAllocatedLinks(_header, Increment(LinksHeader.GetAllocatedLinks(_header)));
                _memory.UsedCapacity += LinkSizeInBytes;
                freeLink = LinksHeader.GetAllocatedLinks(_header);
            }

            return freeLink;
        }

        public void Delete(TLink link)
        {
            if (Comparer.Compare(link, LinksHeader.GetAllocatedLinks(_header)) < 0)
                _unusedLinksListMethods.AttachAsFirst(link);
            else if (EqualityComparer.Equals(link, LinksHeader.GetAllocatedLinks(_header)))
            {
                LinksHeader.SetAllocatedLinks(_header, Decrement(LinksHeader.GetAllocatedLinks(_header)));
                _memory.UsedCapacity -= LinkSizeInBytes;

                // Убираем все связи, находящиеся в списке свободных в конце файла, до тех пор, пока не дойдём до первой существующей связи
                // Позволяет оптимизировать количество выделенных связей (AllocatedLinks)
                while ((Comparer.Compare(LinksHeader.GetAllocatedLinks(_header), Integer<TLink>.Zero) > 0) && IsUnusedLink(LinksHeader.GetAllocatedLinks(_header)))
                {
                    _unusedLinksListMethods.Detach(LinksHeader.GetAllocatedLinks(_header));

                    LinksHeader.SetAllocatedLinks(_header, Decrement(LinksHeader.GetAllocatedLinks(_header)));
                    _memory.UsedCapacity -= LinkSizeInBytes;
                }
            }
        }

        /// <remarks>
        /// TODO: Возможно это должно быть событием, вызываемым из IMemory, в том случае, если адрес реально поменялся
        /// 
        /// Указатель this.links может быть в том же месте, 
        /// так как 0-я связь не используется и имеет такой же размер как Header,
        /// поэтому header размещается в том же месте, что и 0-я связь
        /// </remarks>
        private void SetPointers(IDirectMemory memory)
        {
            if (memory == null)
            {
                _header = _links = IntPtr.Zero;

                _unusedLinksListMethods = null;
                _targetsTreeMethods = null;
                _unusedLinksListMethods = null;
            }
            else
            {
                _header = _links = memory.Pointer;

                _sourcesTreeMethods = new LinksSourcesTreeMethods(this);
                _targetsTreeMethods = new LinksTargetsTreeMethods(this);
                _unusedLinksListMethods = new UnusedLinksListMethods(_links, _header);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Exists(TLink link)
        {
            return (Comparer.Compare(link, Constants.MinPossibleIndex) >= 0) && (Comparer.Compare(link, LinksHeader.GetAllocatedLinks(_header)) <= 0) && !IsUnusedLink(link);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsUnusedLink(TLink link)
        {
            return EqualityComparer.Equals(LinksHeader.GetFirstFreeLink(_header), link)
                   || (EqualityComparer.Equals(Link.GetSizeAsSource(GetLinkUnsafe(link)), Constants.Null) && !EqualityComparer.Equals(Link.GetSource(GetLinkUnsafe(link)), Constants.Null));
        }

        #region Disposable

        protected override bool AllowMultipleDisposeCalls => true;

        protected override void DisposeCore(bool manual, bool wasDisposed)
        {
            if (!wasDisposed)
                SetPointers(null);
            Disposable.TryDispose(_memory);
        }

        #endregion
    }
}