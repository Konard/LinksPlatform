using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetLibrary
{
	static public class LinkExtensions
	{
		static public Link SetName(this Link link, string name)
		{
			Link.Create(link, Net.Has, Link.Create(Net.Name, Net.ThatIsRepresentedBy, LinkConverter.FromString(name)));
			return link; // Chaining
		}

		static private readonly HashSet<Link> LinksWithNamesGatheringProcess = new HashSet<Link>();

		static public bool TryGetName(this Link link, out string str)
		{
			if (!LinksWithNamesGatheringProcess.Add(link))
			{
				str = "...";
				return true;
			}
			else
			{
				try
				{
					if (link != null)
					{
						if (link.Linker == Net.And)
						{
							str = SequenceHelpers.FormatSequence(link);
							return true;
						}
						else if (link.IsGroup())
						{
							str = LinkConverter.ToString(LinkConverter.ToList(link.Target));
							return true;
						}
						else if (link.IsChar())
						{
							str = LinkConverter.ToChar(link).ToString();
							return true;
						}
						else if (TryGetSpecificName(link, out str))
							return true;

						if (link.Source == link || link.Linker == link || link.Target == link)
							return false;

						string sourceName, linkerName, targetName;
						if (TryGetName(link.Source, out sourceName) && TryGetName(link.Linker, out linkerName) && TryGetName(link.Target, out targetName))
						{
							var sb = new StringBuilder();
							sb.Append(sourceName).Append(' ').Append(linkerName).Append(' ').Append(targetName);
							str = sb.ToString();
							return true;
						}
					}
					str = null;
					return false;
				}
				finally
				{
					LinksWithNamesGatheringProcess.Remove(link);
				}
			}
		}

		static public bool TryGetSpecificName(this Link link, out string name)
		{
			string nameLocal = null;
			if (Net.Name.ReferersBySourceCount < link.ReferersBySourceCount)
			{
				Net.Name.WalkThroughReferersBySource(referer =>
				{
					if (referer.Linker == Net.ThatIsRepresentedBy)
					{
						if (Link.Exists(link, Net.Has, referer))
						{
							nameLocal = LinkConverter.ToString(referer.Target);
							return false; // Останавливаем проход
						}
					}
					return true;
				});
			}
			else
			{
				link.WalkThroughReferersBySource(referer =>
				{
					if (referer.Linker == Net.Has)
					{
						Link nameLink = referer.Target;
						if (nameLink.Source == Net.Name && nameLink.Linker == Net.ThatIsRepresentedBy)
						{
							nameLocal = LinkConverter.ToString(nameLink.Target);
							return false; // Останавливаем проход
						}
					}
					return true;
				});
			}

			name = nameLocal;
			return nameLocal != null;
		}

		// Проверка на пренадлежность классу
		static public bool Is(this Link link, Link @class)
		{
			if (link.Linker == Net.IsA)
			{
				if (link.Target == @class)
					return true;
				else
					return link.Target.Is(@class);
			}
			return false;
		}

		// Несколько не правильное определение, так выйдет, что любая сумма входящая в диапазон значений char будет символом.
		// Нужно изменить определение чара, идеально: char consists of sum of [8, 64].
		static public bool IsChar(this Link link)
		{
			return CharacterHelpers.IsChar(link);
		}

		static public bool IsGroup(this Link link)
		{
			return link != null
				&& link.Source == Net.Group
				&& link.Linker == Net.ThatConsistsOf;
		}

		static public bool IsSum(this Link link)
		{
			return link != null
				&& link.Source == Net.Sum
				&& link.Linker == Net.Of;
		}

		static public bool IsString(this Link link)
		{
			return link != null
				&& link.Source == Net.String
				&& link.Linker == Net.ThatConsistsOf;
		}

		static public bool IsName(this Link link)
		{
			return link != null
				&& link.Source == Net.Name
				&& link.Linker == Net.Of;
		}

		static public Link[] GetArrayOfRererersBySource(this Link link)
		{
			if (link == null)
				return new Link[0];
			else
			{
				Link[] array = new Link[link.ReferersBySourceCount];
				int index = 0;
				link.WalkThroughReferersBySource(referer => array[index++] = referer);
				return array;
			}
		}

		static public Link[] GetArrayOfRererersByLinker(this Link link)
		{
			if (link == null)
				return new Link[0];
			else
			{
				Link[] array = new Link[link.ReferersByLinkerCount];
				int index = 0;
				link.WalkThroughReferersByLinker(referer => array[index++] = referer);
				return array;
			}
		}

		static public Link[] GetArrayOfRererersByTarget(this Link link)
		{
			if (link == null)
				return new Link[0];
			else
			{
				Link[] array = new Link[link.ReferersByTargetCount];
				int index = 0;
				link.WalkThroughReferersByTarget(referer => array[index++] = referer);
				return array;
			}
		}

		static public void WalkThroughSequence(this Link link, Action<Link> action)
		{
			var walker = new SequenceWalker(link, action);
			walker.WalkFromLeftToRight();
		}
	}
}
