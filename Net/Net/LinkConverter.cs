using System;
using System.Collections.Generic;

namespace NetLibrary
{
    static public class LinkConverter
    {
        static public Link FromList(List<Link> links)
        {
            int i = links.Count - 1;
            Link element = links[i];
            while (--i >= 0) element = links[i] & element;
            return element;
        }

        static public Link FromList(Link[] links)
        {
            int i = links.Length - 1;
            Link element = links[i];
            while (--i >= 0) element = links[i] & element;
            return element;
        }

        static public List<Link> ToList(Link link)
        {
            List<Link> list = new List<Link>();
            SequenceWalker walker = new SequenceWalker(link, list.Add);
            walker.WalkFromLeftToRight();
            return list;
        }

        static public Link FromNumber(long number)
        {
            return NumberHelpers.FromNumber(number);
        }

        static public long ToNumber(Link number)
        {
            return NumberHelpers.ToNumber(number);
        }

        static public Link FromChar(char c)
        {
            return CharacterHelpers.FromChar(c);
        }

        static public char ToChar(Link charLink)
        {
            return CharacterHelpers.ToChar(charLink);
        }

        static public Link FromChars(char[] chars)
        {
            return FromObjectsToSequence(chars, FromChar);
        }

        static public Link FromChars(char[] chars, int takeFrom, int takeUntil)
        {
            return FromObjectsToSequence(chars, takeFrom, takeUntil, FromChar);
        }

        static public Link FromNumbers(long[] numbers)
        {
            return FromObjectsToSequence(numbers, FromNumber);
        }

        static public Link FromNumbers(long[] numbers, int takeFrom, int takeUntil)
        {
            return FromObjectsToSequence(numbers, takeFrom, takeUntil, FromNumber);
        }

        static public Link FromNumbers(ushort[] numbers)
        {
            return FromObjectsToSequence(numbers, x => FromNumber(x));
        }

        static public Link FromNumbers(ushort[] numbers, int takeFrom, int takeUntil)
        {
            return FromObjectsToSequence(numbers, takeFrom, takeUntil, x => FromNumber(x));
        }

        static public Link FromNumbers(uint[] numbers)
        {
            return FromObjectsToSequence(numbers, x => FromNumber(x));
        }

        static public Link FromNumbers(uint[] numbers, int takeFrom, int takeUntil)
        {
            return FromObjectsToSequence(numbers, takeFrom, takeUntil, x => FromNumber(x));
        }

        static public Link FromNumbers(byte[] numbers)
        {
            return FromObjectsToSequence(numbers, x => FromNumber(x));
        }

        static public Link FromNumbers(byte[] numbers, int takeFrom, int takeUntil)
        {
            return FromObjectsToSequence(numbers, takeFrom, takeUntil, x => FromNumber(x));
        }

        static public Link FromNumbers(bool[] numbers)
        {
            return FromObjectsToSequence(numbers, x => FromNumber(x ? 1 : 0));
        }

        static public Link FromNumbers(bool[] numbers, int takeFrom, int takeUntil)
        {
            return FromObjectsToSequence(numbers, takeFrom, takeUntil, x => FromNumber(x ? 1 : 0));
        }

        static public Link FromObjectsToSequence<T>(T[] objects, Func<T, Link> converter)
        {
            return FromObjectsToSequence(objects, 0, objects.Length, converter);
        }

        static public Link FromObjectsToSequence<T>(T[] objects, int takeFrom, int takeUntil, Func<T, Link> converter)
        {
            int length = takeUntil - takeFrom;
            if (length > 0)
            {
                Link[] copy = new Link[length];

                for (int i = takeFrom, j = 0; i < takeUntil; i++, j++)
                    copy[j] = converter(objects[i]);

                return FromList(copy);
            }
            else
            {
                throw new ArgumentOutOfRangeException("length", "Нельзя преобразовать пустой список к связям.");
            }
        }

        static public Link FromChars(string str)
        {
            Link[] copy = new Link[str.Length];

            for (int i = 0; i < copy.Length; i++)
                copy[i] = FromChar(str[i]);

            return FromList(copy);
        }

        static public Link FromString(string str)
        {
            Link[] copy = new Link[str.Length];

            for (int i = 0; i < copy.Length; i++)
                copy[i] = FromChar(str[i]);

            Link strLink = Link.Create(Net.String, Net.ThatConsistsOf, FromList(copy));
            return strLink;
        }

        static public string ToString(Link link)
        {
            if (link.IsString())
            {
                return ToString(ToList(link.Target));
            }
            else
                throw new ArgumentOutOfRangeException("link", "Specified link is not a string.");
        }

        static public string ToString(List<Link> charLinks)
        {
            char[] chars = new char[charLinks.Count];
            for (int i = 0; i < charLinks.Count; i++)
                chars[i] = ToChar(charLinks[i]);

            return new string(chars);
        }
    }
}
