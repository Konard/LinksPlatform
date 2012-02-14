using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetLibrary
{
	public struct LinkDefinition
	{
		public Link Source;
		public Link Linker;
		public Link Target;

		public LinkDefinition(Link source, Link linker, Link target)
		{
			this.Source = source;
			this.Linker = linker;
			this.Target = target;
		}
	}

	public unsafe partial struct Link : IEquatable<Link>
	{
		public delegate void CreatedDelegate(LinkDefinition createdLink);
		static public event CreatedDelegate CreatedEvent = (createdLink) => { };

		public delegate void UpdatedDelegate(LinkDefinition linkBeforeUpdate, LinkDefinition linkAfterUpdate);
		static public event UpdatedDelegate UpdatedEvent = (linkBeforeUpdate, linkAfterUpdate) => { };

		public delegate void DeletedDelegate(LinkDefinition deletedLink);
		static public event DeletedDelegate DeletedEvent = (deletedLink) => { };

		#region Low Level

		public struct __link
		{
			public __link* Source;
			public __link* Linker;
			public __link* Target;
			public __link* FirstRefererBySource;
			public __link* FirstRefererByLinker;
			public __link* FirstRefererByTarget;
			public __link* NextSiblingRefererBySource;
			public __link* NextSiblingRefererByLinker;
			public __link* NextSiblingRefererByTarget;
			public __link* PreviousSiblingRefererBySource;
			public __link* PreviousSiblingRefererByLinker;
			public __link* PreviousSiblingRefererByTarget;
			public ulong ReferersBySourceCount;
			public ulong ReferersByLinkerCount;
			public ulong ReferersByTargetCount;
			public long Timestamp;
		}

		#region Basic Operations

		[DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
		static private extern __link* CreateLink(__link* source, __link* linker, __link* target);

		[DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
		static private extern __link* UpdateLink(__link* link, __link* newSource, __link* newLinker, __link* newTarget);

		[DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
		static private extern void DeleteLink(__link* link);

		[DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
		static private extern __link* ReplaceLink(__link* link, __link* replacement);

		[DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
		static private extern __link* SearchLink(__link* source, __link* linker, __link* target);

		[DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
		static private extern __link* GetMappedLink(int index);

		[DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
		static private extern void SetMappedLink(int index, __link* link);

		[DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
		static private extern void InitPersistentMemoryManager();

		[DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
		static private extern ulong OpenStorageFile(string filename);

		[DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
		static private extern ulong CloseStorageFile();

		[DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
		static private extern ulong SetStorageFileMemoryMapping();

		[DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
		static private extern ulong ResetStorageFileMemoryMapping();

		#endregion

		#region Referers Count Selectors

#if x64
        [DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
        static private extern ulong GetLinkNumberOfReferersBySource(__link* link);

        [DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
        static private extern ulong GetLinkNumberOfReferersByLinker(__link* link);

        [DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
        static private extern ulong GetLinkNumberOfReferersByTarget(__link* link);
#else
		[DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
		static private extern uint GetLinkNumberOfReferersBySource(__link* link);

		[DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
		static private extern uint GetLinkNumberOfReferersByLinker(__link* link);

		[DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
		static private extern uint GetLinkNumberOfReferersByTarget(__link* link);
#endif

		#endregion

		#region Referers Walkers

		private delegate void Visitor(__link* link);
		private delegate int StopableVisitor(__link* link);

		[DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
		static private extern void WalkThroughAllReferersBySource(__link* root, Visitor action);

		[DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
		static private extern int WalkThroughReferersBySource(__link* root, StopableVisitor func);

		[DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
		static private extern void WalkThroughAllReferersByLinker(__link* root, Visitor action);

		[DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
		static private extern int WalkThroughReferersByLinker(__link* root, StopableVisitor func);

		[DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
		static private extern void WalkThroughAllReferersByTarget(__link* root, Visitor action);

		[DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
		static private extern int WalkThroughReferersByTarget(__link* root, StopableVisitor func);

		[DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
		static private extern void WalkThroughAllLinks(Visitor action);

		[DllImport("Core.dll", CallingConvention = CallingConvention.Cdecl)]
		static private extern int WalkThroughLinks(StopableVisitor func);

		#endregion

		#endregion

		#region Constains

		static public readonly Link Itself = null;
		static public readonly bool Continue = true;
		static public readonly bool Stop = false;

		#endregion

		#region Static Fields

		static private bool MemoryManagerIsReady = false;
		static private Dictionary<IntPtr, int> LinkToMappingIndex = new Dictionary<IntPtr, int>();

		#endregion

		#region Fields

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly __link* _link;

		#endregion

		#region Properties

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public Link Source { get { return _link->Source; } }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public Link Linker { get { return _link->Linker; } }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public Link Target { get { return _link->Target; } }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public Link FirstRefererBySource { get { return _link->FirstRefererBySource; } }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public Link FirstRefererByLinker { get { return _link->FirstRefererByLinker; } }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public Link FirstRefererByTarget { get { return _link->FirstRefererByTarget; } }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public long ReferersBySourceCount { get { return (long)GetLinkNumberOfReferersBySource(this); } }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public long ReferersByLinkerCount { get { return (long)GetLinkNumberOfReferersByLinker(this); } }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public long ReferersByTargetCount { get { return (long)GetLinkNumberOfReferersByTarget(this); } }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public long TotalReferers { get { return (long)GetLinkNumberOfReferersBySource(this) + (long)GetLinkNumberOfReferersByLinker(this) + (long)GetLinkNumberOfReferersByTarget(this); } }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public DateTime Timestamp { get { return DateTime.FromFileTimeUtc(_link->Timestamp); } }

		#endregion

		#region Infrastructure

		public Link(__link* link)
		{
			_link = link;
		}

		static public void StartMemoryManager(string storageFilename)
		{
			InitPersistentMemoryManager();
			if (OpenStorageFile(storageFilename) != 0)
				throw new Exception("Файл хранилища с указанным именем не может быть открыт.");
			if (SetStorageFileMemoryMapping() != 0)
				throw new Exception(string.Format("Файл ({0}) хранилища не удалось отразить на оперативную память.", storageFilename));

			Link.MemoryManagerIsReady = true;
		}

		static public void StopMemoryManager()
		{
			if (CloseStorageFile() != 0)
				throw new Exception("Файл хранилища не удалось закрыть, возможно он был уже закрыт, или не открывался вовсе.");
			if (ResetStorageFileMemoryMapping() != 0)
				throw new Exception("Отображение файла хранилища на оперативную память не удалось снять.");

			Link.MemoryManagerIsReady = false;
		}

		static public implicit operator __link*(Link link)
		{
			return link._link;
		}

		static public implicit operator Link(__link* link)
		{
			return new Link(link);
		}

		static public explicit operator Link(List<Link> links)
		{
			return LinkConverter.FromList(links);
		}

		static public explicit operator Link(Link[] links)
		{
			return LinkConverter.FromList(links);
		}

		static public explicit operator Link(string @string)
		{
			return LinkConverter.FromString(@string);
		}

		static public bool operator ==(Link first, Link second)
		{
			return first.Equals(second);
		}

		static public bool operator !=(Link first, Link second)
		{
			return !first.Equals(second);
		}

		static public Link operator &(Link first, Link second)
		{
			return Link.Create(first, Net.And, second);
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

		static private bool LinkDoesNotExist(__link* link)
		{
			return link == null || link->Source == null;
		}

		static private bool LinkWasDeleted(__link* link)
		{
			return link != null && link->Source == null;
		}

		private bool IsMatchingTo(Link source, Link linker, Link target)
		{
			return ((this.Source == this && source == null) || (this.Source == source))
				&& ((this.Linker == this && linker == null) || (this.Linker == linker))
				&& ((this.Target == this && target == null) || (this.Target == target));
		}

		public IntPtr GetPointer()
		{
			return new IntPtr(_link);
		}

		#endregion

		#region Basic Operations

		static public Link Create(Link source, Link linker, Link target)
		{
			if (!MemoryManagerIsReady)
				throw new Exception("Менеджер памяти ещё не готов.");
			if (LinkWasDeleted(source))
				throw new ArgumentException("Удалённая связь не может использоваться в качестве значения.", "source");
			if (LinkWasDeleted(linker))
				throw new ArgumentException("Удалённая связь не может использоваться в качестве значения.", "linker");
			if (LinkWasDeleted(target))
				throw new ArgumentException("Удалённая связь не может использоваться в качестве значения.", "target");
			__link* link = CreateLink(source, linker, target);
			if (link == null)
				throw new OutOfMemoryException();

			Parallel.Invoke(() => CreatedEvent.Invoke(new LinkDefinition(source, linker, target)));
			
			return link;
		}

		static public Link Create(IntPtr rawPointer)
		{
			if (!MemoryManagerIsReady)
				throw new Exception("Менеджер памяти ещё не готов.");

			if (rawPointer == IntPtr.Zero)
				throw new ArgumentException("У связи не может быть нулевого адреса.");
			try
			{
				Link link = (__link*)rawPointer.ToPointer();
				if (LinkDoesNotExist(link))
					throw new Exception("Связь с указанным адресом удалена, либо не существовала.");
				return link;
			}
			catch (Exception ex)
			{
				throw new Exception("Указатель не является корректным.", ex);
			}
		}

		static public Link CreateMapped(Link source, Link linker, Link target, object mappingIndex)
		{
			return CreateMapped(source, linker, target, (int)mappingIndex);
		}

		static public Link CreateMapped(Link source, Link linker, Link target, int mappingIndex)
		{
			if (!MemoryManagerIsReady)
				throw new Exception("Менеджер памяти ещё не готов.");

			Link mappedLink = GetMappedLink(mappingIndex);

			if (mappedLink == null)
			{
				mappedLink = Create(source, linker, target);
				SetMappedLink(mappingIndex, mappedLink);
			}
			else if (!mappedLink.IsMatchingTo(source, linker, target))
			{
				throw new Exception("Существующая привязанная связь не соответствует указанным Source, Linker и Target.");
			}

			LinkToMappingIndex[mappedLink.GetPointer()] = mappingIndex;

			return mappedLink;
		}

		static public Link GetMapped(object mappingIndex)
		{
			return GetMapped((int)mappingIndex);
		}

		static public Link GetMapped(int mappingIndex)
		{
			Link mappedLink;
			if (!TryGetMapped(mappingIndex, out mappedLink))
				throw new Exception(string.Format("Mapped link with index {0} is not set.", mappingIndex));
			return mappedLink;
		}

		static public bool TryGetMapped(object mappingIndex, out Link mappedLink)
		{
			return TryGetMapped((int)mappingIndex, out mappedLink);
		}

		static public bool TryGetMapped(int mappingIndex, out Link mappedLink)
		{
			mappedLink = GetMappedLink(mappingIndex);
			if (mappedLink != null)
				LinkToMappingIndex[mappedLink.GetPointer()] = mappingIndex;
			return mappedLink != null;
		}

		static public void Update(ref Link link, Link newSource, Link newLinker, Link newTarget)
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

			IntPtr previousLinkPointer = link.GetPointer();
			int mappingIndex = -1;
			LinkToMappingIndex.TryGetValue(previousLinkPointer, out mappingIndex);

			link = UpdateLink(link, newSource, newLinker, newTarget);

			if (mappingIndex >= 0 && previousLinkPointer != link.GetPointer())
			{
				LinkToMappingIndex.Remove(previousLinkPointer);
				SetMappedLink(mappingIndex, link);
				LinkToMappingIndex.Add(link.GetPointer(), mappingIndex);
			}
		}

		static public void Delete(ref Link link)
		{
			if (LinkDoesNotExist(link))
				return;

			IntPtr previousLinkPointer = link.GetPointer();
			int mappingIndex = -1;
			LinkToMappingIndex.TryGetValue(previousLinkPointer, out mappingIndex);

			DeleteLink(link);
			link = null;

			if (mappingIndex >= 0)
			{
				LinkToMappingIndex.Remove(previousLinkPointer);
				SetMappedLink(mappingIndex, null);
			}
		}

		//static public void Replace(ref Link link, Link replacement)
		//{
		//    if (!MemoryManagerIsReady)
		//        throw new Exception("Менеджер памяти ещё не готов.");
		//    if (LinkDoesNotExist(link))
		//        throw new Exception("Если связь не существует, её нельзя заменить.");
		//    if (LinkDoesNotExist(replacement))
		//        throw new ArgumentException("Пустая или удалённая связь не может быть замещаемым значением.", "replacement");

		//    link = ReplaceLink(link, replacement);
		//}

		static public Link Search(Link source, Link linker, Link target)
		{
			if (!MemoryManagerIsReady)
				throw new Exception("Менеджер памяти ещё не готов.");
			if (LinkDoesNotExist(source) || LinkDoesNotExist(linker) || LinkDoesNotExist(target))
				throw new Exception("Выполнить поиск связи можно только по существующим связям.");
			return SearchLink(source, linker, target);
		}

		static public bool Exists(Link source, Link linker, Link target)
		{
			return SearchLink(source, linker, target) != null;
		}

		#endregion

		#region Referers Walkers

		public bool WalkThroughReferersBySource(Func<Link, bool> walker)
		{
			if (LinkDoesNotExist(this))
				throw new Exception("C несуществующей связью нельзя производитить операции.");

			long referers = this.ReferersBySourceCount;
			if (referers == 1)
			{
				return walker(this.FirstRefererBySource);
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

			long referers = this.ReferersBySourceCount;
			if (referers == 1)
			{
				walker(this.FirstRefererBySource);
			}
			else if (referers > 1)
			{
				WalkThroughAllReferersBySource(this, new Visitor(x => walker(x)));
			}
		}

		public bool WalkThroughReferersByLinker(Func<Link, bool> walker)
		{
			if (LinkDoesNotExist(this))
				throw new Exception("C несуществующей связью нельзя производитить операции.");

			long referers = this.ReferersByLinkerCount;
			if (referers == 1)
			{
				return walker(this.FirstRefererByLinker);
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

			long referers = this.ReferersByLinkerCount;
			if (referers == 1)
			{
				walker(this.FirstRefererByLinker);
			}
			else if (referers > 1)
			{
				WalkThroughAllReferersByLinker(this, new Visitor(x => walker(x)));
			}
		}

		public bool WalkThroughReferersByTarget(Func<Link, bool> walker)
		{
			if (LinkDoesNotExist(this))
				throw new Exception("C несуществующей связью нельзя производитить операции.");

			long referers = this.ReferersByTargetCount;
			if (referers == 1)
			{
				return walker(this.FirstRefererByTarget);
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

			long referers = this.ReferersByTargetCount;
			if (referers == 1)
			{
				walker(this.FirstRefererByTarget);
			}
			else if (referers > 1)
			{
				WalkThroughAllReferersByTarget(this, new Visitor(x => walker(x)));
			}
		}

		public void WalkThroughReferers(Action<Link> walker)
		{
			if (LinkDoesNotExist(this))
				throw new Exception("C несуществующей связью нельзя производитить операции.");

			Visitor wrapper = new Visitor(x => walker(x));

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

		static public bool WalkThroughAllLinks(Func<Link, bool> walker)
		{
			return WalkThroughLinks(x => walker(x) ? 1 : 0) != 0;
		}

		static public void WalkThroughAllLinks(Action<Link> walker)
		{
			WalkThroughAllLinks(new Visitor(x => walker(x)));
		}

		#endregion
	}
}
