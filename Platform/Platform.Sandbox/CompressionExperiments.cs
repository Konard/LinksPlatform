using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using Platform.Collections;
using Platform.Threading;
using Platform.Singletons;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.ResizableDirectMemory.Specific;
using Platform.Data.Doublets.Sequences;
using Platform.Data.Doublets.Sequences.Frequencies.Cache;
using Platform.Data.Doublets.Sequences.Frequencies.Counters;
using Platform.Data.Doublets.Sequences.Converters;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Unicode;

namespace Platform.Sandbox
{
    public static class CompressionExperiments
    {
        public static void Test()
        {
            File.Delete("web.links");

            using (var memoryManager = new UInt64ResizableDirectMemoryLinks("web.links", 8 * 1024 * 1024))
            using (var links = new UInt64Links(memoryManager))
            {
                var syncLinks = new SynchronizedLinks<ulong>(links);
                links.UseUnicode();

                var sequences = new Sequences(syncLinks);

                // Get content
                const string url = "https://en.wikipedia.org/wiki/Main_Page";
                var pageContents = GetPageContents(url);

                var totalChars = url.Length + pageContents.Length;

                Global.Trash = totalChars;

                var balancedVariantConverter = new BalancedVariantConverter<ulong>(syncLinks);

                var urlLink = balancedVariantConverter.Convert(UnicodeMap.FromStringToLinkArray(url));

                var responseSourceArray = UnicodeMap.FromStringToLinkArray(pageContents);

                //for (var i = 0; i < 1; i++)
                //{
                //    var sw01 = Stopwatch.StartNew();
                //    var responseLink = sequences.CreateBalancedVariant(responseSourceArray);
                //    sw01.Stop();
                //    Console.WriteLine(sw01.Elapsed);
                //}

                //var sw0 = Stopwatch.StartNew();
                //var groups = UnicodeMap.FromStringToLinkArrayGroups(response);
                //var responseLink = sequences.CreateBalancedVariant(groups); sw0.Stop();

                //var sw1 = Stopwatch.StartNew();
                //var responseCompressedArray1 = links.PrecompressSequence1(responseSourceArray); sw1.Stop();

                //var sw2 = Stopwatch.StartNew();
                //var responseCompressedArray2 = links.PrecompressSequence2(responseSourceArray); sw2.Stop();

                // [+] Можно попробовать искать не максимальный, а первый, который встречается как минимум дважды - медленно, высокое качество, не наивысшее
                // [+] Или использовать не локальный словарь, а глобальный (т.е. считать один раз, потом только делать замены) - быстро, но качество низкое
                // Precompress0 - лучшее соотношение скорость / качество. (тоже что и Data.Core.Sequences.Compressor.Precompress)

                ulong responseLink2 = syncLinks.Constants.Null;

                for (var i = 0; i < 1; i++)
                {
                    var sw3 = Stopwatch.StartNew();
                    var frequencyCounter = new TotalSequenceSymbolFrequencyCounter<ulong>(syncLinks);
                    var doubletFrequenciesCache = new LinkFrequenciesCache<ulong>(syncLinks, frequencyCounter);
                    var compressingConvertor = new CompressingConverter<ulong>(syncLinks, balancedVariantConverter, doubletFrequenciesCache);
                    responseLink2 = compressingConvertor.Convert(responseSourceArray); sw3.Stop();
                    Console.WriteLine(sw3.Elapsed);
                }

                // Combine Groups and Compression (first Compression, then Groups) (DONE)
                // Как после сжатия не группируй, больше сжатия не получить (странно, но это факт)
                //var groups = UnicodeMap.FromLinkArrayToLinkArrayGroups(responseCompressedArray3);
                //var responseLink2 = sequences.CreateBalancedVariant(groups);
                // Equal to `var responseLink2 = sequences.CreateBalancedVariant(responseCompressedArray3);`


                //for (int i = 0; i < responseCompressedArray1.Length; i++)
                //{
                //    if (responseCompressedArray1[i] != responseCompressedArray2[i])
                //    {

                //    }
                //}

                //var responseLink1 = sequences.CreateBalancedVariant(responseCompressedArray1);
                //var responseLink2 = sequences.CreateBalancedVariant(responseCompressedArray3);

                //var decompress1 = sequences.FormatSequence(responseLink1);
                var decompress2 = sequences.FormatSequence(responseLink2);

                Global.Trash = decompress2;

                //for (int i = 0; i < decompress1.Length; i++)
                //{
                //    if (decompress1[i] != decompress2[i])
                //    {

                //    }
                //}

                var unpack = UnicodeMap.FromSequenceLinkToString(responseLink2, syncLinks);

                Global.Trash = unpack == pageContents;

                var totalLinks = syncLinks.Count() - UnicodeMap.MapSize;

                Console.WriteLine(totalLinks);

                Global.Trash = totalLinks;

                syncLinks.CreateAndUpdate(urlLink, responseLink2);

                var divLinksArray = UnicodeMap.FromStringToLinkArray("div");

                var fullyMatched = sequences.GetAllMatchingSequences1(divLinksArray);
                var partiallyMatched = sequences.GetAllPartiallyMatchingSequences1(divLinksArray);

                var intersection = fullyMatched.Intersect(partiallyMatched);

            }

            Console.ReadKey();
        }

        public static void Stats()
        {
            // Get content
            const string url = "https://en.wikipedia.org/wiki/Main_Page";
            var pageContents = GetPageContents(url);

            var responseSourceArray = UnicodeMap.FromStringToLinkArray(pageContents);

            for (var i = 0; i < 3; i++)
            {
                File.Delete("stats.links");

                using (var memoryManager = new UInt64ResizableDirectMemoryLinks("stats.links", 8 * 1024 * 1024))
                using (var links = new UInt64Links(memoryManager))
                {
                    var syncLinks = new SynchronizedLinks<ulong>(links);
                    links.UseUnicode();

                    var sequences = new Sequences(syncLinks);

                    var balancedVariantConverter = new BalancedVariantConverter<ulong>(syncLinks);

                    var sw3 = Stopwatch.StartNew(); balancedVariantConverter.Convert(responseSourceArray); sw3.Stop();

                    var totalLinks = syncLinks.Count() - UnicodeMap.MapSize;

                    Console.WriteLine($"Balanced Variant: {sw3.Elapsed}, {responseSourceArray.Length}, {totalLinks}");
                }
            }

            var minFrequency = 0UL;

            for (var i = 1; i < 200; i++)
            {
                minFrequency += (ulong)(1 + Math.Log(i));

                File.Delete("stats.links");

                using (var memoryManager = new UInt64ResizableDirectMemoryLinks("stats.links", 8 * 1024 * 1024))
                using (var links = new UInt64Links(memoryManager))
                {
                    var syncLinks = new SynchronizedLinks<ulong>(links);
                    links.UseUnicode();

                    var sequences = new Sequences(syncLinks);

                    var sw3 = Stopwatch.StartNew();

                    var frequencyCounter = new TotalSequenceSymbolFrequencyCounter<ulong>(syncLinks);
                    var balancedVariantConverter = new BalancedVariantConverter<ulong>(syncLinks);
                    var doubletFrequenciesCache = new LinkFrequenciesCache<ulong>(syncLinks, frequencyCounter);
                    var compressingConvertor = new CompressingConverter<ulong>(syncLinks, balancedVariantConverter, doubletFrequenciesCache);

                    compressingConvertor.Convert(responseSourceArray); sw3.Stop();

                    var totalLinks = syncLinks.Count() - UnicodeMap.MapSize;

                    Console.WriteLine($"{sw3.Elapsed}, {minFrequency}, {responseSourceArray.Length}, {totalLinks}");
                }
            }

            Console.ReadKey();
        }

