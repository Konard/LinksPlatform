using System;
using Platform.Data.Core.Exceptions;
using Platform.Helpers;

namespace Platform.Data.Core.Pairs
{
    public static class LinksExtensions
    {
        public static ulong RunRandomCreations(this Links links, long amountOfCreations)
        {
            ulong result = 0;
            for (long i = 0; i < amountOfCreations; i++)
            {
                var source = RandomHelpers.DefaultFactory.NextUInt64(links.Count());
                var target = RandomHelpers.DefaultFactory.NextUInt64(links.Count());

                result += links.Create(source, target);
            }
            return result;
        }

        public static ulong RunRandomSearches(this Links links, long amountOfSearches)
        {
            ulong result = 0;
            for (long i = 0; i < amountOfSearches; i++)
            {
                var source = RandomHelpers.DefaultFactory.NextUInt64(1, links.Count());
                var target = RandomHelpers.DefaultFactory.NextUInt64(1, links.Count());

                result += links.Search(source, target);
            }
            return result;
        }

        public static ulong RunRandomDeletions(this Links links, long amountOfDeletions)
        {
            ulong result = 0;

            ulong min = (ulong)amountOfDeletions > links.Count() ? 1 : links.Count() - (ulong)amountOfDeletions;

            for (long i = 0; i < amountOfDeletions; i++)
            {
                var link = RandomHelpers.DefaultFactory.NextUInt64(min, links.Count());
                result += link;
                links.Delete(link);
                if (links.Count() < min)
                    break;
            }
            return result;
        }

        /// <remarks>
        /// TODO: Возможно есть очень простой способ это сделать.
        /// (Например просто удалить файл, или изменить его размер таким образом,
        /// чтобы удалился весь контент)
        /// Например через _header->AllocatedLinks в LinksMemoryManager
        /// </remarks>
        public static void DeleteAll(this Links links)
        {
            for (ulong i = links.Count(); i > 0; i--)
            {
                links.Delete(i);
                if (links.Count() != i - 1)
                    i = links.Count();
            }
        }

        public static ulong First(this Links links)
        {
            ulong resultLink = 0;

            if (links.Count() == 0)
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

        public static void EnsureEachLinkExists(this Links links, params ulong[] sequence)
        {
            if (sequence == null)
                return;

            for (var i = 0; i < sequence.Length; i++)
                if (!links.Exists(sequence[i]))
                    throw new ArgumentLinkDoesNotExistsException<ulong>(sequence[i],
                        string.Format("patternSequence[{0}]", i));
        }

        public static void EnsureEachLinkIsAnyOrExists(this Links links, params ulong[] sequence)
        {
            if (sequence == null)
                return;

            for (var i = 0; i < sequence.Length; i++)
                if (sequence[i] != LinksConstants.Any && !links.Exists(sequence[i]))
                    throw new ArgumentLinkDoesNotExistsException<ulong>(sequence[i],
                        string.Format("patternSequence[{0}]", i));
        }

        public static bool AnyLinkIsAny(this Links links, params ulong[] sequence)
        {
            if (sequence == null)
                return false;

            for (var i = 0; i < sequence.Length; i++)
                if (sequence[i] == LinksConstants.Any) return true;

            return false;
        }
    }
}