using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Platform.Data.Core.Exceptions;
using Platform.Helpers;
using Platform.Helpers.Collections;
using Platform.Helpers.Disposables;
using Platform.Memory;
using static Platform.Helpers.MathHelpers;

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
    public partial class LinksMemoryManager<T> : DisposableBase, ILinksMemoryManager<T>
    {
        /// <summary>Возвращает размер одной связи в байтах.</summary>
        public static readonly int LinkSizeInBytes = UnsafeHelpers.SizeOf<Link>();

        public static readonly int LinkHeaderSizeInBytes = UnsafeHelpers.SizeOf<LinksHeader>();

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

            public T Source;
            public T Target;
            public T LeftAsSource;
            public T RightAsSource;
            public T SizeAsSource;
            public T LeftAsTarget;
            public T RightAsTarget;
            public T SizeAsTarget;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static T GetSource(IntPtr pointer) => (pointer + SourceOffset).GetValue<T>();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static T GetTarget(IntPtr pointer) => (pointer + TargetOffset).GetValue<T>();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static T GetLeftAsSource(IntPtr pointer) => (pointer + LeftAsSourceOffset).GetValue<T>();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static T GetRightAsSource(IntPtr pointer) => (pointer + RightAsSourceOffset).GetValue<T>();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static T GetSizeAsSource(IntPtr pointer) => (pointer + SizeAsSourceOffset).GetValue<T>();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static T GetLeftAsTarget(IntPtr pointer) => (pointer + LeftAsTargetOffset).GetValue<T>();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static T GetRightAsTarget(IntPtr pointer) => (pointer + RightAsTargetOffset).GetValue<T>();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static T GetSizeAsTarget(IntPtr pointer) => (pointer + SizeAsTargetOffset).GetValue<T>();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetSource(IntPtr pointer, T value) => (pointer + SourceOffset).SetValue(value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetTarget(IntPtr pointer, T value) => (pointer + TargetOffset).SetValue(value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetLeftAsSource(IntPtr pointer, T value) => (pointer + LeftAsSourceOffset).SetValue(value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetRightAsSource(IntPtr pointer, T value) => (pointer + RightAsSourceOffset).SetValue(value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetSizeAsSource(IntPtr pointer, T value) => (pointer + SizeAsSourceOffset).SetValue(value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetLeftAsTarget(IntPtr pointer, T value) => (pointer + LeftAsTargetOffset).SetValue(value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetRightAsTarget(IntPtr pointer, T value) => (pointer + RightAsTargetOffset).SetValue(value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetSizeAsTarget(IntPtr pointer, T value) => (pointer + SizeAsTargetOffset).SetValue(value);
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

            public T AllocatedLinks;
            public T ReservedLinks;
            public T FreeLinks;
            public T FirstFreeLink;
            public T FirstAsSource;
            public T FirstAsTarget;
            public T LastFreeLink;
            public T Reserved8;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static T GetAllocatedLinks(IntPtr pointer) => (pointer + AllocatedLinksOffset).GetValue<T>();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static T GetReservedLinks(IntPtr pointer) => (pointer + ReservedLinksOffset).GetValue<T>();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static T GetFreeLinks(IntPtr pointer) => (pointer + FreeLinksOffset).GetValue<T>();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static T GetFirstFreeLink(IntPtr pointer) => (pointer + FirstFreeLinkOffset).GetValue<T>();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static T GetFirstAsSource(IntPtr pointer) => (pointer + FirstAsSourceOffset).GetValue<T>();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static T GetFirstAsTarget(IntPtr pointer) => (pointer + FirstAsTargetOffset).GetValue<T>();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static T GetLastFreeLink(IntPtr pointer) => (pointer + LastFreeLinkOffset).GetValue<T>();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static IntPtr GetFirstAsSourcePointer(IntPtr pointer) => pointer + FirstAsSourceOffset;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static IntPtr GetFirstAsTargetPointer(IntPtr pointer) => pointer + FirstAsTargetOffset;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetAllocatedLinks(IntPtr pointer, T value) => (pointer + AllocatedLinksOffset).SetValue(value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetReservedLinks(IntPtr pointer, T value) => (pointer + ReservedLinksOffset).SetValue(value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetFreeLinks(IntPtr pointer, T value) => (pointer + FreeLinksOffset).SetValue(value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetFirstFreeLink(IntPtr pointer, T value) => (pointer + FirstFreeLinkOffset).SetValue(value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetFirstAsSource(IntPtr pointer, T value) => (pointer + FirstAsSourceOffset).SetValue(value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetFirstAsTarget(IntPtr pointer, T value) => (pointer + FirstAsTargetOffset).SetValue(value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetLastFreeLink(IntPtr pointer, T value) => (pointer + LastFreeLinkOffset).SetValue(value);
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
        private T Total => Subtract(LinksHeader.GetAllocatedLinks(_header), LinksHeader.GetFreeLinks(_header));

        public LinksMemoryManager(string address)
            : this(address, DefaultLinksSizeStep)
        {
        }

        /// <summary>
        /// Создаёт экземпляр базы данных Links в файле по указанному адресу, с указанным минимальным шагом расширения базы данных.
        /// </summary>
        /// <param name="address">Полный пусть к файлу базы данных.</param>
        /// <param name="memoryReservationStep">Минимальный шаг расширения базы данных в байтах.</param>
        public LinksMemoryManager(string address, long memoryReservationStep)
            : this(new FileMappedResizableDirectMemory(address, memoryReservationStep), memoryReservationStep)
        {
        }

        public LinksMemoryManager(IResizableDirectMemory memory)
            : this(memory, DefaultLinksSizeStep)
        {
        }

        public LinksMemoryManager(IResizableDirectMemory memory, long memoryReservationStep)
        {
            _memory = memory;
            _memoryReservationStep = memoryReservationStep;

            if (memory.ReservedCapacity < memoryReservationStep)
                memory.ReservedCapacity = memoryReservationStep;

            SetPointers(_memory);

            // Гарантия корректности _memory.UsedCapacity относительно _header->AllocatedLinks
            _memory.UsedCapacity = (long)(Integer<T>)LinksHeader.GetAllocatedLinks(_header) * LinkSizeInBytes + LinkHeaderSizeInBytes;

            // Гарантия корректности _header->ReservedLinks относительно _memory.ReservedCapacity
            LinksHeader.SetReservedLinks(_header, (Integer<T>)((_memory.ReservedCapacity - LinkHeaderSizeInBytes) / LinkSizeInBytes));
        }

        // TODO: Дать возможность переопределять в конструкторе
        public ILinksCombinedConstants<bool, T, int> Constants { get; } = Default<LinksConstants<bool, T, int>>.Instance;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Count(IList<T> restrictions)
        {
            // Если нет ограничений, тогда возвращаем общее число связей находящихся в хранилище.
            if (restrictions.Count == 0)
                return Total;
            if (restrictions.Count == 1)
            {
                var index = restrictions[Constants.IndexPart];

                if (Equals(index, Constants.Any))
                    return Total;

                return Exists(index) ? (Integer<T>)1 : (Integer<T>)0;
            }
            if (restrictions.Count == 2)
            {
                var index = restrictions[Constants.IndexPart];
                var value = restrictions[1];

                if (Equals(index, Constants.Any))
                {
                    if (Equals(value, Constants.Any))
                        return Total; // Any - как отсутствие ограничения

                    return Add(_sourcesTreeMethods.CalculateReferences(value), _targetsTreeMethods.CalculateReferences(value));
                }
                else
                {
                    if (!Exists(index))
                        return (Integer<T>)0;

                    if (Equals(value, Constants.Any))
                        return (Integer<T>)1;

                    var storedLinkValue = GetLinkUnsafe(index);
                    if (Equals(Link.GetSource(storedLinkValue), value) ||
                        Equals(Link.GetTarget(storedLinkValue), value))
                        return (Integer<T>)1;
                    return (Integer<T>)0;
                }
            }
            if (restrictions.Count == 3)
            {
                var index = restrictions[Constants.IndexPart];
                var source = restrictions[Constants.SourcePart];
                var target = restrictions[Constants.TargetPart];

                if (Equals(index, Constants.Any))
                {
                    if (Equals(source, Constants.Any) && Equals(target, Constants.Any))
                        return Total;
                    else if (Equals(source, Constants.Any))
                        return _targetsTreeMethods.CalculateReferences(target);
                    else if (Equals(target, Constants.Any))
                        return _sourcesTreeMethods.CalculateReferences(source);
                    else //if(source != Any && target != Any)
                    {
                        // Эквивалент Exists(source, target) => Count(Any, source, target) > 0
                        var link = _sourcesTreeMethods.Search(source, target);
                        return Equals(link, Constants.Null) ? (Integer<T>)0 : (Integer<T>)1;
                    }
                }
                else
                {
                    if (!Exists(index))
                        return (Integer<T>)0;

                    if (Equals(source, Constants.Any) && Equals(target, Constants.Any))
                        return (Integer<T>)1;

                    var storedLinkValue = GetLinkUnsafe(index);

                    if (!Equals(source, Constants.Any) && !Equals(target, Constants.Any))
                    {
                        if (Equals(Link.GetSource(storedLinkValue), source) &&
                            Equals(Link.GetTarget(storedLinkValue), target))
                            return (Integer<T>)1;
                        return (Integer<T>)0;
                    }

                    var value = default(T);
                    if (Equals(source, Constants.Any)) value = target;
                    if (Equals(target, Constants.Any)) value = source;

                    if (Equals(Link.GetSource(storedLinkValue), value) ||
                        Equals(Link.GetTarget(storedLinkValue), value))
                        return (Integer<T>)1;
                    return (Integer<T>)0;
                }
            }
            throw new NotSupportedException("Другие размеры и способы ограничений не поддерживаются.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Each(Func<T, bool> handler, IList<T> restrictions)
        {
            if (restrictions.Count == 0)
            {
                for (T link = (Integer<T>)1; LessOrEqualThan(link, (T)(Integer<T>)LinksHeader.GetAllocatedLinks(_header)); link = Increment(link))
                    if (Exists(link))
                        if (handler(link) == Constants.Break)
                            return Constants.Break;

                return Constants.Continue;
            }
            if (restrictions.Count == 1)
            {
                var index = restrictions[Constants.IndexPart];

                if (Equals(index, Constants.Any))
                    return Each(handler, ArrayPool<T>.Empty);

                if (!Exists(index))
                    return Constants.Continue;

                return handler(index);
            }
            if (restrictions.Count == 2)
            {
                var index = restrictions[Constants.IndexPart];
                var value = restrictions[1];

                if (Equals(index, Constants.Any))
                {
                    if (Equals(value, Constants.Any))
                        return Each(handler, ArrayPool<T>.Empty);

                    if (Each(handler, new[] { index, value, Constants.Any }) == Constants.Break)
                        return Constants.Break;

                    return Each(handler, new[] { index, Constants.Any, value });
                }
                else
                {
                    if (!Exists(index))
                        return Constants.Continue;

                    if (Equals(value, Constants.Any))
                        return handler(index);

                    var storedLinkValue = GetLinkUnsafe(index);
                    if (Equals(Link.GetSource(storedLinkValue), value) ||
                        Equals(Link.GetTarget(storedLinkValue), value))
                        return handler(index);
                    return Constants.Continue;
                }
            }
            if (restrictions.Count == 3)
            {
                var index = restrictions[Constants.IndexPart];
                var source = restrictions[Constants.SourcePart];
                var target = restrictions[Constants.TargetPart];

                if (Equals(index, Constants.Any))
                {
                    if (Equals(source, Constants.Any) && Equals(target, Constants.Any))
                        return Each(handler, ArrayPool<T>.Empty);
                    else if (Equals(source, Constants.Any))
                        return _targetsTreeMethods.EachReference(target, handler);
                    else if (Equals(target, Constants.Any))
                        return _sourcesTreeMethods.EachReference(source, handler);
                    else //if(source != Any && target != Any)
                    {
                        var link = _sourcesTreeMethods.Search(source, target);

                        return Equals(link, Constants.Null) ? Constants.Continue : handler(link);
                    }
                }
                else
                {
                    if (!Exists(index))
                        return Constants.Continue;

                    if (Equals(source, Constants.Any) && Equals(target, Constants.Any))
                        return handler(index);

                    var storedLinkValue = GetLinkUnsafe(index);

                    if (!Equals(source, Constants.Any) && !Equals(target, Constants.Any))
                    {
                        if (Equals(Link.GetSource(storedLinkValue), source) &&
                            Equals(Link.GetTarget(storedLinkValue), target))
                            return handler(index);
                        return Constants.Continue;
                    }

                    var value = default(T);
                    if (Equals(source, Constants.Any)) value = target;
                    if (Equals(target, Constants.Any)) value = source;

                    if (Equals(Link.GetSource(storedLinkValue), value) ||
                        Equals(Link.GetTarget(storedLinkValue), value))
                        return handler(index);
                    return Constants.Continue;
                }
            }
            throw new NotSupportedException("Другие размеры и способы ограничений не поддерживаются.");
        }

        /// <remarks>
        /// TODO: Возможно можно перемещать значения, если указан индекс, но значение существует в другом месте (но не в менеджере памяти, а в логике Links)
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLinkValue(IList<T> values)
        {
            var linkIndex = values[Constants.IndexPart];
            var link = GetLinkUnsafe(linkIndex);

            // Будет корректно работать только в том случае, если пространство выделенной связи предварительно заполнено нулями
            if (!Equals(Link.GetSource(link), Constants.Null)) _sourcesTreeMethods.Detach(LinksHeader.GetFirstAsSourcePointer(_header), linkIndex);
            if (!Equals(Link.GetTarget(link), Constants.Null)) _targetsTreeMethods.Detach(LinksHeader.GetFirstAsTargetPointer(_header), linkIndex);

            Link.SetSource(link, values[Constants.SourcePart]);
            Link.SetTarget(link, values[Constants.TargetPart]);

            if (!Equals(Link.GetSource(link), Constants.Null)) _sourcesTreeMethods.Attach(LinksHeader.GetFirstAsSourcePointer(_header), linkIndex);
            if (!Equals(Link.GetTarget(link), Constants.Null)) _targetsTreeMethods.Attach(LinksHeader.GetFirstAsTargetPointer(_header), linkIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IList<T> GetLinkValue(T linkIndex)
        {
            return GetLinkStruct(linkIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Link<T> GetLinkStruct(T linkIndex)
        {
            var link = GetLinkUnsafe(linkIndex);
            return new Link<T>(linkIndex, Link.GetSource(link), Link.GetTarget(link));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IntPtr GetLinkUnsafe(T linkIndex)
        {
            return _links.GetElement(LinkSizeInBytes, linkIndex);
        }

        /// <remarks>
        /// TODO: Возможно нужно будет заполнение нулями, если внешнее API ими не заполняет пространство
        /// </remarks>
        public T AllocateLink()
        {
            var freeLink = LinksHeader.GetFirstFreeLink(_header);

            if (!Equals(freeLink, Constants.Null))
                _unusedLinksListMethods.Detach(freeLink);
            else
            {
                if (GreaterThan(LinksHeader.GetAllocatedLinks(_header), Constants.MaxPossibleIndex))
                    throw new LinksLimitReachedException((Integer<T>)Constants.MaxPossibleIndex);

                if (GreaterOrEqualThan(LinksHeader.GetAllocatedLinks(_header), Decrement(LinksHeader.GetReservedLinks(_header))))
                {
                    _memory.ReservedCapacity += _memoryReservationStep;
                    SetPointers(_memory);
                    LinksHeader.SetReservedLinks(_header, (Integer<T>)(_memory.ReservedCapacity / LinkSizeInBytes));
                }

                LinksHeader.SetAllocatedLinks(_header, Increment(LinksHeader.GetAllocatedLinks(_header)));
                _memory.UsedCapacity += LinkSizeInBytes;
                freeLink = LinksHeader.GetAllocatedLinks(_header);
            }

            return freeLink;
        }

        public void FreeLink(T link)
        {
            if (LessThan(link, LinksHeader.GetAllocatedLinks(_header)))
                _unusedLinksListMethods.AttachAsFirst(link);
            else if (Equals(link, LinksHeader.GetAllocatedLinks(_header)))
            {
                LinksHeader.SetAllocatedLinks(_header, Decrement(LinksHeader.GetAllocatedLinks(_header)));
                _memory.UsedCapacity -= LinkSizeInBytes;

                // Убираем все связи, находящиеся в списке свободных в конце файла, до тех пор, пока не дойдём до первой существующей связи
                // Позволяет оптимизировать количество выделенных связей (AllocatedLinks)
                while (GreaterThan(LinksHeader.GetAllocatedLinks(_header), (T)(Integer<T>)0) && IsUnusedLink(LinksHeader.GetAllocatedLinks(_header)))
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

                _sourcesTreeMethods = new LinksSourcesTreeMethods(_links, _header, Constants);
                _targetsTreeMethods = new LinksTargetsTreeMethods(_links, _header, Constants);
                _unusedLinksListMethods = new UnusedLinksListMethods(_links, _header);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Exists(T link)
        {
            return GreaterOrEqualThan(link, Constants.MinPossibleIndex) && LessOrEqualThan(link, LinksHeader.GetAllocatedLinks(_header)) && !IsUnusedLink(link);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsUnusedLink(T link)
        {
            return Equals(LinksHeader.GetFirstFreeLink(_header), link)
                   || (Equals(Link.GetSizeAsSource(GetLinkUnsafe(link)), Constants.Null) && !Equals(Link.GetSource(GetLinkUnsafe(link)), Constants.Null));
        }

        #region Disposable

        protected override bool AllowMultipleDisposeCalls => true;

        protected override void DisposeCore(bool manual)
        {
            SetPointers(null);
            if (manual)
                Disposable.TryDispose(_memory);
        }

        #endregion
    }
}