        private static string GetPageContents(string url)
        {
            const string pageCacheFile = "response.html";

            string pageContents;

            if (File.Exists(pageCacheFile))
            {
                pageContents = File.ReadAllText(pageCacheFile);
            }
            else
            {
                using (var client = new HttpClient())
                {
                    pageContents = client.GetStringAsync(url).AwaitResult();
                }

                File.WriteAllText(pageCacheFile, pageContents);
            }
            return pageContents;
        }

        /// <remarks>
        /// Original algorithm idea: https://en.wikipedia.org/wiki/Byte_pair_encoding .
        /// Slow version (doublets' frequencies dictionary is recreated).
        /// </remarks>
        public static ulong[] PrecompressSequence1(this SynchronizedLinks<ulong> links, ulong[] sequence)
        {
            if (sequence.IsNullOrEmpty())
            {
                return null;
            }

            if (sequence.Length == 1)
            {
                return sequence;
            }

            var newLength = sequence.Length;

            var copy = new ulong[sequence.Length];
            Array.Copy(sequence, copy, sequence.Length);

            Link<ulong> maxDoublet;

            do
            {
                var doubletsFrequencies = new Dictionary<Link<ulong>, ulong>();

                maxDoublet = Link<ulong>.Null;
                ulong maxFrequency = 1;

                for (var i = 1; i < copy.Length; i++)
                {
                    var startIndex = i - 1;

                    while (i < copy.Length && copy[i] == 0)
                    {
                        i++;
                    }

                    if (i == copy.Length)
                    {
                        break;
                    }

                    var doublet = new Link<ulong>(copy[startIndex], copy[i]);

                    if (doubletsFrequencies.TryGetValue(doublet, out ulong frequency))
                    {
                        var newFrequency = frequency + 1;

                        if (maxFrequency < newFrequency)
                        {
                            maxFrequency = newFrequency;
                            maxDoublet = doublet;
                        }

                        doubletsFrequencies[doublet] = newFrequency;
                    }
                    else
                    {
                        doubletsFrequencies.Add(doublet, 1);
                    }
                }

                if (!maxDoublet.IsNull())
                {
                    var maxDoubletLink = links.CreateAndUpdate(maxDoublet.Source, maxDoublet.Target);

                    // Substitute all usages
                    for (var i = 1; i < copy.Length; i++)
                    {
                        if (copy[i - 1] == maxDoublet.Source)
                        {
                            var startIndex = i - 1;

                            while (i < copy.Length && copy[i] == 0)
                            {
                                i++;
                            }

                            if (i == copy.Length)
                            {
                                break;
                            }

                            if (copy[i] == maxDoublet.Target)
                            {
                                copy[startIndex] = maxDoubletLink;
                                copy[i] = 0;
                                newLength--;
                            }
                        }
                    }
                }

            } while (!maxDoublet.IsNull());


            var final = new ulong[newLength];

            var j = 0;
            for (var i = 1; i < copy.Length; i++)
            {
                final[j++] = copy[i - 1];

                while (i < copy.Length && copy[i] == 0)
                {
                    i++;
                }
            }

            //var finalSequence = new ulong[groupedSequence.Count];

            //for (int i = 0; i < finalSequence.Length; i++)
            //{
            //    var part = groupedSequence[i];
            //    finalSequence[i] = part.Length == 1 ? part[0] : sequences.CreateBalancedVariant(part);
            //}

            //return sequences.CreateBalancedVariant(finalSequence);
            return final;
        }

