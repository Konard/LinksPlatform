using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Net
{
	public partial class Link
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Link m_Source;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public Link Source 
		{
			get
			{
				return m_Source;
			}
			set
			{
				var previousValue = m_Source;
				var newValue = value;

				if (previousValue != newValue)
				{
					if (previousValue != null)
					{
						if (previousValue.m_FirstRefererBySource == this)
						{
							previousValue.m_FirstRefererBySource = m_NextSiblingRefererBySource;
						}
						else
						{
							Link previousSibling = previousValue.m_FirstRefererBySource;
							while (previousSibling.m_NextSiblingRefererBySource != this)
							{
								previousSibling = previousSibling.m_NextSiblingRefererBySource;
							}
							previousSibling.m_NextSiblingRefererBySource = m_NextSiblingRefererBySource;
						}
					}
					if (newValue != null)
					{
						m_NextSiblingRefererBySource = newValue.m_FirstRefererBySource;
						newValue.m_FirstRefererBySource = this;
					}
					else
					{
						m_NextSiblingRefererBySource = null;
					}
					m_Source = newValue;
				}
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Link m_Linker;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public Link Linker
		{
			get
			{
				return m_Linker;
			}
			set
			{
				var previousValue = m_Linker;
				var newValue = value;

				if (previousValue != newValue)
				{
					if (previousValue != null)
					{
						if (previousValue.m_FirstRefererByLinker == this)
						{
							previousValue.m_FirstRefererByLinker = m_NextSiblingRefererByLinker;
						}
						else
						{
							Link previousSibling = previousValue.m_FirstRefererByLinker;
							while (previousSibling.m_NextSiblingRefererByLinker != this)
							{
								previousSibling = previousSibling.m_NextSiblingRefererByLinker;
							}
							previousSibling.m_NextSiblingRefererByLinker = m_NextSiblingRefererByLinker;
						}
					}
					if (newValue != null)
					{
						m_NextSiblingRefererByLinker = newValue.m_FirstRefererByLinker;
						newValue.m_FirstRefererByLinker = this;
					}
					else
					{
						m_NextSiblingRefererByLinker = null;
					}
					m_Linker = newValue;
				}
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Link m_Target;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public Link Target
		{
			get
			{
				return m_Target;
			}
			set
			{
				var previousValue = m_Target;
				var newValue = value;

				if (previousValue != newValue)
				{
					if (previousValue != null)
					{
						if (previousValue.m_FirstRefererByTarget == this)
						{
							previousValue.m_FirstRefererByTarget = m_NextSiblingRefererByTarget;
						}
						else
						{
							Link previousSibling = previousValue.m_FirstRefererByTarget;
							while (previousSibling.m_NextSiblingRefererByTarget != this)
							{
								previousSibling = previousSibling.m_NextSiblingRefererByTarget;
							}
							previousSibling.m_NextSiblingRefererByTarget = m_NextSiblingRefererByTarget;
						}
					}
					if (newValue != null)
					{
						m_NextSiblingRefererByTarget = newValue.m_FirstRefererByTarget;
						newValue.m_FirstRefererByTarget = this;
					}
					else
					{
						m_NextSiblingRefererByTarget = null;
					}
					m_Target = newValue;
				}
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Link m_FirstRefererBySource;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Link m_FirstRefererByLinker;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Link m_FirstRefererByTarget;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Link m_NextSiblingRefererBySource;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Link m_NextSiblingRefererByLinker;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Link m_NextSiblingRefererByTarget;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public IEnumerable<Link> ReferersBySource
		{
			get
			{
				Link referer = m_FirstRefererBySource;
				while (referer != null)
				{
					yield return referer;
					referer = referer.m_NextSiblingRefererBySource;
				}
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public IEnumerable<Link> ReferersByLinker
		{
			get
			{
				Link referer = m_FirstRefererByLinker;
				while (referer != null)
				{
					yield return referer;
					referer = referer.m_NextSiblingRefererByLinker;
				}
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public IEnumerable<Link> ReferersByTarget
		{
			get
			{
				Link referer = m_FirstRefererByTarget;
				while (referer != null)
				{
					yield return referer;
					referer = referer.m_NextSiblingRefererByTarget;
				}
			}
		}

		static public Link Create(string name)
		{
			throw new InvalidOperationException("Текущая реализация функции недопустима.");

			Link link = new Link();
			link.SetName(name);
			return link;
		}

		// Можно было бы попробовать разрешать создавать линки без установки свойств,
		// но тогда есть возможность подмены линка, при установке свойств.
		static public Link Create(Link source, Link linker, Link target)
		{
			if (source == null)
			{
				throw new ArgumentNullException("Источник связи (начало) должен быть указан.");
			}
			if (linker == null)
			{
				throw new ArgumentNullException("Связчик должен быть указан.");
			}
			if (target == null)
			{
				throw new ArgumentNullException("Цель связи (конец) должна быть указана.");
			}

			Link link = TryFindExistingLink(source, linker, target);
			if (link == null)
			{
				link = new Link()
				{
					Source = source,
					Linker = linker,
					Target = target,
				};
			}
			return link;
		}

		static public Link CreateOutcomingSelflink(Link linker, Link target)
		{
			Link link = new Link();
			link.Source = link;
			link.Linker = linker;
			link.Target = target;
			return link;
		}

		static public Link CreateOutcomingSelflinker(Link target)
		{
			Link link = new Link();
			link.Source = link;
			link.Linker = link;
			link.Target = target;
			return link;
		}

		static public Link CreateIncomingSelflink(Link source, Link linker)
		{
			Link link = new Link();
			link.Source = source;
			link.Linker = linker;
			link.Target = link;
			return link;
		}

		static public Link CreateSelflinker(Link source, Link target)
		{
			Link link = new Link();
			link.Source = source;
			link.Linker = link;
			link.Target = target;
			return link;
		}

		static public Link CreateCycleSelflink(Link linker)
		{
			Link link = new Link();
			link.Source = link;
			link.Linker = linker;
			link.Target = link;
			return link;
		}

		static public Link CreateLinkLinkingItself()
		{
			Link link = new Link();
			link.Source = link;
			link.Linker = link;
			link.Target = link;
			return link;
		}

		static public void InitializeNet()
		{
			// Наивная инициализация (Не является корректным объяснением).
			Link isA = CreateOutcomingSelflinker(null);
			Link isNotA = CreateOutcomingSelflinker(isA); 
			Link link = CreateCycleSelflink(isA);
			Link thing = CreateOutcomingSelflink(isNotA, link);

			isA.Target = link; // Исключение, позволяющие завершить систему

			Link isNotAIsALink = Create(isNotA, isA, link);

			//isA.Delete();

			link.Delete();

			/*
			Link theLink = CreateLinkLinkingItself();
			Link theThing = CreateCycleSelflink(theLink);
			Link theLinkThing = CreateOutcomingSelflink(theLink, theThing);


			Link isA = new Link();
			Link isNotA = new Link();

			Link thing = new Link();	
			Link nothing = new Link();

			thing.SetSource(thing); thing.SetLinker(isA); thing.SetTarget(thing);
			nothing.SetLinker(isA);

			Link.Create(thing, isNotA, nothing);
			Link.Create(nothing, isNotA, thing);
			*/
		}

		// act of link on the link by the link
		// linker      target      source
		// как?		   что?	       чем?
		// что?        над чем?    чем?

		// Как избавиться от act? 

		// source  source  source
		// linker  reason  method
		// target  target  target

		// нечем нисвязная несвязь
		// not linked nolink
		// nothing not linking nothing

		// Может это ответ? Откуда мы? Из неоткуда.

		/// <remarks>
		/// Так как Null не является допустимым значением, всегда будет выполняться только первый поиск
		/// по Target`у.
		/// Есть возможность выполнять поиск в 3 потока, и тот, что выполнит быстрее завершает всю операцию.
		/// Также одновременно можно сохранять результаты производительность, т.е. кто за сколько закончил,
		/// чтобы решить имеет ли смысл действительно останавливаться на одном проходе, может быть действительно 
		/// проход по Target`у самый быстрый.
		/// </remarks>
		static private Link TryFindExistingLink(Link source, Link linker, Link target)
		{
			Func<Link, bool> isEqual = link =>
			{
				return link.Source == source
					&& link.Linker == linker
					&& link.Target == target;
			};

			return target.ReferersByTarget.FirstOrDefault(isEqual);
			//return source.ReferersBySource.FirstOrDefault(isEqual);
			//return linker.ReferersByLinker.FirstOrDefault(isEqual);
		}

		// Может значить и то, что Link ещё ни с чем не связан, но скоро будет.
		public bool IsDeleted()
		{
			return m_FirstRefererBySource == null
				&& m_FirstRefererByLinker == null
				&& m_FirstRefererByTarget == null;
		}

		public void Delete()
		{
			this.Source = null;
			this.Linker = null;
			this.Target = null;
			while (m_FirstRefererBySource != null) m_FirstRefererBySource.Delete();
			while (m_FirstRefererByLinker != null) m_FirstRefererByLinker.Delete();
			while (m_FirstRefererByTarget != null) m_FirstRefererByTarget.Delete();
		}

		public override string ToString()
		{
			string name;
			if (this.TryGetName(out name))
				return name;
			else if (this.Source.HasName() && this.Linker.HasName() && this.Target.HasName())
				return string.Format("{0} {1} {2}", this.Source, this.Linker, this.Target);
			else
				return base.GetHashCode().ToString();
		}
	}
}
