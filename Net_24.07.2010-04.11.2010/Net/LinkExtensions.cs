using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Net
{
	static public class LinkExtensions
	{
		static public void SetName(this Link link, string name)
		{
			if (link != null)
				Link.Create(link, Net.HasName, LinkConverter.FromString(name));
		}

		static public bool HasName(this Link link)
		{
			return false;
			if (link != null)
			{
				if (link.Linker == Net.And)
					return true;
				else
					foreach (var referer in link.ReferersBySource)
					{
						if (referer.IsAChar())
							return true;
						if (referer.Linker == Net.HasName)
							return true;
					}

				return HasName(link.Source)
					&& HasName(link.Linker)
					&& HasName(link.Target);
			}
			else
			{
				return false;
			}
		}

		static public bool TryGetName(this Link link, out string str)
		{
			str = null;
			return false;
			if (link != null)
			{
				if (link.Linker == Net.And)
				{
					List<Link> links = LinkConverter.ToList(link);
					str = LinkHelpers.FormatSequence(links);
					return true;
				}
				else
				{
					foreach (var referer in link.ReferersBySource)
					{
						if (referer.IsAChar())
						{
							char c = (char)LinkConverter.ToNumber(link);
							str = c.ToString();
							return true;
						}
						if (referer.Linker == Net.HasName)
						{
							str = LinkConverter.ToString(referer.Target);
							return true;
						}
					}
				}
			}
			str = null;
			return false;
		}

		static public bool IsAChar(this Link link)
		{
			return link != null
				&& link.Linker == Net.IsA
				&& link.Target == Net.Char;
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
				&& link.Linker == Net.ConsistsOf;
		}
	}
}