        /// <remarks>
        /// Original algorithm idea: https://en.wikipedia.org/wiki/Byte_pair_encoding .
        /// Faster version (doublets' frequencies dictionary is not recreated).
        /// </remarks>
        public static ulong[] PrecompressSequence2(this SynchronizedLinks<ulong> links, ulong[] sequence)
        {
            if (sequence.IsNullOrEmpty())
            {
                return null;
            }

            if (sequence.Length == 1)
            {
                return sequence;
            }

            var newLength = sequence.Length;
            var copy = new ulong[sequence.Length];
            copy[0] = sequence[0];

            var doubletsFrequencies = new Dictionary<Link<ulong>, ulong>();

            var maxDoublet = Link<ulong>.Null;
            ulong maxFrequency = 1;

            for (var i = 1; i < sequence.Length; i++)
            {
                copy[i] = sequence[i];

                var doublet = new Link<ulong>(sequence[i - 1], sequence[i]);

                if (doubletsFrequencies.TryGetValue(doublet, out ulong frequency))
                {
                    var newFrequency = frequency + 1;

                    if (maxFrequency < newFrequency)
                    {
                        maxFrequency = newFrequency;
                        maxDoublet = doublet;
                    }

                    doubletsFrequencies[doublet] = newFrequency;
                }
                else
                {
                    doubletsFrequencies.Add(doublet, 1);
                }
            }

            while (!maxDoublet.IsNull())
            {
                var maxDoubletSource = maxDoublet.Source;

                var maxDoubletLink = links.CreateAndUpdate(maxDoubletSource, maxDoublet.Target);

                // Substitute all usages
                for (var i = 1; i < copy.Length; i++)
                {
                    var startIndex = i - 1;

                    if (copy[startIndex] == maxDoubletSource)
                    {
                        while (i < copy.Length && copy[i] == 0)
                        {
                            i++;
                        }

                        if (i == copy.Length)
                        {
                            break;
                        }

                        if (copy[i] == maxDoublet.Target)
                        {
                            var oldLeft = copy[startIndex];
                            var oldRight = copy[i];

                            copy[startIndex] = maxDoubletLink;
                            copy[i] = 0; // TODO: Вместо записи нулевых дырок, можно хранить отрицательным числом размер диапазона (дырки) на которую надо прыгнуть, это дополнительно ускорило бы алгоритм.

                            // Требуется отдельно, так как пары могут идти подряд,
                            // например в "ааа" пара "аа" была посчитана дважды
                            doubletsFrequencies[maxDoublet]--;

                            newLength--;

                            if (startIndex > 0)
                            {
                                var previous = startIndex - 1;
                                while (previous >= 0 && copy[previous] == 0)
                                {
                                    previous--;
                                }

                                if (previous >= 0)
                                {
                                    ulong frequency;

                                    var nextOldDoublet = new Link<ulong>(copy[previous], oldLeft);
                                    //if (!nextOldDoublet.Equals(maxDoublet))
                                    {
                                        //doubletsFrequencies[nextOldDoublet]--;
                                        if (doubletsFrequencies.TryGetValue(nextOldDoublet, out frequency))
                                        {
                                            doubletsFrequencies[nextOldDoublet] = frequency - 1;
                                        }
                                    }

                                    var nextNewDoublet = new Link<ulong>(copy[previous], copy[startIndex]);
                                    //doubletsFrequencies[nextNewDoublet]++;
                                    if (doubletsFrequencies.TryGetValue(nextNewDoublet, out frequency))
                                    {
                                        doubletsFrequencies[nextNewDoublet] = frequency + 1;
                                    }
                                    else
                                    {
                                        doubletsFrequencies.Add(nextNewDoublet, 1);
                                    }
                                }
                            }

                            if (i < copy.Length)
                            {
                                var next = i;
                                while (next < copy.Length && copy[next] == 0)
                                {
                                    next++;
                                }

                                if (next < copy.Length)
                                {
                                    ulong frequency;

                                    var nextOldDoublet = new Link<ulong>(oldRight, copy[next]);
                                    //if (!nextOldDoublet.Equals(maxDoublet))
                                    {
                                        //doubletsFrequencies[nextOldDoublet]--;
                                        if (doubletsFrequencies.TryGetValue(nextOldDoublet, out frequency))
                                        {
                                            doubletsFrequencies[nextOldDoublet] = frequency - 1;
                                        }
                                    }

                                    var nextNewDoublet = new Link<ulong>(copy[startIndex], copy[next]);
                                    //doubletsFrequencies[nextNewDoublet]++;
                                    if (doubletsFrequencies.TryGetValue(nextNewDoublet, out frequency))
                                    {
                                        doubletsFrequencies[nextNewDoublet] = frequency + 1;
                                    }
                                    else
                                    {
                                        doubletsFrequencies.Add(nextNewDoublet, 1);
                                    }
                                }
                            }
                        }
                    }
                }

                //doubletsFrequencies[maxDoublet] = 0;
                //doubletsFrequencies.Remove(maxDoublet);

                //if (doubletsFrequencies[maxDoublet] > 0)
                //{

                //}

                maxDoublet = Link<ulong>.Null;
                maxFrequency = 1;

                foreach (var doubletsFrequency in doubletsFrequencies)
                {
                    var frequency = doubletsFrequency.Value;
                    if (frequency > 1)
                    {
                        var doublet = doubletsFrequency.Key;

                        if (maxFrequency < frequency)
                        {
                            maxFrequency = frequency;
                            maxDoublet = doublet;
                        }
                        if (maxFrequency == frequency &&
                            (doublet.Source + doublet.Target) > (maxDoublet.Source + maxDoublet.Target))
                        {
                            maxDoublet = doublet;
                        }
                    }
                }

                //{
                //    var doubletsFrequenciesCheck = new Dictionary<Link, ulong>();
                //    var maxDoubletCheck = Link.Null;
                //    ulong maxFrequencyCheck = 1;

                //    for (var i = 1; i < copy.Length; i++)
                //    {
                //        var startIndex = i - 1;

                //        while (i < copy.Length && copy[i] == 0) i++;
                //        if (i == copy.Length) break;

                //        var doublet = new Link(copy[startIndex], copy[i]);

                //        ulong frequency;
                //        if (doubletsFrequenciesCheck.TryGetValue(doublet, out frequency))
                //        {
                //            var newFrequency = frequency + 1;

                //            if (maxFrequencyCheck < newFrequency)
                //            {
                //                maxFrequencyCheck = newFrequency;
                //                maxDoubletCheck = doublet;
                //            }

                //            doubletsFrequenciesCheck[doublet] = newFrequency;
                //        }
                //        else
                //            doubletsFrequenciesCheck.Add(doublet, 1);
                //    }

                //    if (!maxDoubletCheck.Equals(maxDoublet) || maxFrequency != maxFrequencyCheck)
                //    {

                //    }
                //}
            }

            var final = new ulong[newLength];

            var j = 0;
            for (var i = 1; i < copy.Length; i++)
            {
                final[j++] = copy[i - 1];

                while (i < copy.Length && copy[i] == 0)
                {
                    i++;
                }
            }

            //var finalSequence = new ulong[groupedSequence.Count];

            //for (int i = 0; i < finalSequence.Length; i++)
            //{
            //    var part = groupedSequence[i];
            //    finalSequence[i] = part.Length == 1 ? part[0] : sequences.CreateBalancedVariant(part);
            //}

            //return sequences.CreateBalancedVariant(finalSequence);
            //return sequences.CreateBalancedVariant(final);

            return final;
        }

        public struct Compressor
        {
            private readonly SynchronizedLinks<ulong> _links;
            private Link<ulong> _maxDoublet;
            private ulong _maxFrequency;
            private Link<ulong> _maxDoublet2;
            private ulong _maxFrequency2;
            private readonly Dictionary<Link<ulong>, ulong> _doubletsFrequencies;

