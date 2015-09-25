using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Net
{
	static public class LinkConverter
	{
		static public Link FromList(IList<Link> links)
		{
			int i = links.Count - 1;
			Link element = links[i];
			while (--i >= 0) element = Link.Create(links[i], Net.And, element);
			return element;
		}

		static public List<Link> ToList(Link link)
		{
			List<Link> list = new List<Link>();

			Link element = link;
			while (element.Linker == Net.And)
			{
				list.Add(element.Source);
				element = element.Target;
			}
			list.Add(element);

			return list;
		}

		static public Link FromNumber(long number)
		{
			List<Link> sumSequenceList = new List<Link>();

			if (number > 0)
			{
				foreach (var key in Net.PowerOf2Links.Keys)
				{
					if ((number & key) == key)
					{
						sumSequenceList.Add(Net.PowerOf2Links[key]);
						number -= key;
						if (number == 0)
							break;
					}
				}
			}
			else
			{
				throw new NotSupportedException("Negative numbers are not supported yet.");
			}

			Link sum = Link.Create(Net.Sum, Net.Of, FromList(sumSequenceList));
			return sum;
		}

		static public long ToNumber(Link link)
		{
			if (link.IsSum())
			{
				long number = 0;
				List<Link> numberParts = ToList(link.Target);
				for (int i = 0; i < numberParts.Count; i++)
				{
					number += Net.PowerOf2Numbers[numberParts[i]];
				}
				return number;
			}
			else
			{
				throw new ArgumentOutOfRangeException("link", "Specified link is not a number.");
			}
		}

		static public Link FromString(string str)
		{
			Link[] charsSequenceList = new Link[str.Length];

			for (int i = 0; i < str.Length; i++)
			{
				Link number = FromNumber(str[i]);
				Link character = Link.Create(number, Net.IsA, Net.Char); // Только для отладки
                charsSequenceList[i] = character;
			}

			Link strLink = Link.Create(Net.String, Net.ConsistsOf, FromList(charsSequenceList));
			return strLink;
		}

		static public string ToString(Link link)
		{
			if (link.IsString())
			{
				List<Link> charLinks = ToList(link.Target);

				char[] chars = new char[charLinks.Count];
				for (int i = 0; i < charLinks.Count; i++)
				{
					chars[i] = (char)ToNumber(charLinks[i].Source);
				}

				return new string(chars);
			}
			else
			{
				throw new ArgumentOutOfRangeException("link", "Specified link is not a string.");
			}
		}
	}
}
