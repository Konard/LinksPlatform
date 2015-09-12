using System;
using System.Collections.Generic;

namespace NetLibrary
{
	static public class NumberHelpers
	{
		static public Link[] NumbersToLinks;
		static public Dictionary<Link, long> LinksToNumbers;

		static NumberHelpers()
		{
			Create();
		}

		static private void Create()
		{
			NumbersToLinks = new Link[64];
			LinksToNumbers = new Dictionary<Link, long>();

			NumbersToLinks[0] = Net.One;
			LinksToNumbers[Net.One] = 1;
		}

		static public void Recreate()
		{
			Create();
		}

		static private long CountBits(long x)
		{
			long n = 0;
			while (x != 0)
			{
				n++;
				x = x & (x - 1);
			}
			return n;
		}

		static private Link FromPowerOf2(long powerOf2)
		{
			Link result = NumbersToLinks[powerOf2];
			if (result == null)
			{
				Link previousPowerOf2Link = NumbersToLinks[powerOf2 - 1];

				if (previousPowerOf2Link == null)
				{
					previousPowerOf2Link = NumbersToLinks[0];
					for (int i = 1; i < powerOf2; i++)
					{
						if (NumbersToLinks[i] == null)
						{
							Link numberLink = Link.Create(Net.Sum, Net.Of, previousPowerOf2Link & previousPowerOf2Link);

							var num = (long)Math.Pow(2, i);

							NumbersToLinks[i] = numberLink;
							LinksToNumbers[numberLink] = num;

							numberLink.SetName(num.ToString());
						}
						previousPowerOf2Link = NumbersToLinks[i];
					}
				}

				result = Link.Create(Net.Sum, Net.Of, previousPowerOf2Link & previousPowerOf2Link);

				var number = (long)Math.Pow(2, powerOf2);

				NumbersToLinks[powerOf2] = result;
				LinksToNumbers[result] = number;

				result.SetName(number.ToString());
			}
			return result;
		}

		static public Link FromNumber(long number)
		{
		    if (number == 0)
				return Net.Zero;
		    if (number == 1)
		        return Net.One;
		    
            var links = new Link[CountBits(number)];

		    if (number >= 0)
		    {
		        for (long key = 1, powerOf2 = 0, i = 0; key <= number; key += key, powerOf2++)
		            if ((number & key) == key)
		                links[i++] = FromPowerOf2(powerOf2);
		    }
		    else
		        throw new NotSupportedException("Negative numbers are not supported yet.");

		    Link sum = Link.Create(Net.Sum, Net.Of, LinkConverter.FromList(links));
		    return sum;
		}

	    static public long ToNumber(Link link)
	    {
	        if (link == Net.Zero)
				return 0;
	        if (link == Net.One)
	            return 1;

	        if (link.IsSum())
	        {
	            List<Link> numberParts = LinkConverter.ToList(link.Target);

	            long number = 0;
	            for (var i = 0; i < numberParts.Count; i++)
	            {
	                long numberPart;
	                GoDownAndTakeIt(numberParts[i], out numberPart);
	                number += numberPart;
	            }

	            return number;
	        }
	        throw new ArgumentOutOfRangeException("link", "Specified link is not a number.");
	    }

	    static private void GoDownAndTakeIt(Link link, out long number)
		{
			if (!LinksToNumbers.TryGetValue(link, out number))
			{
				Link previousNumberLink = link.Target.Source;

				GoDownAndTakeIt(previousNumberLink, out number);

				var previousNumberIndex = (int)Math.Log(number, 2);

				int newNumberIndex = previousNumberIndex + 1;
				Link newNumberLink = Link.Create(Net.Sum, Net.Of, previousNumberLink & previousNumberLink);

				number = number + number;

				NumbersToLinks[newNumberIndex] = newNumberLink;
				LinksToNumbers[newNumberLink] = number;
			}
		}
	}
}
