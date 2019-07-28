using System;
using System.Collections.Generic;
using System.Globalization;
using Platform.Numbers;

namespace Platform.Data.Triplets
{
    public static class NumberHelpers
    {
        public static Link[] NumbersToLinks;
        public static Dictionary<Link, long> LinksToNumbers;

        static NumberHelpers() => Create();

        private static void Create()
        {
            NumbersToLinks = new Link[64];
            LinksToNumbers = new Dictionary<Link, long>();
            NumbersToLinks[0] = Net.One;
            LinksToNumbers[Net.One] = 1;
        }

        public static void Recreate() => Create();

        private static Link FromPowerOf2(long powerOf2)
        {
            var result = NumbersToLinks[powerOf2];
            if (result == null)
            {
                var previousPowerOf2Link = NumbersToLinks[powerOf2 - 1];
                if (previousPowerOf2Link == null)
                {
                    previousPowerOf2Link = NumbersToLinks[0];
                    for (var i = 1; i < powerOf2; i++)
                    {
                        if (NumbersToLinks[i] == null)
                        {
                            var numberLink = Link.Create(Net.Sum, Net.Of, previousPowerOf2Link & previousPowerOf2Link);
                            var num = (long)Math.Pow(2, i);
                            NumbersToLinks[i] = numberLink;
                            LinksToNumbers[numberLink] = num;
                            numberLink.SetName(num.ToString(CultureInfo.InvariantCulture));
                        }
                        previousPowerOf2Link = NumbersToLinks[i];
                    }
                }
                result = Link.Create(Net.Sum, Net.Of, previousPowerOf2Link & previousPowerOf2Link);
                var number = (long)Math.Pow(2, powerOf2);
                NumbersToLinks[powerOf2] = result;
                LinksToNumbers[result] = number;
                result.SetName(number.ToString(CultureInfo.InvariantCulture));
            }
            return result;
        }

        public static Link FromNumber(long number)
        {
            if (number == 0)
            {
                return Net.Zero;
            }
            if (number == 1)
            {
                return Net.One;
            }
            var links = new Link[BitwiseHelpers.CountBits(number)];
            if (number >= 0)
            {
                for (long key = 1, powerOf2 = 0, i = 0; key <= number; key *= 2, powerOf2++)
                {
                    if ((number & key) == key)
                    {
                        links[i++] = FromPowerOf2(powerOf2);
                    }
                }
            }
            else
            {
                throw new NotSupportedException("Negative numbers are not supported yet.");
            }
            var sum = Link.Create(Net.Sum, Net.Of, LinkConverter.FromList(links));
            return sum;
        }

        public static long ToNumber(Link link)
        {
            if (link == Net.Zero)
            {
                return 0;
            }
            if (link == Net.One)
            {
                return 1;
            }
            if (link.IsSum())
            {
                var numberParts = LinkConverter.ToList(link.Target);
                long number = 0;
                for (var i = 0; i < numberParts.Count; i++)
                {
                    GoDownAndTakeIt(numberParts[i], out long numberPart);
                    number += numberPart;
                }
                return number;
            }
            throw new ArgumentOutOfRangeException(nameof(link), "Specified link is not a number.");
        }

        private static void GoDownAndTakeIt(Link link, out long number)
        {
            if (!LinksToNumbers.TryGetValue(link, out number))
            {
                var previousNumberLink = link.Target.Source;
                GoDownAndTakeIt(previousNumberLink, out number);
                var previousNumberIndex = (int)Math.Log(number, 2);
                var newNumberIndex = previousNumberIndex + 1;
                var newNumberLink = Link.Create(Net.Sum, Net.Of, previousNumberLink & previousNumberLink);
                number += number;
                NumbersToLinks[newNumberIndex] = newNumberLink;
                LinksToNumbers[newNumberLink] = number;
            }
        }
    }
}
