using System;
using System.Collections.Generic;

namespace NetLibrary
{
    public static class LinkConverter
    {
        public static Link FromList(List<Link> links)
        {
            int i = links.Count - 1;
            Link element = links[i];
            while (--i >= 0) element = links[i] & element;
            return element;
        }

        public static Link FromList(Link[] links)
        {
            int i = links.Length - 1;
            Link element = links[i];
            while (--i >= 0) element = links[i] & element;
            return element;
        }

        public static List<Link> ToList(Link link)
        {
            var list = new List<Link>();
            var walker = new SequenceWalker(link, list.Add);
            walker.WalkFromLeftToRight();
            return list;
        }

        public static Link FromNumber(long number)
        {
            return NumberHelpers.FromNumber(number);
        }

        public static long ToNumber(Link number)
        {
            return NumberHelpers.ToNumber(number);
        }

        public static Link FromChar(char c)
        {
            return CharacterHelpers.FromChar(c);
        }

        public static char ToChar(Link charLink)
        {
            return CharacterHelpers.ToChar(charLink);
        }

        public static Link FromChars(char[] chars)
        {
            return FromObjectsToSequence(chars, FromChar);
        }

        public static Link FromChars(char[] chars, int takeFrom, int takeUntil)
        {
            return FromObjectsToSequence(chars, takeFrom, takeUntil, FromChar);
        }

        public static Link FromNumbers(long[] numbers)
        {
            return FromObjectsToSequence(numbers, FromNumber);
        }

        public static Link FromNumbers(long[] numbers, int takeFrom, int takeUntil)
        {
            return FromObjectsToSequence(numbers, takeFrom, takeUntil, FromNumber);
        }

        public static Link FromNumbers(ushort[] numbers)
        {
            return FromObjectsToSequence(numbers, x => FromNumber(x));
        }

        public static Link FromNumbers(ushort[] numbers, int takeFrom, int takeUntil)
        {
            return FromObjectsToSequence(numbers, takeFrom, takeUntil, x => FromNumber(x));
        }

        public static Link FromNumbers(uint[] numbers)
        {
            return FromObjectsToSequence(numbers, x => FromNumber(x));
        }

        public static Link FromNumbers(uint[] numbers, int takeFrom, int takeUntil)
        {
            return FromObjectsToSequence(numbers, takeFrom, takeUntil, x => FromNumber(x));
        }

        public static Link FromNumbers(byte[] numbers)
        {
            return FromObjectsToSequence(numbers, x => FromNumber(x));
        }

        public static Link FromNumbers(byte[] numbers, int takeFrom, int takeUntil)
        {
            return FromObjectsToSequence(numbers, takeFrom, takeUntil, x => FromNumber(x));
        }

        public static Link FromNumbers(bool[] numbers)
        {
            return FromObjectsToSequence(numbers, x => FromNumber(x ? 1 : 0));
        }

        public static Link FromNumbers(bool[] numbers, int takeFrom, int takeUntil)
        {
            return FromObjectsToSequence(numbers, takeFrom, takeUntil, x => FromNumber(x ? 1 : 0));
        }

        public static Link FromObjectsToSequence<T>(T[] objects, Func<T, Link> converter)
        {
            return FromObjectsToSequence(objects, 0, objects.Length, converter);
        }

        public static Link FromObjectsToSequence<T>(T[] objects, int takeFrom, int takeUntil, Func<T, Link> converter)
        {
            int length = takeUntil - takeFrom;

            if (length <= 0)
                throw new ArgumentOutOfRangeException("length", "Нельзя преобразовать пустой список к связям.");

            var copy = new Link[length];

            for (int i = takeFrom, j = 0; i < takeUntil; i++, j++)
                copy[j] = converter(objects[i]);

            return FromList(copy);
        }

        public static Link FromChars(string str)
        {
            var copy = new Link[str.Length];

            for (int i = 0; i < copy.Length; i++)
                copy[i] = FromChar(str[i]);

            return FromList(copy);
        }

        public static Link FromString(string str)
        {
            var copy = new Link[str.Length];

            for (int i = 0; i < copy.Length; i++)
                copy[i] = FromChar(str[i]);

            Link strLink = Link.Create(Net.String, Net.ThatConsistsOf, FromList(copy));
            return strLink;
        }

        public static string ToString(Link link)
        {
            if (link.IsString())
                return ToString(ToList(link.Target));

            throw new ArgumentOutOfRangeException("link", "Specified link is not a string.");
        }

        public static string ToString(List<Link> charLinks)
        {
            var chars = new char[charLinks.Count];
            for (int i = 0; i < charLinks.Count; i++)
                chars[i] = ToChar(charLinks[i]);

            return new string(chars);
        }
    }
}
