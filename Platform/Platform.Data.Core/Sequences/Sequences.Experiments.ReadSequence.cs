using System;
using Platform.Data.Core.Doublets;

namespace Platform.Data.Core.Sequences
{
    partial class Sequences
    {
        public ulong[] ReadSequenceCore(ulong sequence, Func<ulong, bool> isElement)
        {
            var links = Links.Unsync;
            var length = 1;
            var array = new ulong[length];
            array[0] = sequence;

            if (isElement(sequence))
                return array;

            bool hasElements;
            do
            {
                var nextArray = new ulong[length *= 2];

                hasElements = false;

                for (var i = 0; i < array.Length; i++)
                {
                    var candidate = array[i];
                    if (candidate == 0)
                        continue;

                    if (isElement(candidate))
                        nextArray[i * 2] = candidate;
                    else
                    {
                        var link = links.GetLink(candidate);
                        var linkSource = links.GetSource(link);
                        var linkTarget = links.GetTarget(link);
                        nextArray[i * 2] = linkSource;
                        nextArray[i * 2 + 1] = linkTarget;
                        if (!hasElements)
                            hasElements = !(isElement(linkSource) && isElement(linkTarget));
                    }
                }

                array = nextArray;
            }
            while (hasElements);

            var count = 0;
            for (var i = 0; i < array.Length; i++)
                if (array[i] != 0)
                    count++;

            if (count == array.Length)
                return array;
            else
            {
                var finalArray = new ulong[count];
                for (int i = 0, j = 0; i < array.Length; i++)
                    if (array[i] != 0)
                        finalArray[j++] = array[i];
                return finalArray;
            }
        }
    }
}
