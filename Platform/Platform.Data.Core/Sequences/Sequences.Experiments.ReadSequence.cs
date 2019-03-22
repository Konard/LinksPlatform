using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.Data.Core.Collections;
using Platform.Data.Core.Exceptions;
using Platform.Data.Core.Doublets;
using Platform.Helpers;
using Platform.Helpers.Collections;
using LinkIndex = System.UInt64;

namespace Platform.Data.Core.Sequences
{
    partial class Sequences
    {
        public ulong[] ReadSequenceCore(ulong sequence, Func<ulong, bool> isElement)
        {
            var links = Links.Unsync;
            var length = 1;
            var hasElements = !isElement(sequence);
            var array = new ulong[length];
            array[0] = sequence;
            
            while (hasElements)
            {
                var nextArray = new ulong[length = length * 2];
                
                hasElements = false;

                for (var i = 0; i < array.Length; i++)
                {
                    if (array[i] == 0)
                        continue;
                        
                    if (isElement(array[i]))
                    {
                        nextArray[i*2] = array[i];
                    }
                    else
                    {
                        nextArray[i*2] = links.GetSource(array[i]);
                        nextArray[i*2 + 1] = links.GetTarget(array[i]);
                        if (!hasElements)
                            hasElements = !isElement(nextArray[i*2]) || !isElement(nextArray[i*2 + 1]);
                    }
                }
                
                array = nextArray;
            }
            
            var count = 0;
            for (var i = 0; i < array.Length; i++)
            {
                if (array[i] == 0)
                    continue;
                count++;
            }
            
            ulong[] finalArray = new ulong[count];
            for (int i = 0, j = 0; i < array.Length; i++)
            {
                if (array[i] == 0)
                    continue;
                finalArray[j++] = array[i];
            }
            
            return finalArray;
        }
    }
}
