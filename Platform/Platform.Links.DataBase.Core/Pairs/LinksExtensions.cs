using System;
using Platform.Links.System.Helpers;

namespace Platform.Links.DataBase.Core.Pairs
{
    public static class LinksExtensions
    {
        private static readonly Random Rnd = new Random();

        public static ulong RunRandomCreations(this Links links, long amountOfCreations)
        {
            ulong result = 0;
            for (long i = 0; i < amountOfCreations; i++)
            {
                var source = Rnd.NextUInt64(links.Total);
                var target = Rnd.NextUInt64(links.Total);

                result += links.Create(source, target);
            }
            return result;
        }

        public static ulong RunRandomSearches(this Links links, long amountOfSearches)
        {
            ulong result = 0;
            for (long i = 0; i < amountOfSearches; i++)
            {
                var source = Rnd.NextUInt64(1, links.Total);
                var target = Rnd.NextUInt64(1, links.Total);

                result += links.Search(source, target);
            }
            return result;
        }

        public static ulong RunRandomDeletions(this Links links, long amountOfDeletions)
        {
            ulong result = 0;

            ulong min = (ulong) amountOfDeletions > links.Total ? 1 : links.Total - (ulong) amountOfDeletions;

            for (long i = 0; i < amountOfDeletions; i++)
            {
                var link = Rnd.NextUInt64(min, links.Total);
                result += link;
                links.Delete(ref link);
                if (links.Total < min)
                    break;
            }
            return result;
        }

        public static void DeleteAllLinks(this Links links)
        {
            for (ulong i = links.Total; i > 0; i--)
            {
                ulong link = i;
                links.Delete(ref link);
                if (links.Total != i - 1)
                    i = links.Total;
            }
        }

        public static ulong First(this Links links)
        {
            ulong resultLink = 0;

            if (links.Total == 0)
                throw new Exception("В базе данных нет связей.");

            links.Each(0, 0, x =>
            {
                resultLink = x;
                return false;
            });

            if (resultLink == 0)
                throw new Exception("В процессе поиска по базе данных не было найдено связей.");

            return resultLink;
        }
    }
}