using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Platform.Data.Core.Pairs;
using Platform.Helpers;

namespace Platform.Data.Core.Sequences
{
    public class UnicodeMap
    {
        public const ulong FirstCharLink = 1;
        public const ulong LastCharLink = FirstCharLink + char.MaxValue;
        public const long MapSize = 1 + char.MaxValue;

        private readonly ILinks<ulong> _links;

        private bool _initialized;

        public UnicodeMap(ILinks<ulong> links)
        {
            _links = links;
        }

        public static UnicodeMap InitNew(ILinks<ulong> links)
        {
            var map = new UnicodeMap(links);
            map.Init();
            return map;
        }

        public void Init()
        {
            if (_initialized)
                return;

            _initialized = true;

            var firstLink = _links.Create(0, 0);

            if (firstLink != FirstCharLink)
            {
                _links.Delete(firstLink);

                ConsoleHelpers.Debug("Assume UTF16 table already created.");
            }
            else
            {
                for (var i = FirstCharLink + 1; i <= LastCharLink; i++)
                {
                    // From NIL to It (NIL -> Character) transformation meaning, (or infinite amount of NIL characters before actual Character)
                    var createdLink = _links.Create(firstLink, 0);
                    if (createdLink != i)
                        throw new Exception("Unable to initialize UTF 16 table.");
                }

                ConsoleHelpers.Debug("UTF16 table created and initialized.");
            }

            ConsoleHelpers.Debug("Total links count: {0}.", _links.Count());
        }

        // 0 - null link
        // 1 - nil character (0 character)
        // ...
        // 65536 (0(1) + 65535 = 65536 possible values)

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong FromCharToLink(char character)
        {
            return ((ulong)character + 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char FromLinkToChar(ulong link)
        {
            return (char)(link - 1);
        }

        public static string FromSequenceLinkToString(ulong link, Links links)
        {
            var sb = new StringBuilder();

            if (links.Exists(link))
            {
                StopableSequenceWalker.WalkRight(link, links.GetSourceCore, links.GetTargetCore,
                    x => x <= MapSize || links.GetSourceCore(x) == x || links.GetTargetCore(x) == x, element =>
                    {
                        sb.Append(FromLinkToChar(element));
                        return true;
                    });
            }

            return sb.ToString();
        }

        public static ulong[] FromStringToLinkArray(string sequence)
        {
            // char array to ulong array
            var linksSequence = new ulong[sequence.Length];
            for (var i = 0; i < sequence.Length; i++)
                linksSequence[i] = FromCharToLink(sequence[i]);
            return linksSequence;
        }

        public static List<ulong[]> FromStringToLinkArrayGroups(string sequence)
        {
            var result = new List<ulong[]>();

            var offset = 0;

            while (offset < sequence.Length)
            {
                var currentCategory = char.GetUnicodeCategory(sequence[offset]);

                var relativeLength = 1;
                var absoluteLength = offset + relativeLength;
                while (absoluteLength < sequence.Length &&
                       currentCategory == char.GetUnicodeCategory(sequence[absoluteLength]))
                {
                    relativeLength++;
                    absoluteLength++;
                }

                // char array to ulong array
                var innerSequence = new ulong[relativeLength];
                var maxLength = offset + relativeLength;
                for (var i = offset; i < maxLength; i++)
                    innerSequence[i - offset] = FromCharToLink(sequence[i]);
                result.Add(innerSequence);

                offset += relativeLength;
            }

            return result;
        }

        public static List<ulong[]> FromLinkArrayToLinkArrayGroups(ulong[] array)
        {
            var result = new List<ulong[]>();

            var offset = 0;

            while (offset < array.Length)
            {
                var relativeLength = 1;

                if (array[offset] <= LastCharLink)
                {
                    var currentCategory = char.GetUnicodeCategory(FromLinkToChar(array[offset]));

                    var absoluteLength = offset + relativeLength;
                    while (absoluteLength < array.Length &&
                           array[absoluteLength] <= LastCharLink &&
                           currentCategory == char.GetUnicodeCategory(FromLinkToChar(array[absoluteLength])))
                    {
                        relativeLength++;
                        absoluteLength++;
                    }
                }
                else
                {
                    var absoluteLength = offset + relativeLength;
                    while (absoluteLength < array.Length && array[absoluteLength] > LastCharLink)
                    {
                        relativeLength++;
                        absoluteLength++;
                    }
                }

                // copy array
                var innerSequence = new ulong[relativeLength];
                var maxLength = offset + relativeLength;
                for (var i = offset; i < maxLength; i++)
                    innerSequence[i - offset] = array[i];
                result.Add(innerSequence);

                offset += relativeLength;
            }

            return result;
        }
    }
}
