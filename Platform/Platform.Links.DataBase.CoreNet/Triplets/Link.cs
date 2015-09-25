using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

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

        [DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 GetSourceIndex(UInt64 link);

        [DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 GetLinkerIndex(UInt64 link);

        [DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 GetTargetIndex(UInt64 link);

        [DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 GetFirstRefererBySourceIndex(UInt64 link);

        [DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 GetFirstRefererByLinkerIndex(UInt64 link);

        [DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 GetFirstRefererByTargetIndex(UInt64 link);

        [DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern Int64 GetTime(UInt64 link);

        [DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 CreateLink(UInt64 source, UInt64 linker, UInt64 target);

        [DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 UpdateLink(UInt64 link, UInt64 newSource, UInt64 newLinker, UInt64 newTarget);

        [DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void DeleteLink(UInt64 link);

        [DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 ReplaceLink(UInt64 link, UInt64 replacement);

        [DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 SearchLink(UInt64 source, UInt64 linker, UInt64 target);

        [DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 GetMappedLink(Int64 mappedIndex);

        [DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetMappedLink(Int64 mappedIndex, UInt64 linkIndex);

        [DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void InitPersistentMemoryManager();

        [DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern Int64 OpenStorageFile(string filename);

        [DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern Int64 CloseStorageFile();

        [DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern Int64 SetStorageFileMemoryMapping();

        [DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern Int64 ResetStorageFileMemoryMapping();

        #endregion

        #region Referers Count Selectors

        [DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 GetLinkNumberOfReferersBySource(UInt64 link);

        [DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 GetLinkNumberOfReferersByLinker(UInt64 link);

        [DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 GetLinkNumberOfReferersByTarget(UInt64 link);

        #endregion

        #region Referers Walkers

        private delegate void Visitor(UInt64 link);
        private delegate Int64 StopableVisitor(UInt64 link);

        [DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void WalkThroughAllReferersBySource(UInt64 root, Visitor action);

        [DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int WalkThroughReferersBySource(UInt64 root, StopableVisitor func);

        [DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void WalkThroughAllReferersByLinker(UInt64 root, Visitor action);

        [DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int WalkThroughReferersByLinker(UInt64 root, StopableVisitor func);

        [DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void WalkThroughAllReferersByTarget(UInt64 root, Visitor action);

        [DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int WalkThroughReferersByTarget(UInt64 root, StopableVisitor func);

        [DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void WalkThroughAllLinks(Visitor action);

        [DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
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
        private readonly UInt64 _link;

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
        public Int64 ReferersBySourceCount { get { return (Int64)GetLinkNumberOfReferersBySource(_link); } }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Int64 ReferersByLinkerCount { get { return (Int64)GetLinkNumberOfReferersByLinker(_link); } }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Int64 ReferersByTargetCount { get { return (Int64)GetLinkNumberOfReferersByTarget(_link); } }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Int64 TotalReferers { get { return (Int64)GetLinkNumberOfReferersBySource(_link) + (Int64)GetLinkNumberOfReferersByLinker(_link) + (Int64)GetLinkNumberOfReferersByTarget(_link); } }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public DateTime Timestamp { get { return DateTime.FromFileTimeUtc(GetTime(_link)); } }

        #endregion

        #region Infrastructure

        public Link(UInt64 link)
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

        public static implicit operator UInt64?(Link link)
        {
            return link._link == 0 ? (UInt64?)null : link._link;
        }

        public static implicit operator Link(UInt64? link)
        {
            return new Link(link == null ? 0 : (UInt64)link);
        }

        public static implicit operator Int64(Link link)
        {
            return (Int64)link._link;
        }

        public static implicit operator Link(Int64 link)
        {
            return new Link((UInt64)link);
        }

        public static implicit operator UInt64(Link link)
        {
            return link._link;
        }

        public static implicit operator Link(UInt64 link)
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

        private static bool LinkDoesNotExist(UInt64 link)
        {
            return link == 0 || GetLinkerIndex(link) == 0;
        }

        private static bool LinkWasDeleted(UInt64 link)
        {
            return link != 0 && GetLinkerIndex(link) == 0;
        }

        private bool IsMatchingTo(Link source, Link linker, Link target)
        {
            return ((Source == this && source == null) || (Source == source))
                && ((Linker == this && linker == null) || (Linker == linker))
                && ((Target == this && target == null) || (Target == target));
        }

        public UInt64 ToIndex()
        {
            return _link;
        }

        public Int64 ToInt()
        {
            return (Int64)_link;
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

        public static Link Restore(Int64 index)
        {
            return Restore((UInt64)index);
        }

        public static Link Restore(UInt64 index)
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

        public static Link CreateMapped(Link source, Link linker, Link target, Int64 mappingIndex)
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

        public static bool TrySetMapped(Link link, Int64 mappingIndex, bool rewrite = false)
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

        public static Link GetMapped(Int64 mappingIndex)
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

        public static bool TryGetMapped(Int64 mappingIndex, out Link mappedLink)
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

            UInt64 previousLinkIndex = link;
            Int64 mappingIndex;
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

            UInt64 previousLinkIndex = link;
            Int64 mappingIndex;
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
