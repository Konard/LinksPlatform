using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Int = System.Int64;
using LinkIndex = System.UInt64;

namespace Platform.Links.DataBase.CoreNet.Triplets
{
    public struct LinkDefinition
    {
        public Link Source;
        public Link Linker;
        public Link Target;

        public LinkDefinition(Link source, Link linker, Link target)
        {
            Source = source;
            Linker = linker;
            Target = target;
        }

        public LinkDefinition(Link link)
            : this(link.Source, link.Linker, link.Target)
        {
        }
    }

    public unsafe partial struct Link : IEquatable<Link>
    {
        // TODO: Заменить на очередь событий, по примеру Node.js (+сделать выключаемым)
        public delegate void CreatedDelegate(LinkDefinition createdLink);
        public static event CreatedDelegate CreatedEvent = (createdLink) => { };

        public delegate void UpdatedDelegate(LinkDefinition linkBeforeUpdate, LinkDefinition linkAfterUpdate);
        public static event UpdatedDelegate UpdatedEvent = (linkBeforeUpdate, linkAfterUpdate) => { };

        public delegate void DeletedDelegate(LinkDefinition deletedLink);
        public static event DeletedDelegate DeletedEvent = (deletedLink) => { };

        #region Low Level

        #region Basic Operations

        [DllImport("Platform.Data.Kernel.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern LinkIndex GetSourceIndex(LinkIndex link);

        [DllImport("Platform.Data.Kernel.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern LinkIndex GetLinkerIndex(LinkIndex link);

        [DllImport("Platform.Data.Kernel.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern LinkIndex GetTargetIndex(LinkIndex link);

        [DllImport("Platform.Data.Kernel.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern LinkIndex GetFirstRefererBySourceIndex(LinkIndex link);

        [DllImport("Platform.Data.Kernel.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern LinkIndex GetFirstRefererByLinkerIndex(LinkIndex link);

        [DllImport("Platform.Data.Kernel.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern LinkIndex GetFirstRefererByTargetIndex(LinkIndex link);

        [DllImport("Platform.Data.Kernel.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern Int GetTime(LinkIndex link);

        [DllImport("Platform.Data.Kernel.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern LinkIndex CreateLink(LinkIndex source, LinkIndex linker, LinkIndex target);

        [DllImport("Platform.Data.Kernel.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern LinkIndex UpdateLink(LinkIndex link, LinkIndex newSource, LinkIndex newLinker, LinkIndex newTarget);

        [DllImport("Platform.Data.Kernel.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void DeleteLink(LinkIndex link);

        [DllImport("Platform.Data.Kernel.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern LinkIndex ReplaceLink(LinkIndex link, LinkIndex replacement);

        [DllImport("Platform.Data.Kernel.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern LinkIndex SearchLink(LinkIndex source, LinkIndex linker, LinkIndex target);

        [DllImport("Platform.Data.Kernel.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern LinkIndex GetMappedLink(Int mappedIndex);

        [DllImport("Platform.Data.Kernel.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetMappedLink(Int mappedIndex, LinkIndex linkIndex);

        [DllImport("Platform.Data.Kernel.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void InitPersistentMemoryManager();

        [DllImport("Platform.Data.Kernel.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern Int OpenStorageFile(string filename);

        [DllImport("Platform.Data.Kernel.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern Int CloseStorageFile();

        [DllImport("Platform.Data.Kernel.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern Int SetStorageFileMemoryMapping();

        [DllImport("Platform.Data.Kernel.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern Int ResetStorageFileMemoryMapping();

        #endregion

        #region Referers Count Selectors

        [DllImport("Platform.Data.Kernel.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern LinkIndex GetLinkNumberOfReferersBySource(LinkIndex link);

        [DllImport("Platform.Data.Kernel.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern LinkIndex GetLinkNumberOfReferersByLinker(LinkIndex link);

        [DllImport("Platform.Data.Kernel.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern LinkIndex GetLinkNumberOfReferersByTarget(LinkIndex link);

        #endregion

        #region Referers Walkers

        private delegate void Visitor(LinkIndex link);
        private delegate Int StopableVisitor(LinkIndex link);

        [DllImport("Platform.Data.Kernel.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void WalkThroughAllReferersBySource(LinkIndex root, Visitor action);

        [DllImport("Platform.Data.Kernel.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int WalkThroughReferersBySource(LinkIndex root, StopableVisitor func);

        [DllImport("Platform.Data.Kernel.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void WalkThroughAllReferersByLinker(LinkIndex root, Visitor action);

        [DllImport("Platform.Data.Kernel.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int WalkThroughReferersByLinker(LinkIndex root, StopableVisitor func);

        [DllImport("Platform.Data.Kernel.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void WalkThroughAllReferersByTarget(LinkIndex root, Visitor action);

        [DllImport("Platform.Data.Kernel.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int WalkThroughReferersByTarget(LinkIndex root, StopableVisitor func);

        [DllImport("Platform.Data.Kernel.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void WalkThroughAllLinks(Visitor action);

        [DllImport("Platform.Data.Kernel.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int WalkThroughLinks(StopableVisitor func);

        #endregion

        #endregion

        #region Constains

        public static readonly Link Itself = null;
        public static readonly bool Continue = true;
        public static readonly bool Stop = false;

        #endregion

        #region Static Fields

        private static readonly object LockObject = new object();
        private static bool MemoryManagerIsReady = false;
        private static readonly Dictionary<ulong, long> LinkToMappingIndex = new Dictionary<ulong, long>();

        #endregion

        #region Fields

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly LinkIndex _link;

        #endregion

        #region Properties

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Link Source { get { return GetSourceIndex(_link); } }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Link Linker { get { return GetLinkerIndex(_link); } }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Link Target { get { return GetTargetIndex(_link); } }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Link FirstRefererBySource { get { return GetFirstRefererBySourceIndex(_link); } }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Link FirstRefererByLinker { get { return GetFirstRefererByLinkerIndex(_link); } }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Link FirstRefererByTarget { get { return GetFirstRefererByTargetIndex(_link); } }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Int ReferersBySourceCount { get { return (Int)GetLinkNumberOfReferersBySource(_link); } }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Int ReferersByLinkerCount { get { return (Int)GetLinkNumberOfReferersByLinker(_link); } }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Int ReferersByTargetCount { get { return (Int)GetLinkNumberOfReferersByTarget(_link); } }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Int TotalReferers { get { return (Int)GetLinkNumberOfReferersBySource(_link) + (Int)GetLinkNumberOfReferersByLinker(_link) + (Int)GetLinkNumberOfReferersByTarget(_link); } }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public DateTime Timestamp { get { return DateTime.FromFileTimeUtc(GetTime(_link)); } }

        #endregion

        #region Infrastructure

        public Link(LinkIndex link)
        {
            _link = link;
        }

        public static void StartMemoryManager(string storageFilename)
        {
            lock (LockObject)
            {
                if (!MemoryManagerIsReady)
                {
                    InitPersistentMemoryManager();
                    if (OpenStorageFile(storageFilename) == 0)
                        throw new Exception("Файл хранилища с указанным именем не может быть открыт.");
                    if (SetStorageFileMemoryMapping() == 0)
                        throw new Exception(string.Format(
                            "Файл ({0}) хранилища не удалось отразить на оперативную память.", storageFilename));

                    MemoryManagerIsReady = true;
                }
            }
        }

        public static void StopMemoryManager()
        {
            lock (LockObject)
            {
                if (MemoryManagerIsReady)
                {
                    if (ResetStorageFileMemoryMapping() == 0)
                        throw new Exception("Отображение файла хранилища на оперативную память не удалось снять.");
                    if (CloseStorageFile() == 0)
                        throw new Exception(
                            "Файл хранилища не удалось закрыть, возможно он был уже закрыт, или не открывался вовсе.");

                    MemoryManagerIsReady = false;
                }
            }
        }

        public static implicit operator LinkIndex?(Link link)
        {
            return link._link == 0 ? (LinkIndex?)null : link._link;
        }

        public static implicit operator Link(LinkIndex? link)
        {
            return new Link(link == null ? 0 : (LinkIndex)link);
        }

        public static implicit operator Int(Link link)
        {
            return (Int)link._link;
        }

        public static implicit operator Link(Int link)
        {
            return new Link((LinkIndex)link);
        }

        public static implicit operator LinkIndex(Link link)
        {
            return link._link;
        }

        public static implicit operator Link(LinkIndex link)
        {
            return new Link(link);
        }

        public static explicit operator Link(List<Link> links)
        {
            return LinkConverter.FromList(links);
        }

        public static explicit operator Link(Link[] links)
        {
            return LinkConverter.FromList(links);
        }

        public static explicit operator Link(string @string)
        {
            return LinkConverter.FromString(@string);
        }

        public static bool operator ==(Link first, Link second)
        {
            return first.Equals(second);
        }

        public static bool operator !=(Link first, Link second)
        {
            return !first.Equals(second);
        }

        public static Link operator &(Link first, Link second)
        {
            return Create(first, Net.And, second);
        }

        public override bool Equals(object obj)
        {
            return Equals((Link)obj);
        }

        public bool Equals(Link other)
        {
            return _link == other._link || (LinkDoesNotExist(_link) && LinkDoesNotExist(other._link));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        private static bool LinkDoesNotExist(LinkIndex link)
        {
            return link == 0 || GetLinkerIndex(link) == 0;
        }

        private static bool LinkWasDeleted(LinkIndex link)
        {
            return link != 0 && GetLinkerIndex(link) == 0;
        }

        private bool IsMatchingTo(Link source, Link linker, Link target)
        {
            return ((Source == this && source == null) || (Source == source))
                && ((Linker == this && linker == null) || (Linker == linker))
                && ((Target == this && target == null) || (Target == target));
        }

        public LinkIndex ToIndex()
        {
            return _link;
        }

        public Int ToInt()
        {
            return (Int)_link;
        }

        #endregion

        #region Basic Operations

        public static Link Create(Link source, Link linker, Link target)
        {
            if (!MemoryManagerIsReady)
                throw new Exception("Менеджер памяти ещё не готов.");
            if (LinkWasDeleted(source))
                throw new ArgumentException("Удалённая связь не может использоваться в качестве значения.", "source");
            if (LinkWasDeleted(linker))
                throw new ArgumentException("Удалённая связь не может использоваться в качестве значения.", "linker");
            if (LinkWasDeleted(target))
                throw new ArgumentException("Удалённая связь не может использоваться в качестве значения.", "target");
            Link link = CreateLink(source, linker, target);
            if (link == null)
                throw new OutOfMemoryException();

            CreatedEvent.Invoke(new LinkDefinition(link));

            return link;
        }

        public static Link Restore(Int index)
        {
            return Restore((LinkIndex)index);
        }

        public static Link Restore(LinkIndex index)
        {
            if (!MemoryManagerIsReady)
                throw new Exception("Менеджер памяти ещё не готов.");

            if (index == 0)
                throw new ArgumentException("У связи не может быть нулевого адреса.");

            try
            {
                Link link = index;
                if (LinkDoesNotExist(link))
                    throw new Exception("Связь с указанным адресом удалена, либо не существовала.");
                return link;
            }
            catch (Exception ex)
            {
                throw new Exception("Указатель не является корректным.", ex);
            }
        }

        public static Link CreateMapped(Link source, Link linker, Link target, object mappingIndex)
        {
            return CreateMapped(source, linker, target, Convert.ToInt64(mappingIndex));
        }

        public static Link CreateMapped(Link source, Link linker, Link target, Int mappingIndex)
        {
            if (!MemoryManagerIsReady)
                throw new Exception("Менеджер памяти ещё не готов.");

            Link mappedLink = GetMappedLink(mappingIndex);

            if (mappedLink == null)
            {
                mappedLink = Create(source, linker, target);
                SetMappedLink(mappingIndex, mappedLink);

                if (GetMappedLink(mappingIndex) != mappedLink)
                    throw new Exception("Установить привязанную связь не удалось.");
            }
            else if (!mappedLink.IsMatchingTo(source, linker, target))
            {
                throw new Exception("Существующая привязанная связь не соответствует указанным Source, Linker и Target.");
            }

            LinkToMappingIndex[mappedLink] = mappingIndex;

            return mappedLink;
        }

        public static bool TrySetMapped(Link link, Int mappingIndex, bool rewrite = false)
        {
            Link mappedLink = GetMappedLink(mappingIndex);

            if (mappedLink == null || rewrite)
            {
                mappedLink = link;
                SetMappedLink(mappingIndex, mappedLink);

                if (GetMappedLink(mappingIndex) != mappedLink)
                    return false;
            }
            else if (!mappedLink.IsMatchingTo(link.Source, link.Linker, link.Target))
            {
                return false;
            }

            LinkToMappingIndex[mappedLink] = mappingIndex;

            return true;
        }

        public static Link GetMapped(object mappingIndex)
        {
            return GetMapped(Convert.ToInt64(mappingIndex));
        }

        public static Link GetMapped(Int mappingIndex)
        {
            Link mappedLink;
            if (!TryGetMapped(mappingIndex, out mappedLink))
                throw new Exception(string.Format("Mapped link with index {0} is not set.", mappingIndex));
            return mappedLink;
        }

        public static bool TryGetMapped(object mappingIndex, out Link mappedLink)
        {
            return TryGetMapped(Convert.ToInt64(mappingIndex), out mappedLink);
        }

        public static bool TryGetMapped(Int mappingIndex, out Link mappedLink)
        {
            if (!MemoryManagerIsReady)
                throw new Exception("Менеджер памяти ещё не готов.");

            mappedLink = GetMappedLink(mappingIndex);
            if (mappedLink != null)
                LinkToMappingIndex[mappedLink] = mappingIndex;
            return mappedLink != null;
        }

        public static void Update(ref Link link, Link newSource, Link newLinker, Link newTarget)
        {
            if (!MemoryManagerIsReady)
                throw new Exception("Менеджер памяти ещё не готов.");
            if (LinkDoesNotExist(link))
                throw new ArgumentException("Нельзя обновить несуществующую связь.", "link");
            if (LinkWasDeleted(newSource))
                throw new ArgumentException("Удалённая связь не может использоваться в качестве нового значения.", "newSource");
            if (LinkWasDeleted(newLinker))
                throw new ArgumentException("Удалённая связь не может использоваться в качестве нового значения.", "newLinker");
            if (LinkWasDeleted(newTarget))
                throw new ArgumentException("Удалённая связь не может использоваться в качестве нового значения.", "newTarget");

            LinkIndex previousLinkIndex = link;
            Int mappingIndex;
            LinkToMappingIndex.TryGetValue(link, out mappingIndex);
            var previousDefinition = new LinkDefinition(link);

            link = UpdateLink(link, newSource, newLinker, newTarget);

            if (mappingIndex >= 0 && previousLinkIndex != link)
            {
                LinkToMappingIndex.Remove(previousLinkIndex);
                SetMappedLink(mappingIndex, link);
                LinkToMappingIndex.Add(link, mappingIndex);
            }
            UpdatedEvent(previousDefinition, new LinkDefinition(link));
        }

        public static void Delete(ref Link link)
        {
            if (LinkDoesNotExist(link))
                return;

            LinkIndex previousLinkIndex = link;
            Int mappingIndex;
            LinkToMappingIndex.TryGetValue(link, out mappingIndex);
            var previousDefinition = new LinkDefinition(link);

            DeleteLink(link);
            link = null;

            if (mappingIndex >= 0)
            {
                LinkToMappingIndex.Remove(previousLinkIndex);
                SetMappedLink(mappingIndex, 0);
            }
            DeletedEvent(previousDefinition);
        }

        //public static void Replace(ref Link link, Link replacement)
        //{
        //    if (!MemoryManagerIsReady)
        //        throw new Exception("Менеджер памяти ещё не готов.");
        //    if (LinkDoesNotExist(link))
        //        throw new Exception("Если связь не существует, её нельзя заменить.");
        //    if (LinkDoesNotExist(replacement))
        //        throw new ArgumentException("Пустая или удалённая связь не может быть замещаемым значением.", "replacement");

        //    link = ReplaceLink(link, replacement);
        //}

        public static Link Search(Link source, Link linker, Link target)
        {
            if (!MemoryManagerIsReady)
                throw new Exception("Менеджер памяти ещё не готов.");
            if (LinkDoesNotExist(source) || LinkDoesNotExist(linker) || LinkDoesNotExist(target))
                throw new Exception("Выполнить поиск связи можно только по существующим связям.");
            return SearchLink(source, linker, target);
        }

        public static bool Exists(Link source, Link linker, Link target)
        {
            return SearchLink(source, linker, target) != 0;
        }

        #endregion

        #region Referers Walkers

        public bool WalkThroughReferersBySource(Func<Link, bool> walker)
        {
            if (LinkDoesNotExist(this))
                throw new Exception("C несуществующей связью нельзя производитить операции.");

            long referers = ReferersBySourceCount;
            if (referers == 1)
            {
                return walker(FirstRefererBySource);
            }
            else if (referers > 1)
            {
                return WalkThroughReferersBySource(this, x => walker(x) ? 1 : 0) != 0;
            }
            else
            {
                return true;
            }
        }

        public void WalkThroughReferersBySource(Action<Link> walker)
        {
            if (LinkDoesNotExist(this))
                throw new Exception("C несуществующей связью нельзя производитить операции.");

            long referers = ReferersBySourceCount;
            if (referers == 1)
            {
                walker(FirstRefererBySource);
            }
            else if (referers > 1)
            {
                WalkThroughAllReferersBySource(this, x => walker(x));
            }
        }

        public bool WalkThroughReferersByLinker(Func<Link, bool> walker)
        {
            if (LinkDoesNotExist(this))
                throw new Exception("C несуществующей связью нельзя производитить операции.");

            long referers = ReferersByLinkerCount;
            if (referers == 1)
            {
                return walker(FirstRefererByLinker);
            }
            else if (referers > 1)
            {
                return WalkThroughReferersByLinker(this, x => walker(x) ? 1 : 0) != 0;
            }
            else
            {
                return true;
            }
        }

        public void WalkThroughReferersByLinker(Action<Link> walker)
        {
            if (LinkDoesNotExist(this))
                throw new Exception("C несуществующей связью нельзя производитить операции.");

            long referers = ReferersByLinkerCount;
            if (referers == 1)
            {
                walker(FirstRefererByLinker);
            }
            else if (referers > 1)
            {
                WalkThroughAllReferersByLinker(this, x => walker(x));
            }
        }

        public bool WalkThroughReferersByTarget(Func<Link, bool> walker)
        {
            if (LinkDoesNotExist(this))
                throw new Exception("C несуществующей связью нельзя производитить операции.");

            long referers = ReferersByTargetCount;
            if (referers == 1)
            {
                return walker(FirstRefererByTarget);
            }
            else if (referers > 1)
            {
                return WalkThroughReferersByTarget(this, x => walker(x) ? 1 : 0) != 0;
            }
            else
            {
                return true;
            }
        }

        public void WalkThroughReferersByTarget(Action<Link> walker)
        {
            if (LinkDoesNotExist(this))
                throw new Exception("C несуществующей связью нельзя производитить операции.");

            long referers = ReferersByTargetCount;
            if (referers == 1)
            {
                walker(FirstRefererByTarget);
            }
            else if (referers > 1)
            {
                WalkThroughAllReferersByTarget(this, x => walker(x));
            }
        }

        public void WalkThroughReferers(Action<Link> walker)
        {
            if (LinkDoesNotExist(this))
                throw new Exception("C несуществующей связью нельзя производитить операции.");

            Visitor wrapper = x => walker(x);

            WalkThroughAllReferersBySource(this, wrapper);
            WalkThroughAllReferersByLinker(this, wrapper);
            WalkThroughAllReferersByTarget(this, wrapper);
        }

        public void WalkThroughReferers(Func<Link, bool> walker)
        {
            if (LinkDoesNotExist(this))
                throw new Exception("C несуществующей связью нельзя производитить операции.");

            StopableVisitor wrapper = x => walker(x) ? 1 : 0;

            WalkThroughReferersBySource(this, wrapper);
            WalkThroughReferersByLinker(this, wrapper);
            WalkThroughReferersByTarget(this, wrapper);
        }

        public static bool WalkThroughAllLinks(Func<Link, bool> walker)
        {
            return WalkThroughLinks(x => walker(x) ? 1 : 0) != 0;
        }

        public static void WalkThroughAllLinks(Action<Link> walker)
        {
            WalkThroughAllLinks(new Visitor(x => walker(x)));
        }

        #endregion
    }
}
