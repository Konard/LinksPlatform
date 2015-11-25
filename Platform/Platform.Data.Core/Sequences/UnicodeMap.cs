using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Platform.Data.Core.Pairs;

namespace Platform.Data.Core.Sequences
{
    public class UnicodeMap
    {
        public const long MapSize = 65536;

        private readonly ILinks<ulong> _links;

        private bool _initialized;
        private ulong _firstCharLink;
        private ulong _lastCharLink;

        public ulong FirstCharLink { get { return _firstCharLink; } }
        public ulong LastCharLink { get { return _lastCharLink; } }

        public UnicodeMap(ILinks<ulong> links)
        {
            _links = links;
        }

        public void Init()
        {
            if (_initialized)
                return;

            _initialized = true;
            _firstCharLink = 1;
            _lastCharLink = _firstCharLink + char.MaxValue;

            var firstLink = _links.Create(0, 0);

            if (firstLink != _firstCharLink)
            {
                _links.Delete(firstLink);
#if DEBUG
                Console.WriteLine("Assume UTF16 table already created.");
#endif
            }
            else
            {
                for (var i = _firstCharLink + 1; i <= _lastCharLink; i++)
                {
                    // From NIL to It (NIL → Character) transformation meaning, (or infinite amount of NIL characters before actual Character)
                    var createdLink = _links.Create(firstLink, 0);
                    if (createdLink != i)
                        throw new Exception("Unable to initialize UTF 16 table.");
                }
#if DEBUG
                Console.WriteLine("UTF16 table created and initialized.");
#endif
            }

#if DEBUG
            Console.WriteLine("Total links count: {0}.", _links.Total);
#endif
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
    }
}