            public Compressor(SynchronizedLinks<ulong> links)
            {
                _links = links;
                _maxDoublet = Link<ulong>.Null;
                _maxFrequency = 1;
                _maxDoublet2 = Link<ulong>.Null;
                _maxFrequency2 = 1;
                _doubletsFrequencies = new Dictionary<Link<ulong>, ulong>();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private ulong IncrementFrequency(Link<ulong> doublet)
            {
                if (_doubletsFrequencies.TryGetValue(doublet, out ulong frequency))
                {
                    frequency++;
                    _doubletsFrequencies[doublet] = frequency;
                }
                else
                {
                    frequency = 1;
                    _doubletsFrequencies.Add(doublet, frequency);
                }
                return frequency;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void DecrementFrequency(Link<ulong> doublet)
            {
                if (_doubletsFrequencies.TryGetValue(doublet, out ulong frequency))
                {
                    frequency--;

                    if (frequency == 0)
                    {
                        _doubletsFrequencies.Remove(doublet);
                    }
                    else
                    {
                        _doubletsFrequencies[doublet] = frequency;
                    }
                }
                //return frequency;
            }

            /// <remarks>
            /// Original algorithm idea: https://en.wikipedia.org/wiki/Byte_pair_encoding .
            /// Faster version (doublets' frequencies dictionary is not recreated).
            /// </remarks>
            public ulong[] Precompress0(ulong[] sequence)
            {
                if (sequence.IsNullOrEmpty())
                {
                    return null;
                }

                if (sequence.Length == 1)
                {
                    return sequence;
                }

                var oldLength = sequence.Length;
                var newLength = sequence.Length;

                // Can be faster if source sequence allowed to be changed
                var copy = new ulong[sequence.Length];
                copy[0] = sequence[0];

                for (var i = 1; i < sequence.Length; i++)
                {
                    copy[i] = sequence[i];

                    var doublet = new Link<ulong>(sequence[i - 1], sequence[i]);
                    UpdateMaxDoublet(doublet, IncrementFrequency(doublet));
                }

                while (!_maxDoublet.IsNull())
                {
                    var maxDoubletSource = _maxDoublet.Source;
                    var maxDoubletTarget = _maxDoublet.Target;
                    var maxDoubletResult = _links.CreateAndUpdate(maxDoubletSource, maxDoubletTarget);

                    oldLength--;
                    var oldLengthMinusTwo = oldLength - 1;

                    // Substitute all usages
                    int w = 0, r = 0; // (r == read, w == write)
                    for (; r < oldLength; r++)
                    {
                        if (copy[r] == maxDoubletSource && copy[r + 1] == maxDoubletTarget)
                        {
                            if (r > 0)
                            {
                                var previous = copy[w - 1];
                                DecrementFrequency(new Link<ulong>(previous, maxDoubletSource));
                                IncrementFrequency(new Link<ulong>(previous, maxDoubletResult));
                            }
                            if (r < oldLengthMinusTwo)
                            {
                                var next = copy[r + 2];
                                DecrementFrequency(new Link<ulong>(maxDoubletTarget, next));
                                IncrementFrequency(new Link<ulong>(maxDoubletResult, next));
                            }

                            copy[w++] = maxDoubletResult;
                            r++;
                            newLength--;
                        }
                        else
                        {
                            copy[w++] = copy[r];
                        }
                    }
                    copy[w] = copy[r];

                    _doubletsFrequencies.Remove(_maxDoublet);

                    oldLength = newLength;

                    // Медленный вариант UpdateMaxDoublet
                    //_maxDoublet = Link.Null;
                    //_maxFrequency = 1;

                    // TODO: Разобраться почему, если переместить сюда строчку "_doubletsFrequencies.Remove(_maxDoublet);" алгоритм зацикливается

                    //foreach (var doubletsFrequency in _doubletsFrequencies)
                    //    UpdateMaxDoublet(doubletsFrequency.Key, doubletsFrequency.Value);

                    // Быстрее
                    UpdateMaxDoublet2();
                }

                var final = new ulong[newLength];
                Array.Copy(copy, final, newLength);

                return final;
            }

            /// <remarks>
            /// Original algorithm idea: https://en.wikipedia.org/wiki/Byte_pair_encoding .
            /// Faster version (doublets' frequencies dictionary is not recreated).
            /// </remarks>
            public ulong[] Precompress1(ulong[] sequence)
            {
                if (sequence.IsNullOrEmpty())
                {
                    return null;
                }

                if (sequence.Length == 1)
                {
                    return sequence;
                }

                var newLength = sequence.Length;
                var copy = new ulong[sequence.Length];
                copy[0] = sequence[0];

                for (var i = 1; i < sequence.Length; i++)
                {
                    copy[i] = sequence[i];

                    var doublet = new Link<ulong>(sequence[i - 1], sequence[i]);

                    if (_doubletsFrequencies.TryGetValue(doublet, out ulong frequency))
                    {
                        var newFrequency = frequency + 1;

                        if (_maxFrequency < newFrequency)
                        {
                            _maxFrequency = newFrequency;
                            _maxDoublet = doublet;
                        }

                        _doubletsFrequencies[doublet] = newFrequency;
                    }
                    else
                    {
                        _doubletsFrequencies.Add(doublet, 1);
                    }
                }

                while (!_maxDoublet.IsNull())
                {
                    var maxDoublet = _maxDoublet;

                    //ResetMaxDoublet();

                    var maxDoubletSource = maxDoublet.Source;

                    var maxDoubletLink = _links.CreateAndUpdate(maxDoubletSource, maxDoublet.Target);

                    // Substitute all usages
                    for (var i = 1; i < copy.Length; i++)
                    {
                        var startIndex = i - 1;

                        if (copy[startIndex] == maxDoubletSource)
                        {
                            while (i < copy.Length && copy[i] == 0)
                            {
                                i++;
                            }

                            if (i == copy.Length)
                            {
                                break;
                            }

                            if (copy[i] == maxDoublet.Target)
                            {
                                var oldLeft = copy[startIndex];
                                var oldRight = copy[i];

                                copy[startIndex] = maxDoubletLink;
                                copy[i] = 0; // TODO: Вместо записи нулевых дырок, можно хранить отрицательным числом размер диапазона (дырки) на которую надо прыгнуть, это дополнительно ускорило бы алгоритм.

                                // Требуется отдельно, так как пары могут идти подряд,
                                // например в "ааа" пара "аа" была посчитана дважды
                                //doubletsFrequencies[maxDoublet]--;

                                ulong frequency;
                                //ulong frequency = _doubletsFrequencies[maxDoublet];
                                //if (frequency == 1)
                                //    _doubletsFrequencies.Remove(maxDoublet);
                                //else
                                //    _doubletsFrequencies[maxDoublet] = frequency - 1;

                                //UpdateMaxDoublet2(maxDoublet, frequency);

                                newLength--;

                                if (startIndex > 0)
                                {
                                    var previous = startIndex - 1;
                                    while (previous >= 0 && copy[previous] == 0)
                                    {
                                        previous--;
                                    }

                                    if (previous >= 0)
                                    {
                                        var previousOldDoublet = new Link<ulong>(copy[previous], oldLeft);
                                        //if (!nextOldDoublet.Equals(maxDoublet))
                                        {
                                            //doubletsFrequencies[nextOldDoublet]--;
                                            if (_doubletsFrequencies.TryGetValue(previousOldDoublet, out frequency))
                                            {
                                                if (frequency == 1)
                                                {
                                                    _doubletsFrequencies.Remove(previousOldDoublet);
                                                }
                                                else
                                                {
                                                    _doubletsFrequencies[previousOldDoublet] = frequency - 1;
                                                }

                                                //if(!maxDoublet.Equals(previousOldDoublet))
                                                //    UpdateMaxDoublet2(previousOldDoublet, frequency - 1);
                                            }
                                        }

                                        var previousNewDoublet = new Link<ulong>(copy[previous], copy[startIndex]);
                                        //doubletsFrequencies[nextNewDoublet]++;
                                        if (_doubletsFrequencies.TryGetValue(previousNewDoublet, out frequency))
                                        {
                                            _doubletsFrequencies[previousNewDoublet] = frequency + 1;

                                            //UpdateMaxDoublet(previousNewDoublet, frequency + 1);
                                        }
                                        else
                                        {
                                            _doubletsFrequencies.Add(previousNewDoublet, 1);
                                        }
                                    }
                                }

                                if (i < copy.Length)
                                {
                                    var next = i;
                                    while (next < copy.Length && copy[next] == 0)
                                    {
                                        next++;
                                    }

                                    if (next < copy.Length)
                                    {
                                        var nextOldDoublet = new Link<ulong>(oldRight, copy[next]);
                                        //if (!nextOldDoublet.Equals(maxDoublet))
                                        {
                                            //doubletsFrequencies[nextOldDoublet]--;
                                            if (_doubletsFrequencies.TryGetValue(nextOldDoublet, out frequency))
                                            {
                                                if (frequency == 1)
                                                {
                                                    _doubletsFrequencies.Remove(nextOldDoublet);
                                                }
                                                else
                                                {
                                                    _doubletsFrequencies[nextOldDoublet] = frequency - 1;
                                                }

                                                //if (!maxDoublet.Equals(nextOldDoublet))
                                                //    UpdateMaxDoublet2(nextOldDoublet, frequency - 1);
                                            }
                                        }

                                        var nextNewDoublet = new Link<ulong>(copy[startIndex], copy[next]);
                                        //doubletsFrequencies[nextNewDoublet]++;
                                        if (_doubletsFrequencies.TryGetValue(nextNewDoublet, out frequency))
                                        {
                                            _doubletsFrequencies[nextNewDoublet] = frequency + 1;

                                            //UpdateMaxDoublet(nextNewDoublet, frequency + 1);
                                        }
                                        else
                                        {
                                            _doubletsFrequencies.Add(nextNewDoublet, 1);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //////if (!_maxDoublet2.IsNull())
                    //////{
                    //////    UpdateMaxDoublet(_maxDoublet2, _maxFrequency2);
                    //////}

                    //doubletsFrequencies[maxDoublet] = 0;
                    //doubletsFrequencies.Remove(maxDoublet);

                    //if (doubletsFrequencies[maxDoublet] > 0)
                    //{

                    //}

                    _doubletsFrequencies.Remove(_maxDoublet);

                    _maxDoublet = Link<ulong>.Null;
                    _maxFrequency = 1;

                    foreach (var doubletsFrequency in _doubletsFrequencies)
                    {
                        UpdateMaxDoublet(doubletsFrequency.Key, doubletsFrequency.Value);
                    }

                    //_maxDoublet = Link.Null;
                    //_maxFrequency = 1;

                    //for (var i = 1; i < copy.Length; i++)
                    //{
                    //    var startIndex = i - 1;

                    //    while (i < copy.Length && copy[i] == 0) i++;
                    //    if (i == copy.Length) break;

                    //    var doublet = new Link(copy[startIndex], copy[i]);

                    //    var frequency = _doubletsFrequencies[doublet];
                    //    UpdateMaxDoublet(doublet, frequency);
                    //}

                    //{
                    //    var doubletsFrequenciesCheck = new Dictionary<Link, ulong>();
                    //    var maxDoubletCheck = Link.Null;
                    //    ulong maxFrequencyCheck = 1;

                    //    for (var i = 1; i < copy.Length; i++)
                    //    {
                    //        var startIndex = i - 1;

                    //        while (i < copy.Length && copy[i] == 0) i++;
                    //        if (i == copy.Length) break;

                    //        var doublet = new Link(copy[startIndex], copy[i]);

                    //        ulong frequency;
                    //        if (doubletsFrequenciesCheck.TryGetValue(doublet, out frequency))
                    //        {
                    //            var newFrequency = frequency + 1;

                    //            if (maxFrequencyCheck < newFrequency)
                    //            {
                    //                maxFrequencyCheck = newFrequency;
                    //                maxDoubletCheck = doublet;
                    //            }

                    //            doubletsFrequenciesCheck[doublet] = newFrequency;
                    //        }
                    //        else
                    //            doubletsFrequenciesCheck.Add(doublet, 1);
                    //    }

                    //    if (!maxDoubletCheck.Equals(maxDoublet) || maxFrequency != maxFrequencyCheck)
                    //    {

                    //    }
                    //}
                }

                var final = new ulong[newLength];

                var j = 0;
                for (var i = 0; i < copy.Length; i++)
                {
                    while (i < copy.Length && copy[i] == 0)
                    {
                        i++;
                    }

                    if (i == copy.Length)
                    {
                        break;
                    }

                    final[j++] = copy[i];
                }

                //var finalSequence = new ulong[groupedSequence.Count];

                //for (int i = 0; i < finalSequence.Length; i++)
                //{
                //    var part = groupedSequence[i];
                //    finalSequence[i] = part.Length == 1 ? part[0] : sequences.CreateBalancedVariant(part);
                //}

                //return sequences.CreateBalancedVariant(finalSequence);
                //return sequences.CreateBalancedVariant(final);

                return final;
            }

            /// <remarks>
            /// Original algorithm idea: https://en.wikipedia.org/wiki/Byte_pair_encoding .
            /// Faster version (doublets' frequencies dictionary is not recreated).
            /// </remarks>
            public ulong[] Precompress2(ulong[] sequence)
            {
                if (sequence.IsNullOrEmpty())
                {
                    return null;
                }

                if (sequence.Length == 1)
                {
                    return sequence;
                }

                var newLength = sequence.Length;
                var copy = new ulong[sequence.Length];
                copy[0] = sequence[0];

                for (var i = 1; i < sequence.Length; i++)
                {
                    copy[i] = sequence[i];

                    var doublet = new Link<ulong>(sequence[i - 1], sequence[i]);

                    if (_doubletsFrequencies.TryGetValue(doublet, out ulong frequency))
                    {
                        var newFrequency = frequency + 1;

                        if (_maxFrequency < newFrequency)
                        {
                            _maxFrequency = newFrequency;
                            _maxDoublet = doublet;
                        }

                        _doubletsFrequencies[doublet] = newFrequency;
                    }
                    else
                    {
                        _doubletsFrequencies.Add(doublet, 1);
                    }
                }

                //var tempDoublet = new Link();

                while (!_maxDoublet.IsNull())
                {
                    var maxDoublet = _maxDoublet;

                    ResetMaxDoublet();

                    var maxDoubletSource = maxDoublet.Source;

                    var maxDoubletLink = _links.CreateAndUpdate(maxDoubletSource, maxDoublet.Target);

                    // Substitute all usages
                    for (var i = 1; i < copy.Length; i++)
                    {
                        var startIndex = i - 1;

                        while (startIndex < copy.Length && copy[startIndex] == 0)
                        {
                            i++;
                            startIndex++;
                        }
                        if (startIndex == copy.Length - 1)
                        {
                            break;
                        }

                        if (copy[startIndex] == maxDoubletSource)
                        {
                            while (i < copy.Length && copy[i] == 0)
                            {
                                i++;
                            }

                            if (i == copy.Length)
                            {
                                break;
                            }

                            if (copy[i] == maxDoublet.Target)
                            {
                                var oldLeft = copy[startIndex];
                                var oldRight = copy[i];

                                copy[startIndex] = maxDoubletLink;
                                copy[i] = 0;
                                // TODO: Вместо записи нулевых дырок, можно хранить отрицательным числом размер диапазона (дырки) на которую надо прыгнуть, это дополнительно ускорило бы алгоритм.

                                // Требуется отдельно, так как пары могут идти подряд,
                                // например в "ааа" пара "аа" была посчитана дважды
                                //doubletsFrequencies[maxDoublet]--;

                                var frequency = _doubletsFrequencies[maxDoublet];
                                if (frequency == 1)
                                {
                                    _doubletsFrequencies.Remove(maxDoublet);
                                }
                                else
                                {
                                    _doubletsFrequencies[maxDoublet] = frequency - 1;
                                }

                                //UpdateMaxDoublet2(maxDoublet, frequency);

                                newLength--;

                                if (startIndex > 0)
                                {
                                    var previous = startIndex - 1;
                                    while (previous >= 0 && copy[previous] == 0)
                                    {
                                        previous--;
                                    }

                                    if (previous >= 0)
                                    {
                                        var previousOldDoublet = new Link<ulong>(copy[previous], oldLeft);
                                        //if (!nextOldDoublet.Equals(maxDoublet))
                                        {
                                            //doubletsFrequencies[nextOldDoublet]--;
                                            if (_doubletsFrequencies.TryGetValue(previousOldDoublet, out frequency))
                                            {
                                                if (frequency == 1)
                                                {
                                                    _doubletsFrequencies.Remove(previousOldDoublet);
                                                }
                                                else
                                                {
                                                    _doubletsFrequencies[previousOldDoublet] = frequency - 1;
                                                }

                                                //if(!maxDoublet.Equals(previousOldDoublet))
                                                //    UpdateMaxDoublet2(previousOldDoublet, frequency - 1);
                                            }
                                        }

                                        var previousNewDoublet = new Link<ulong>(copy[previous], copy[startIndex]);
                                        //doubletsFrequencies[nextNewDoublet]++;
                                        if (_doubletsFrequencies.TryGetValue(previousNewDoublet, out frequency))
                                        {
                                            _doubletsFrequencies[previousNewDoublet] = frequency + 1;

                                            //if (!maxDoublet.Equals(previousNewDoublet))
                                            UpdateMaxDoublet(previousNewDoublet, frequency + 1);
                                        }
                                        else
                                        {
                                            _doubletsFrequencies.Add(previousNewDoublet, 1);
                                        }
                                    }
                                }

                                if (i < copy.Length)
                                {
                                    var next = i;
                                    while (next < copy.Length && copy[next] == 0)
                                    {
                                        next++;
                                    }

                                    if (next < copy.Length)
                                    {
                                        var nextOldDoublet = new Link<ulong>(oldRight, copy[next]);
                                        //if (!nextOldDoublet.Equals(maxDoublet))
                                        {
                                            //doubletsFrequencies[nextOldDoublet]--;
                                            if (_doubletsFrequencies.TryGetValue(nextOldDoublet, out frequency))
                                            {
                                                if (frequency == 1)
                                                {
                                                    _doubletsFrequencies.Remove(nextOldDoublet);
                                                }
                                                else
                                                {
                                                    _doubletsFrequencies[nextOldDoublet] = frequency - 1;
                                                }

                                                //if (!maxDoublet.Equals(nextOldDoublet))
                                                //    UpdateMaxDoublet2(nextOldDoublet, frequency - 1);
                                            }
                                        }

                                        var nextNewDoublet = new Link<ulong>(copy[startIndex], copy[next]);
                                        //doubletsFrequencies[nextNewDoublet]++;
                                        if (_doubletsFrequencies.TryGetValue(nextNewDoublet, out frequency))
                                        {
                                            _doubletsFrequencies[nextNewDoublet] = frequency + 1;

                                            //if (!maxDoublet.Equals(nextNewDoublet))
                                            UpdateMaxDoublet(nextNewDoublet, frequency + 1);
                                        }
                                        else
                                        {
                                            _doubletsFrequencies.Add(nextNewDoublet, 1);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            while (i < copy.Length && copy[i] == 0)
                            {
                                i++;
                            }

                            if (i == copy.Length)
                            {
                                break;
                            }

                            //tempDoublet.Source = copy[startIndex];
                            //tempDoublet.Target = copy[i];

                            var doublet = new Link<ulong>(copy[startIndex], copy[i]);

                            //if (!maxDoublet.Equals(doublet))
                            //{
                            if (_doubletsFrequencies.TryGetValue(doublet, out ulong frequency))
                            {
                                UpdateMaxDoublet(doublet, frequency);
                            }
                            //}
                        }
                    }

                    //////if (!_maxDoublet2.IsNull())
                    //////{
                    //////    UpdateMaxDoublet(_maxDoublet2, _maxFrequency2);
                    //////}

                    //_maxDoublet = Link.Null;
                    //_maxFrequency = 1;

                    //foreach (var doubletsFrequency in _doubletsFrequencies)
                    //    UpdateMaxDoublet(doubletsFrequency.Key, doubletsFrequency.Value);
                }

                var final = new ulong[newLength];

                var j = 0;
                for (var i = 0; i < copy.Length; i++)
                {
                    while (i < copy.Length && copy[i] == 0)
                    {
                        i++;
                    }

                    if (i == copy.Length)
                    {
                        break;
                    }

                    final[j++] = copy[i];
                }

                return final;
            }

            /// <remarks>
            /// Original algorithm idea: https://en.wikipedia.org/wiki/Byte_pair_encoding .
            /// If doublet repeats twice it is maximum doublet.
            /// </remarks>
            public ulong[] Precompress3(ulong[] sequence)
            {
                if (sequence.IsNullOrEmpty())
                {
                    return null;
                }

                if (sequence.Length == 1)
                {
                    return sequence;
                }

                var oldLength = sequence.Length;
                var newLength = sequence.Length;

                // Can be faster if source sequence allowed to be changed
                var copy = new ulong[sequence.Length];
                Array.Copy(sequence, copy, copy.Length);

                var set = new HashSet<Link<ulong>>();

                for (var i = 1; i < sequence.Length; i++)
                {
                    var doublet = new Link<ulong>(sequence[i - 1], sequence[i]);

                    //UpdateMaxDoublet(doublet, IncrementFrequency(doublet));
                    //if(_maxFrequency >= 2)
                    //    break;

                    if (!set.Add(doublet))
                    {
                        _maxDoublet = doublet;
                        //_maxFrequency = 2;
                        break;
                    }
                }

                while (!_maxDoublet.IsNull())
                {
                    var maxDoubletSource = _maxDoublet.Source;
                    var maxDoubletTarget = _maxDoublet.Target;
                    var maxDoubletResult = _links.CreateAndUpdate(maxDoubletSource, maxDoubletTarget);

                    oldLength--;
                    //var oldLengthMinusTwo = oldLength - 1;

                    // Substitute all usages
                    int w = 0, r = 0; // (r == read, w == write)
                    for (; r < oldLength; r++)
                    {
                        if (copy[r] == maxDoubletSource && copy[r + 1] == maxDoubletTarget)
                        {
                            //if (r > 0)
                            //{
                            //    var previous = copy[w - 1];
                            //    DecrementFrequency(new Link(previous, maxDoubletSource));
                            //    IncrementFrequency(new Link(previous, maxDoubletResult));
                            //}
                            //if (r < oldLengthMinusTwo)
                            //{
                            //    var next = copy[r + 2];
                            //    DecrementFrequency(new Link(maxDoubletTarget, next));
                            //    IncrementFrequency(new Link(maxDoubletResult, next));
                            //}

                            copy[w++] = maxDoubletResult;
                            r++;
                            newLength--;
                        }
                        else
                        {
                            copy[w++] = copy[r];
                        }
                    }
                    copy[w] = copy[r];

                    //_doubletsFrequencies.Remove(_maxDoublet);

                    _maxDoublet = Link<ulong>.Null;

                    //ResetMaxDoublet();
                    set.Clear();
                    //_doubletsFrequencies = new Dictionary<Link, ulong>();

                    oldLength = newLength;

                    for (var i = 1; i < newLength; i++)
                    {
                        var doublet = new Link<ulong>(copy[i - 1], copy[i]);

                        //UpdateMaxDoublet(doublet, IncrementFrequency(doublet));
                        //if (_maxFrequency >= 2)
                        //    break;

                        if (!set.Add(doublet))
                        {
                            _maxDoublet = doublet;
                            //_maxFrequency = 2;
                            break; ;
                        }
                    }

                    // Медленный вариант UpdateMaxDoublet
                    //_maxDoublet = Link.Null;
                    //_maxFrequency = 1;

                    // TODO: Разобраться почему, если переместить сюда строчку "_doubletsFrequencies.Remove(_maxDoublet);" алгоритм зацикливается

                    //foreach (var doubletsFrequency in _doubletsFrequencies)
                    //    UpdateMaxDoublet(doubletsFrequency.Key, doubletsFrequency.Value);

                    // Быстрее
                    //UpdateMaxDoublet2();
                }

                var final = new ulong[newLength];
                Array.Copy(copy, final, newLength);

                return final;
            }

            /// <remarks>
            /// Original algorithm idea: https://en.wikipedia.org/wiki/Byte_pair_encoding .
            /// If doublet repeats twice it is maximum doublet.
            /// </remarks>
            public ulong[] Precompress4(ulong[] sequence)
            {
                if (sequence.IsNullOrEmpty())
                {
                    return null;
                }

                if (sequence.Length == 1)
                {
                    return sequence;
                }

                var oldLength = sequence.Length;
                var newLength = sequence.Length;

                // Can be faster if source sequence allowed to be changed
                var copy = new ulong[sequence.Length];
                Array.Copy(sequence, copy, copy.Length);

                var set = new HashSet<Link<ulong>>();

                for (var i = 1; i < sequence.Length; i++)
                {
                    var doublet = new Link<ulong>(sequence[i - 1], sequence[i]);

                    //UpdateMaxDoublet(doublet, IncrementFrequency(doublet));
                    //if(_maxFrequency >= 2)
                    //    break;

                    if (!set.Add(doublet))
                    {
                        _maxDoublet = doublet;
                        //_maxFrequency = 2;
                        break;
                    }
                }

                while (!_maxDoublet.IsNull())
                {
                    var maxDoubletSource = _maxDoublet.Source;
                    var maxDoubletTarget = _maxDoublet.Target;
                    var maxDoubletResult = _links.CreateAndUpdate(maxDoubletSource, maxDoubletTarget);

                    oldLength--;

                    _maxDoublet = Link<ulong>.Null;
                    set.Clear();

                    // Substitute all usages
                    int w = 0, r = 0; // (r == read, w == write)
                    for (; r < oldLength; r++)
                    {
                        if (copy[r] == maxDoubletSource && copy[r + 1] == maxDoubletTarget)
                        {
                            //if (_maxDoublet.IsNull())
                            //{
                            //    if (r > 0)
                            //    {
                            //        var previous = copy[w - 1];
                            //        set.Remove(new Link(previous, maxDoubletSource));
                            //        var doublet = new Link(previous, maxDoubletResult);
                            //        if (!set.Add(doublet)) _maxDoublet = doublet;
                            //        //DecrementFrequency(new Link(previous, maxDoubletSource));
                            //        //IncrementFrequency(new Link(previous, maxDoubletResult));
                            //    }
                            //    if (r < oldLengthMinusTwo)
                            //    {
                            //        var next = copy[r + 2];
                            //        set.Remove(new Link(maxDoubletTarget, next));
                            //        var doublet = new Link(maxDoubletResult, next);
                            //        if (!set.Add(doublet)) _maxDoublet = doublet;
                            //        //DecrementFrequency(new Link(maxDoubletTarget, next));
                            //        //IncrementFrequency(new Link(maxDoubletResult, next));
                            //    }
                            //}

                            copy[w++] = maxDoubletResult;
                            r++;
                            newLength--;
                        }
                        else
                        {
                            //if (_maxDoublet.IsNull() && w > 0) // 8 sec
                            //{
                            //    var doublet = new Link(copy[w - 1], copy[w]);
                            //    if (!set.Add(doublet)) _maxDoublet = doublet;
                            //}

                            if (_maxDoublet.IsNull()) // 4 sec
                            {
                                var doublet = new Link<ulong>(copy[r], copy[r + 1]);
                                if (!set.Add(doublet))
                                {
                                    _maxDoublet = doublet;
                                }
                            }
                            copy[w++] = copy[r];

                            //if (_maxDoublet.IsNull()) // 8 sec
                            //{
                            //    var doublet = new Link(copy[w - 1], copy[w]);
                            //    if (!set.Add(doublet)) _maxDoublet = doublet;
                            //}
                        }
                    }
                    //if (_maxDoublet.IsNull()) // 8 sec
                    //{
                    //    var doublet = new Link(copy[w - 1], copy[w]);
                    //    if (!set.Add(doublet)) _maxDoublet = doublet;
                    //}
                    copy[w] = copy[r];

                    //_doubletsFrequencies.Remove(_maxDoublet);

                    //_maxDoublet = Link.Null;
                    //set.Clear();

                    oldLength = newLength;

                    //for (var i = 1; i < newLength; i++)
                    //{
                    //    var doublet = new Link(copy[i - 1], copy[i]);

                    //    //UpdateMaxDoublet(doublet, IncrementFrequency(doublet));
                    //    //if (_maxFrequency >= 2)
                    //    //    break;

                    //    if (!set.Add(doublet))
                    //    {
                    //        _maxDoublet = doublet;
                    //        //_maxFrequency = 2;
                    //        break; ;
                    //    }
                    //}

                    // Медленный вариант UpdateMaxDoublet
                    //_maxDoublet = Link.Null;
                    //_maxFrequency = 1;

                    // TODO: Разобраться почему, если переместить сюда строчку "_doubletsFrequencies.Remove(_maxDoublet);" алгоритм зацикливается

                    //foreach (var doubletsFrequency in _doubletsFrequencies)
                    //    UpdateMaxDoublet(doubletsFrequency.Key, doubletsFrequency.Value);

                    // Быстрее
                    //UpdateMaxDoublet2();
                }

                var final = new ulong[newLength];
                Array.Copy(copy, final, newLength);

                return final;
            }

            /// <remarks>
            /// Original algorithm idea: https://en.wikipedia.org/wiki/Byte_pair_encoding .
            /// Global dictionary
            /// </remarks>
            public ulong[] Precompress5(ulong[] sequence)
            {
                if (sequence.IsNullOrEmpty())
                {
                    return null;
                }

                if (sequence.Length == 1)
                {
                    return sequence;
                }

                var oldLength = sequence.Length;
                var newLength = sequence.Length;

                // Can be faster if source sequence allowed to be changed
                var copy = new ulong[sequence.Length];
                copy[0] = sequence[0];

                for (var i = 1; i < sequence.Length; i++)
                {
                    copy[i] = sequence[i];

                    var doublet = new Link<ulong>(sequence[i - 1], sequence[i]);
                    UpdateMaxDoublet(doublet, IncrementFrequency(doublet));
                }

                while (!_maxDoublet.IsNull())
                {
                    var maxDoubletSource = _maxDoublet.Source;
                    var maxDoubletTarget = _maxDoublet.Target;
                    var maxDoubletResult = _links.CreateAndUpdate(maxDoubletSource, maxDoubletTarget);

                    oldLength--;
                    //var oldLengthMinusTwo = oldLength - 1;

                    // Substitute all usages
                    int w = 0, r = 0; // (r == read, w == write)
                    for (; r < oldLength; r++)
                    {
                        if (copy[r] == maxDoubletSource && copy[r + 1] == maxDoubletTarget)
                        {
                            //if (r > 0)
                            //{
                            //    var previous = copy[w - 1];
                            //    DecrementFrequency(new Link(previous, maxDoubletSource));
                            //    IncrementFrequency(new Link(previous, maxDoubletResult));
                            //}
                            //if (r < oldLengthMinusTwo)
                            //{
                            //    var next = copy[r + 2];
                            //    DecrementFrequency(new Link(maxDoubletTarget, next));
                            //    IncrementFrequency(new Link(maxDoubletResult, next));
                            //}

                            copy[w++] = maxDoubletResult;
                            r++;
                            newLength--;
                        }
                        else
                        {
                            copy[w++] = copy[r];
                        }
                    }
                    copy[w] = copy[r];

                    _doubletsFrequencies.Remove(_maxDoublet);

                    oldLength = newLength;

                    // Медленный вариант UpdateMaxDoublet
                    //_maxDoublet = Link.Null;
                    //_maxFrequency = 1;

                    // TODO: Разобраться почему, если переместить сюда строчку "_doubletsFrequencies.Remove(_maxDoublet);" алгоритм зацикливается

                    //foreach (var doubletsFrequency in _doubletsFrequencies)
                    //    UpdateMaxDoublet(doubletsFrequency.Key, doubletsFrequency.Value);

                    // Быстрее
                    UpdateMaxDoublet2();
                }

                var final = new ulong[newLength];
                Array.Copy(copy, final, newLength);

                return final;
            }

            public ulong Compress(ulong[] sequence)
            {
                var precompressedSequence = Precompress1(sequence);
                var balancedVariantConverter = new BalancedVariantConverter<ulong>(_links);
                return balancedVariantConverter.Convert(precompressedSequence);
            }

            private void ResetMaxDoublet()
            {
                _maxDoublet = Link<ulong>.Null;
                _maxFrequency = 1;
                _maxDoublet2 = Link<ulong>.Null;
                _maxFrequency2 = 1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void UpdateMaxDoublet(Link<ulong> doublet, ulong frequency)
            {
                if (frequency > 1)
                {
                    if (_maxFrequency < frequency)
                    {
                        _maxFrequency = frequency;
                        _maxDoublet = doublet;
                    }
                    else if (_maxFrequency == frequency &&
                        (doublet.Source + doublet.Target) > (_maxDoublet.Source + _maxDoublet.Target))
                    {
                        _maxDoublet = doublet;
                    }
                }
            }

            private void UpdateMaxDoublet2(Link<ulong> doublet, ulong frequency)
            {
                if (!_maxDoublet.Equals(doublet))
                {
                    if (_maxDoublet2.Equals(doublet))
                    {
                        _maxFrequency2 = frequency;
                    }
                    else if (_maxFrequency2 < frequency)
                    {
                        _maxFrequency2 = frequency;
                        _maxDoublet2 = doublet;
                    }
                    else if (_maxFrequency2 == frequency &&
                             (doublet.Source + doublet.Target) > (_maxDoublet2.Source + _maxDoublet2.Target))
                    {
                        _maxDoublet = doublet;
                    }
                }
            }

            private void UpdateMaxDoublet()
            {
                ResetMaxDoublet();

                foreach(var entry in _doubletsFrequencies)
                {
                    var doublet = entry.Key;
                    var frequency = entry.Value;
                    if (frequency > 1)
                    {
                        if (_maxFrequency < frequency)
                        {
                            _maxFrequency = frequency;
                            _maxDoublet = doublet;
                        }
                        else if (_maxFrequency == frequency &&
                            (doublet.Source + doublet.Target) > (_maxDoublet.Source + _maxDoublet.Target))
                        {
                            _maxDoublet = doublet;
                        }
                    }
                }
            }

            private void UpdateMaxDoublet2()
            {
                ResetMaxDoublet();

                foreach(var entry in _doubletsFrequencies)
                {
                    var doublet = entry.Key;
                    var frequency = entry.Value;
                    if (frequency > 1)
                    {
                        if (_maxFrequency > frequency)
                        {
                            continue;
                        }

                        if (_maxFrequency < frequency)
                        {
                            _maxFrequency = frequency;
                            _maxDoublet = doublet;
                        }
                        else if (_maxFrequency == frequency &&
                            (doublet.Source + doublet.Target) > (_maxDoublet.Source + _maxDoublet.Target))
                        {
                            _maxDoublet = doublet;
                        }
                    }
                }
            }
        }
    }
}