
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Net
{
	static public class LinkHelpers
	{
		static public string FormatSequence(List<Link> links)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append('[');
			for (int i = 0; i < links.Count; i++)
			{
				if (i > 0)
				{
					sb.Append(',');
				}
				sb.Append(links[i].ToString());
			}
			sb.Append(']');

			return sb.ToString();
		}
	}
}
