using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using Platform.Data.Core.Pairs;
using Platform.Data.Core.Sequences;
using Platform.Helpers;
using Platform.Helpers.Collections;
using Platform.Helpers.Threading;

namespace Platform.Sandbox
{
    public static class CompressionExperiments
    {
        public static void Test()
        {
            File.Delete("web.links");

            using (var memoryManager = new LinksMemoryManager("web.links", 8 * 1024 * 1024))
            using (var links = new Links(memoryManager))
            {
                UnicodeMap.InitNew(links);

                var sequences = new Sequences(links);

                // Get content
                const string url = "https://en.wikipedia.org/wiki/Main_Page";
                const string pageCacheFile = "response.html";

                string pageContents;

                if (File.Exists(pageCacheFile))
                    pageContents = File.ReadAllText(pageCacheFile);
                else
                {
                    using (var client = new HttpClient())
                        pageContents = client.GetStringAsync(url).AwaitResult();
                    File.WriteAllText(pageCacheFile, pageContents);
                }

                var totalChars = url.Length + pageContents.Length;

                Global.Trash = totalChars;

                var urlLink = sequences.CreateBalancedVariant(UnicodeMap.FromStringToLinkArray(url));

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

                ulong[] responseCompressedArray3 = null;

                for (var i = 0; i < 5; i++)
                {
                    var sw3 = Stopwatch.StartNew();
                    var compressor = new Data.Core.Sequences.Compressor(links, sequences);
                    responseCompressedArray3 = compressor.Precompress(responseSourceArray); sw3.Stop();
                    Console.WriteLine(sw3.Elapsed);
                }

                //for (int i = 0; i < responseCompressedArray1.Length; i++)
                //{
                //    if (responseCompressedArray1[i] != responseCompressedArray2[i])
                //    {

                //    }
                //}

                //var responseLink1 = sequences.CreateBalancedVariant(responseCompressedArray1);
                var responseLink2 = sequences.CreateBalancedVariant(responseCompressedArray3);

                //var decompress1 = sequences.FormatSequence(responseLink1);
                var decompress2 = sequences.FormatSequence(responseLink2);

                Global.Trash = decompress2;

                //for (int i = 0; i < decompress1.Length; i++)
                //{
                //    if (decompress1[i] != decompress2[i])
                //    {

                //    }
                //}

                var unpack = UnicodeMap.FromSequenceLinkToString(responseLink2, links);

                Global.Trash = (unpack == pageContents);

                // TODO: Combine Groups and Compression (first Compression, then Groups)

                var totalLinks = links.Count() - UnicodeMap.MapSize;

                Global.Trash = totalLinks;

                links.Create(urlLink, responseLink2);

                var divLinksArray = UnicodeMap.FromStringToLinkArray("div");

                var fullyMatched = sequences.GetAllMatchingSequences1(divLinksArray);
                var partiallyMatched = sequences.GetAllPartiallyMatchingSequences1(divLinksArray);

                var intersection = fullyMatched.Intersect(partiallyMatched);

            }

            Console.ReadKey();
        }

        /// <remarks>
        /// Original algorithm idea: https://en.wikipedia.org/wiki/Byte_pair_encoding .
        /// Slow version (pairs' frequencies dictionary is recreated).
        /// </remarks>
        public static ulong[] PrecompressSequence1(this Links links, ulong[] sequence)
        {
            if (sequence.IsNullOrEmpty())
                return null;

            if (sequence.Length == 1)
                return sequence;

            var newLength = sequence.Length;

            var copy = new ulong[sequence.Length];
            Array.Copy(sequence, copy, sequence.Length);

            Link maxPair;

            do
            {
                var pairsFrequencies = new Dictionary<Link, ulong>();

                maxPair = Link.Null;
                ulong maxFrequency = 1;

                for (var i = 1; i < copy.Length; i++)
                {
                    var startIndex = i - 1;

                    while (i < copy.Length && copy[i] == 0) i++;
                    if (i == copy.Length) break;

                    var pair = new Link(copy[startIndex], copy[i]);

                    ulong frequency;
                    if (pairsFrequencies.TryGetValue(pair, out frequency))
                    {
                        var newFrequency = frequency + 1;

                        if (maxFrequency < newFrequency)
                        {
                            maxFrequency = newFrequency;
                            maxPair = pair;
                        }

                        pairsFrequencies[pair] = newFrequency;
                    }
                    else
                        pairsFrequencies.Add(pair, 1);
                }

                if (!maxPair.IsNull())
                {
                    var maxPairLink = links.Create(maxPair.Source, maxPair.Target);

                    // Substitute all usages
                    for (var i = 1; i < copy.Length; i++)
                    {
                        if (copy[i - 1] == maxPair.Source)
                        {
                            var startIndex = i - 1;

                            while (i < copy.Length && copy[i] == 0) i++;
                            if (i == copy.Length) break;

                            if (copy[i] == maxPair.Target)
                            {
                                copy[startIndex] = maxPairLink;
                                copy[i] = 0;
                                newLength--;
                            }
                        }
                    }
                }

            } while (!maxPair.IsNull());


            var final = new ulong[newLength];

            var j = 0;
            for (var i = 1; i < copy.Length; i++)
            {
                final[j++] = copy[i - 1];

                while (i < copy.Length && copy[i] == 0) i++;
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
        /// Faster version (pairs' frequencies dictionary is not recreated).
        /// </remarks>
        public static ulong[] PrecompressSequence2(this Links links, ulong[] sequence)
        {
            if (sequence.IsNullOrEmpty())
                return null;

            if (sequence.Length == 1)
                return sequence;

            var newLength = sequence.Length;
            var copy = new ulong[sequence.Length];
            copy[0] = sequence[0];

            var pairsFrequencies = new Dictionary<Link, ulong>();

            var maxPair = Link.Null;
            ulong maxFrequency = 1;

            for (var i = 1; i < sequence.Length; i++)
            {
                copy[i] = sequence[i];

                var pair = new Link(sequence[i - 1], sequence[i]);

                ulong frequency;
                if (pairsFrequencies.TryGetValue(pair, out frequency))
                {
                    var newFrequency = frequency + 1;

                    if (maxFrequency < newFrequency)
                    {
                        maxFrequency = newFrequency;
                        maxPair = pair;
                    }

                    pairsFrequencies[pair] = newFrequency;
                }
                else
                    pairsFrequencies.Add(pair, 1);
            }

            while (!maxPair.IsNull())
            {
                var maxPairSource = maxPair.Source;

                var maxPairLink = links.Create(maxPairSource, maxPair.Target);

                // Substitute all usages
                for (var i = 1; i < copy.Length; i++)
                {
                    var startIndex = i - 1;

                    if (copy[startIndex] == maxPairSource)
                    {
                        while (i < copy.Length && copy[i] == 0) i++;
                        if (i == copy.Length) break;

                        if (copy[i] == maxPair.Target)
                        {
                            var oldLeft = copy[startIndex];
                            var oldRight = copy[i];

                            copy[startIndex] = maxPairLink;
                            copy[i] = 0; // TODO: Вместо записи нулевых дырок, можно хранить отрицательным числом размер диапазона (дырки) на которую надо прыгнуть, это дополнительно ускорило бы алгоритм.

                            // Требуется отдельно, так как пары могут идти подряд,
                            // например в "ааа" пара "аа" была посчитана дважды
                            pairsFrequencies[maxPair]--;

                            newLength--;

                            if (startIndex > 0)
                            {
                                var previous = startIndex - 1;
                                while (previous >= 0 && copy[previous] == 0) previous--;
                                if (previous >= 0)
                                {
                                    ulong frequency;

                                    var nextOldPair = new Link(copy[previous], oldLeft);
                                    //if (!nextOldPair.Equals(maxPair))
                                    {
                                        //pairsFrequencies[nextOldPair]--;
                                        if (pairsFrequencies.TryGetValue(nextOldPair, out frequency))
                                            pairsFrequencies[nextOldPair] = frequency - 1;
                                    }

                                    var nextNewPair = new Link(copy[previous], copy[startIndex]);
                                    //pairsFrequencies[nextNewPair]++;
                                    if (pairsFrequencies.TryGetValue(nextNewPair, out frequency))
                                        pairsFrequencies[nextNewPair] = frequency + 1;
                                    else
                                        pairsFrequencies.Add(nextNewPair, 1);
                                }
                            }

                            if (i < copy.Length)
                            {
                                var next = i;
                                while (next < copy.Length && copy[next] == 0) next++;
                                if (next < copy.Length)
                                {
                                    ulong frequency;

                                    var nextOldPair = new Link(oldRight, copy[next]);
                                    //if (!nextOldPair.Equals(maxPair))
                                    {
                                        //pairsFrequencies[nextOldPair]--;
                                        if (pairsFrequencies.TryGetValue(nextOldPair, out frequency))
                                            pairsFrequencies[nextOldPair] = frequency - 1;
                                    }

                                    var nextNewPair = new Link(copy[startIndex], copy[next]);
                                    //pairsFrequencies[nextNewPair]++;
                                    if (pairsFrequencies.TryGetValue(nextNewPair, out frequency))
                                        pairsFrequencies[nextNewPair] = frequency + 1;
                                    else
                                        pairsFrequencies.Add(nextNewPair, 1);
                                }
                            }
                        }
                    }
                }

                //pairsFrequencies[maxPair] = 0;
                //pairsFrequencies.Remove(maxPair);

                //if (pairsFrequencies[maxPair] > 0)
                //{

                //}

                maxPair = Link.Null;
                maxFrequency = 1;

                foreach (var pairsFrequency in pairsFrequencies)
                {
                    var frequency = pairsFrequency.Value;
                    if (frequency > 1)
                    {
                        var pair = pairsFrequency.Key;

                        if (maxFrequency < frequency)
                        {
                            maxFrequency = frequency;
                            maxPair = pair;
                        }
                        if (maxFrequency == frequency &&
                            (pair.Source + pair.Target) > (maxPair.Source + maxPair.Target))
                        {
                            maxPair = pair;
                        }
                    }
                }

                //{
                //    var pairsFrequenciesCheck = new Dictionary<Link, ulong>();
                //    var maxPairCheck = Link.Null;
                //    ulong maxFrequencyCheck = 1;

                //    for (var i = 1; i < copy.Length; i++)
                //    {
                //        var startIndex = i - 1;

                //        while (i < copy.Length && copy[i] == 0) i++;
                //        if (i == copy.Length) break;

                //        var pair = new Link(copy[startIndex], copy[i]);

                //        ulong frequency;
                //        if (pairsFrequenciesCheck.TryGetValue(pair, out frequency))
                //        {
                //            var newFrequency = frequency + 1;

                //            if (maxFrequencyCheck < newFrequency)
                //            {
                //                maxFrequencyCheck = newFrequency;
                //                maxPairCheck = pair;
                //            }

                //            pairsFrequenciesCheck[pair] = newFrequency;
                //        }
                //        else
                //            pairsFrequenciesCheck.Add(pair, 1);
                //    }

                //    if (!maxPairCheck.Equals(maxPair) || maxFrequency != maxFrequencyCheck)
                //    {

                //    }
                //}
            }

            var final = new ulong[newLength];

            var j = 0;
            for (var i = 1; i < copy.Length; i++)
            {
                final[j++] = copy[i - 1];

                while (i < copy.Length && copy[i] == 0) i++;
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
            private readonly Links _links;
            private readonly Sequences _sequences;
            private Link _maxPair;
            private ulong _maxFrequency;
            private Link _maxPair2;
            private ulong _maxFrequency2;
            private UnsafeDictionary<Link, ulong> _pairsFrequencies;

            public Compressor(Links links, Sequences sequences)
            {
                _links = links;
                _sequences = sequences;
                _maxPair = Link.Null;
                _maxFrequency = 1;
                _maxPair2 = Link.Null;
                _maxFrequency2 = 1;
                _pairsFrequencies = new UnsafeDictionary<Link, ulong>();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private ulong IncrementFrequency(Link pair)
            {
                ulong frequency;
                if (_pairsFrequencies.TryGetValue(pair, out frequency))
                {
                    frequency++;
                    _pairsFrequencies[pair] = frequency;
                }
                else
                {
                    frequency = 1;
                    _pairsFrequencies.Add(pair, frequency);
                }
                return frequency;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void DecrementFrequency(Link pair)
            {
                ulong frequency;
                if (_pairsFrequencies.TryGetValue(pair, out frequency))
                {
                    frequency--;

                    if (frequency == 0)
                        _pairsFrequencies.Remove(pair);
                    else
                        _pairsFrequencies[pair] = frequency;
                }
                //return frequency;
            }

            /// <remarks>
            /// Original algorithm idea: https://en.wikipedia.org/wiki/Byte_pair_encoding .
            /// Faster version (pairs' frequencies dictionary is not recreated).
            /// </remarks>
            public ulong[] Precompress0(ulong[] sequence)
            {
                if (sequence.IsNullOrEmpty())
                    return null;

                if (sequence.Length == 1)
                    return sequence;

                var oldLength = sequence.Length;
                var newLength = sequence.Length;

                // Can be faster if source sequence allowed to be changed
                var copy = new ulong[sequence.Length];
                copy[0] = sequence[0];

                for (var i = 1; i < sequence.Length; i++)
                {
                    copy[i] = sequence[i];

                    var pair = new Link(sequence[i - 1], sequence[i]);
                    UpdateMaxPair(pair, IncrementFrequency(pair));
                }

                while (!_maxPair.IsNull())
                {
                    var maxPairSource = _maxPair.Source;
                    var maxPairTarget = _maxPair.Target;
                    var maxPairResult = _links.Create(maxPairSource, maxPairTarget);

                    oldLength--;
                    var oldLengthMinusTwo = oldLength - 1;

                    // Substitute all usages
                    int w = 0, r = 0; // (r == read, w == write)
                    for (; r < oldLength; r++)
                    {
                        if (copy[r] == maxPairSource && copy[r + 1] == maxPairTarget)
                        {
                            if (r > 0)
                            {
                                var previous = copy[w - 1];
                                DecrementFrequency(new Link(previous, maxPairSource));
                                IncrementFrequency(new Link(previous, maxPairResult));
                            }
                            if (r < oldLengthMinusTwo)
                            {
                                var next = copy[r + 2];
                                DecrementFrequency(new Link(maxPairTarget, next));
                                IncrementFrequency(new Link(maxPairResult, next));
                            }

                            copy[w++] = maxPairResult;
                            r++;
                            newLength--;
                        }
                        else
                        {
                            copy[w++] = copy[r];
                        }
                    }
                    copy[w] = copy[r];

                    _pairsFrequencies.Remove(_maxPair);

                    oldLength = newLength;

                    // Медленный вариант UpdateMaxPair
                    //_maxPair = Link.Null;
                    //_maxFrequency = 1;

                    // TODO: Разобраться почему, если переместить сюда строчку "_pairsFrequencies.Remove(_maxPair);" алгоритм зацикливается

                    //foreach (var pairsFrequency in _pairsFrequencies)
                    //    UpdateMaxPair(pairsFrequency.Key, pairsFrequency.Value);

                    // Быстрее
                    UpdateMaxPair2();
                }

                var final = new ulong[newLength];
                Array.Copy(copy, final, newLength);

                return final;
            }

            /// <remarks>
            /// Original algorithm idea: https://en.wikipedia.org/wiki/Byte_pair_encoding .
            /// Faster version (pairs' frequencies dictionary is not recreated).
            /// </remarks>
            public ulong[] Precompress1(ulong[] sequence)
            {
                if (sequence.IsNullOrEmpty())
                    return null;

                if (sequence.Length == 1)
                    return sequence;

                var newLength = sequence.Length;
                var copy = new ulong[sequence.Length];
                copy[0] = sequence[0];

                for (var i = 1; i < sequence.Length; i++)
                {
                    copy[i] = sequence[i];

                    var pair = new Link(sequence[i - 1], sequence[i]);

                    ulong frequency;
                    if (_pairsFrequencies.TryGetValue(pair, out frequency))
                    {
                        var newFrequency = frequency + 1;

                        if (_maxFrequency < newFrequency)
                        {
                            _maxFrequency = newFrequency;
                            _maxPair = pair;
                        }

                        _pairsFrequencies[pair] = newFrequency;
                    }
                    else
                        _pairsFrequencies.Add(pair, 1);
                }

                while (!_maxPair.IsNull())
                {
                    var maxPair = _maxPair;

                    //ResetMaxPair();

                    var maxPairSource = maxPair.Source;

                    var maxPairLink = _links.Create(maxPairSource, maxPair.Target);

                    // Substitute all usages
                    for (var i = 1; i < copy.Length; i++)
                    {
                        var startIndex = i - 1;

                        if (copy[startIndex] == maxPairSource)
                        {
                            while (i < copy.Length && copy[i] == 0) i++;
                            if (i == copy.Length) break;

                            if (copy[i] == maxPair.Target)
                            {
                                var oldLeft = copy[startIndex];
                                var oldRight = copy[i];

                                copy[startIndex] = maxPairLink;
                                copy[i] = 0; // TODO: Вместо записи нулевых дырок, можно хранить отрицательным числом размер диапазона (дырки) на которую надо прыгнуть, это дополнительно ускорило бы алгоритм.

                                // Требуется отдельно, так как пары могут идти подряд,
                                // например в "ааа" пара "аа" была посчитана дважды
                                //pairsFrequencies[maxPair]--;

                                ulong frequency;
                                //ulong frequency = _pairsFrequencies[maxPair];
                                //if (frequency == 1)
                                //    _pairsFrequencies.Remove(maxPair);
                                //else
                                //    _pairsFrequencies[maxPair] = frequency - 1;

                                //UpdateMaxPair2(maxPair, frequency);

                                newLength--;

                                if (startIndex > 0)
                                {
                                    var previous = startIndex - 1;
                                    while (previous >= 0 && copy[previous] == 0) previous--;
                                    if (previous >= 0)
                                    {
                                        var previousOldPair = new Link(copy[previous], oldLeft);
                                        //if (!nextOldPair.Equals(maxPair))
                                        {
                                            //pairsFrequencies[nextOldPair]--;
                                            if (_pairsFrequencies.TryGetValue(previousOldPair, out frequency))
                                            {
                                                if (frequency == 1)
                                                    _pairsFrequencies.Remove(previousOldPair);
                                                else
                                                    _pairsFrequencies[previousOldPair] = frequency - 1;

                                                //if(!maxPair.Equals(previousOldPair))
                                                //    UpdateMaxPair2(previousOldPair, frequency - 1);
                                            }
                                        }

                                        var previousNewPair = new Link(copy[previous], copy[startIndex]);
                                        //pairsFrequencies[nextNewPair]++;
                                        if (_pairsFrequencies.TryGetValue(previousNewPair, out frequency))
                                        {
                                            _pairsFrequencies[previousNewPair] = frequency + 1;

                                            //UpdateMaxPair(previousNewPair, frequency + 1);
                                        }
                                        else
                                            _pairsFrequencies.Add(previousNewPair, 1);
                                    }
                                }

                                if (i < copy.Length)
                                {
                                    var next = i;
                                    while (next < copy.Length && copy[next] == 0) next++;
                                    if (next < copy.Length)
                                    {
                                        var nextOldPair = new Link(oldRight, copy[next]);
                                        //if (!nextOldPair.Equals(maxPair))
                                        {
                                            //pairsFrequencies[nextOldPair]--;
                                            if (_pairsFrequencies.TryGetValue(nextOldPair, out frequency))
                                            {
                                                if (frequency == 1)
                                                    _pairsFrequencies.Remove(nextOldPair);
                                                else
                                                    _pairsFrequencies[nextOldPair] = frequency - 1;

                                                //if (!maxPair.Equals(nextOldPair))
                                                //    UpdateMaxPair2(nextOldPair, frequency - 1);
                                            }
                                        }

                                        var nextNewPair = new Link(copy[startIndex], copy[next]);
                                        //pairsFrequencies[nextNewPair]++;
                                        if (_pairsFrequencies.TryGetValue(nextNewPair, out frequency))
                                        {
                                            _pairsFrequencies[nextNewPair] = frequency + 1;

                                            //UpdateMaxPair(nextNewPair, frequency + 1);
                                        }
                                        else
                                            _pairsFrequencies.Add(nextNewPair, 1);
                                    }
                                }
                            }
                        }
                    }

                    //////if (!_maxPair2.IsNull())
                    //////{
                    //////    UpdateMaxPair(_maxPair2, _maxFrequency2);
                    //////}

                    //pairsFrequencies[maxPair] = 0;
                    //pairsFrequencies.Remove(maxPair);

                    //if (pairsFrequencies[maxPair] > 0)
                    //{

                    //}

                    _pairsFrequencies.Remove(_maxPair);

                    _maxPair = Link.Null;
                    _maxFrequency = 1;

                    foreach (var pairsFrequency in _pairsFrequencies)
                        UpdateMaxPair(pairsFrequency.Key, pairsFrequency.Value);

                    //_maxPair = Link.Null;
                    //_maxFrequency = 1;

                    //for (var i = 1; i < copy.Length; i++)
                    //{
                    //    var startIndex = i - 1;

                    //    while (i < copy.Length && copy[i] == 0) i++;
                    //    if (i == copy.Length) break;

                    //    var pair = new Link(copy[startIndex], copy[i]);

                    //    var frequency = _pairsFrequencies[pair];
                    //    UpdateMaxPair(pair, frequency);
                    //}

                    //{
                    //    var pairsFrequenciesCheck = new Dictionary<Link, ulong>();
                    //    var maxPairCheck = Link.Null;
                    //    ulong maxFrequencyCheck = 1;

                    //    for (var i = 1; i < copy.Length; i++)
                    //    {
                    //        var startIndex = i - 1;

                    //        while (i < copy.Length && copy[i] == 0) i++;
                    //        if (i == copy.Length) break;

                    //        var pair = new Link(copy[startIndex], copy[i]);

                    //        ulong frequency;
                    //        if (pairsFrequenciesCheck.TryGetValue(pair, out frequency))
                    //        {
                    //            var newFrequency = frequency + 1;

                    //            if (maxFrequencyCheck < newFrequency)
                    //            {
                    //                maxFrequencyCheck = newFrequency;
                    //                maxPairCheck = pair;
                    //            }

                    //            pairsFrequenciesCheck[pair] = newFrequency;
                    //        }
                    //        else
                    //            pairsFrequenciesCheck.Add(pair, 1);
                    //    }

                    //    if (!maxPairCheck.Equals(maxPair) || maxFrequency != maxFrequencyCheck)
                    //    {

                    //    }
                    //}
                }

                var final = new ulong[newLength];

                var j = 0;
                for (var i = 0; i < copy.Length; i++)
                {
                    while (i < copy.Length && copy[i] == 0) i++;
                    if (i == copy.Length) break;

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
            /// Faster version (pairs' frequencies dictionary is not recreated).
            /// </remarks>
            public ulong[] Precompress2(ulong[] sequence)
            {
                if (sequence.IsNullOrEmpty())
                    return null;

                if (sequence.Length == 1)
                    return sequence;

                var newLength = sequence.Length;
                var copy = new ulong[sequence.Length];
                copy[0] = sequence[0];

                for (var i = 1; i < sequence.Length; i++)
                {
                    copy[i] = sequence[i];

                    var pair = new Link(sequence[i - 1], sequence[i]);

                    ulong frequency;
                    if (_pairsFrequencies.TryGetValue(pair, out frequency))
                    {
                        var newFrequency = frequency + 1;

                        if (_maxFrequency < newFrequency)
                        {
                            _maxFrequency = newFrequency;
                            _maxPair = pair;
                        }

                        _pairsFrequencies[pair] = newFrequency;
                    }
                    else
                        _pairsFrequencies.Add(pair, 1);
                }

                //var tempPair = new Link();

                while (!_maxPair.IsNull())
                {
                    var maxPair = _maxPair;

                    ResetMaxPair();

                    var maxPairSource = maxPair.Source;

                    var maxPairLink = _links.Create(maxPairSource, maxPair.Target);

                    // Substitute all usages
                    for (var i = 1; i < copy.Length; i++)
                    {
                        var startIndex = i - 1;

                        while (startIndex < copy.Length && copy[startIndex] == 0)
                        {
                            i++;
                            startIndex++;
                        }
                        if (startIndex == copy.Length - 1) break;

                        if (copy[startIndex] == maxPairSource)
                        {
                            while (i < copy.Length && copy[i] == 0) i++;
                            if (i == copy.Length) break;

                            if (copy[i] == maxPair.Target)
                            {
                                var oldLeft = copy[startIndex];
                                var oldRight = copy[i];

                                copy[startIndex] = maxPairLink;
                                copy[i] = 0;
                                // TODO: Вместо записи нулевых дырок, можно хранить отрицательным числом размер диапазона (дырки) на которую надо прыгнуть, это дополнительно ускорило бы алгоритм.

                                // Требуется отдельно, так как пары могут идти подряд,
                                // например в "ааа" пара "аа" была посчитана дважды
                                //pairsFrequencies[maxPair]--;

                                var frequency = _pairsFrequencies[maxPair];
                                if (frequency == 1)
                                    _pairsFrequencies.Remove(maxPair);
                                else
                                    _pairsFrequencies[maxPair] = frequency - 1;

                                //UpdateMaxPair2(maxPair, frequency);

                                newLength--;

                                if (startIndex > 0)
                                {
                                    var previous = startIndex - 1;
                                    while (previous >= 0 && copy[previous] == 0) previous--;
                                    if (previous >= 0)
                                    {
                                        var previousOldPair = new Link(copy[previous], oldLeft);
                                        //if (!nextOldPair.Equals(maxPair))
                                        {
                                            //pairsFrequencies[nextOldPair]--;
                                            if (_pairsFrequencies.TryGetValue(previousOldPair, out frequency))
                                            {
                                                if (frequency == 1)
                                                    _pairsFrequencies.Remove(previousOldPair);
                                                else
                                                    _pairsFrequencies[previousOldPair] = frequency - 1;

                                                //if(!maxPair.Equals(previousOldPair))
                                                //    UpdateMaxPair2(previousOldPair, frequency - 1);
                                            }
                                        }

                                        var previousNewPair = new Link(copy[previous], copy[startIndex]);
                                        //pairsFrequencies[nextNewPair]++;
                                        if (_pairsFrequencies.TryGetValue(previousNewPair, out frequency))
                                        {
                                            _pairsFrequencies[previousNewPair] = frequency + 1;

                                            //if (!maxPair.Equals(previousNewPair))
                                            UpdateMaxPair(previousNewPair, frequency + 1);
                                        }
                                        else
                                            _pairsFrequencies.Add(previousNewPair, 1);
                                    }
                                }

                                if (i < copy.Length)
                                {
                                    var next = i;
                                    while (next < copy.Length && copy[next] == 0) next++;
                                    if (next < copy.Length)
                                    {
                                        var nextOldPair = new Link(oldRight, copy[next]);
                                        //if (!nextOldPair.Equals(maxPair))
                                        {
                                            //pairsFrequencies[nextOldPair]--;
                                            if (_pairsFrequencies.TryGetValue(nextOldPair, out frequency))
                                            {
                                                if (frequency == 1)
                                                    _pairsFrequencies.Remove(nextOldPair);
                                                else
                                                    _pairsFrequencies[nextOldPair] = frequency - 1;

                                                //if (!maxPair.Equals(nextOldPair))
                                                //    UpdateMaxPair2(nextOldPair, frequency - 1);
                                            }
                                        }

                                        var nextNewPair = new Link(copy[startIndex], copy[next]);
                                        //pairsFrequencies[nextNewPair]++;
                                        if (_pairsFrequencies.TryGetValue(nextNewPair, out frequency))
                                        {
                                            _pairsFrequencies[nextNewPair] = frequency + 1;

                                            //if (!maxPair.Equals(nextNewPair))
                                            UpdateMaxPair(nextNewPair, frequency + 1);
                                        }
                                        else
                                            _pairsFrequencies.Add(nextNewPair, 1);
                                    }
                                }
                            }
                        }
                        else
                        {
                            while (i < copy.Length && copy[i] == 0) i++;
                            if (i == copy.Length) break;

                            //tempPair.Source = copy[startIndex];
                            //tempPair.Target = copy[i];

                            var pair = new Link(copy[startIndex], copy[i]);

                            //if (!maxPair.Equals(pair))
                            //{
                            ulong frequency;
                            if (_pairsFrequencies.TryGetValue(pair, out frequency))
                                UpdateMaxPair(pair, frequency);
                            //}
                        }
                    }

                    //////if (!_maxPair2.IsNull())
                    //////{
                    //////    UpdateMaxPair(_maxPair2, _maxFrequency2);
                    //////}

                    //_maxPair = Link.Null;
                    //_maxFrequency = 1;

                    //foreach (var pairsFrequency in _pairsFrequencies)
                    //    UpdateMaxPair(pairsFrequency.Key, pairsFrequency.Value);
                }

                var final = new ulong[newLength];

                var j = 0;
                for (var i = 0; i < copy.Length; i++)
                {
                    while (i < copy.Length && copy[i] == 0) i++;
                    if (i == copy.Length) break;

                    final[j++] = copy[i];
                }

                return final;
            }

            /// <remarks>
            /// Original algorithm idea: https://en.wikipedia.org/wiki/Byte_pair_encoding .
            /// If pair repeats twice it is maximum pair.
            /// </remarks>
            public ulong[] Precompress3(ulong[] sequence)
            {
                if (sequence.IsNullOrEmpty())
                    return null;

                if (sequence.Length == 1)
                    return sequence;

                var oldLength = sequence.Length;
                var newLength = sequence.Length;

                // Can be faster if source sequence allowed to be changed
                var copy = new ulong[sequence.Length];
                Array.Copy(sequence, copy, copy.Length);

                var set = new HashSet<Link>();

                for (var i = 1; i < sequence.Length; i++)
                {
                    var pair = new Link(sequence[i - 1], sequence[i]);

                    //UpdateMaxPair(pair, IncrementFrequency(pair));
                    //if(_maxFrequency >= 2)
                    //    break;

                    if (!set.Add(pair))
                    {
                        _maxPair = pair;
                        //_maxFrequency = 2;
                        break;
                    }
                }

                while (!_maxPair.IsNull())
                {
                    var maxPairSource = _maxPair.Source;
                    var maxPairTarget = _maxPair.Target;
                    var maxPairResult = _links.Create(maxPairSource, maxPairTarget);

                    oldLength--;
                    //var oldLengthMinusTwo = oldLength - 1;

                    // Substitute all usages
                    int w = 0, r = 0; // (r == read, w == write)
                    for (; r < oldLength; r++)
                    {
                        if (copy[r] == maxPairSource && copy[r + 1] == maxPairTarget)
                        {
                            //if (r > 0)
                            //{
                            //    var previous = copy[w - 1];
                            //    DecrementFrequency(new Link(previous, maxPairSource));
                            //    IncrementFrequency(new Link(previous, maxPairResult));
                            //}
                            //if (r < oldLengthMinusTwo)
                            //{
                            //    var next = copy[r + 2];
                            //    DecrementFrequency(new Link(maxPairTarget, next));
                            //    IncrementFrequency(new Link(maxPairResult, next));
                            //}

                            copy[w++] = maxPairResult;
                            r++;
                            newLength--;
                        }
                        else
                        {
                            copy[w++] = copy[r];
                        }
                    }
                    copy[w] = copy[r];

                    //_pairsFrequencies.Remove(_maxPair);

                    _maxPair = Link.Null;

                    //ResetMaxPair();
                    set.Clear();
                    //_pairsFrequencies = new UnsafeDictionary<Link, ulong>();

                    oldLength = newLength;

                    for (var i = 1; i < newLength; i++)
                    {
                        var pair = new Link(copy[i - 1], copy[i]);

                        //UpdateMaxPair(pair, IncrementFrequency(pair));
                        //if (_maxFrequency >= 2)
                        //    break;

                        if (!set.Add(pair))
                        {
                            _maxPair = pair;
                            //_maxFrequency = 2;
                            break;;
                        }
                    }

                    // Медленный вариант UpdateMaxPair
                    //_maxPair = Link.Null;
                    //_maxFrequency = 1;

                    // TODO: Разобраться почему, если переместить сюда строчку "_pairsFrequencies.Remove(_maxPair);" алгоритм зацикливается

                    //foreach (var pairsFrequency in _pairsFrequencies)
                    //    UpdateMaxPair(pairsFrequency.Key, pairsFrequency.Value);

                    // Быстрее
                    //UpdateMaxPair2();
                }

                var final = new ulong[newLength];
                Array.Copy(copy, final, newLength);

                return final;
            }

            /// <remarks>
            /// Original algorithm idea: https://en.wikipedia.org/wiki/Byte_pair_encoding .
            /// If pair repeats twice it is maximum pair.
            /// </remarks>
            public ulong[] Precompress4(ulong[] sequence)
            {
                if (sequence.IsNullOrEmpty())
                    return null;

                if (sequence.Length == 1)
                    return sequence;

                var oldLength = sequence.Length;
                var newLength = sequence.Length;

                // Can be faster if source sequence allowed to be changed
                var copy = new ulong[sequence.Length];
                Array.Copy(sequence, copy, copy.Length);

                var set = new HashSet<Link>();

                for (var i = 1; i < sequence.Length; i++)
                {
                    var pair = new Link(sequence[i - 1], sequence[i]);

                    //UpdateMaxPair(pair, IncrementFrequency(pair));
                    //if(_maxFrequency >= 2)
                    //    break;

                    if (!set.Add(pair))
                    {
                        _maxPair = pair;
                        //_maxFrequency = 2;
                        break;
                    }
                }

                while (!_maxPair.IsNull())
                {
                    var maxPairSource = _maxPair.Source;
                    var maxPairTarget = _maxPair.Target;
                    var maxPairResult = _links.Create(maxPairSource, maxPairTarget);

                    oldLength--;
                    var oldLengthMinusTwo = oldLength - 1;

                    _maxPair = Link.Null;
                    set.Clear();

                    // Substitute all usages
                    int w = 0, r = 0; // (r == read, w == write)
                    for (; r < oldLength; r++)
                    {
                        if (copy[r] == maxPairSource && copy[r + 1] == maxPairTarget)
                        {
                            //if (_maxPair.IsNull())
                            //{
                            //    if (r > 0)
                            //    {
                            //        var previous = copy[w - 1];
                            //        set.Remove(new Link(previous, maxPairSource));
                            //        var pair = new Link(previous, maxPairResult);
                            //        if (!set.Add(pair)) _maxPair = pair;
                            //        //DecrementFrequency(new Link(previous, maxPairSource));
                            //        //IncrementFrequency(new Link(previous, maxPairResult));
                            //    }
                            //    if (r < oldLengthMinusTwo)
                            //    {
                            //        var next = copy[r + 2];
                            //        set.Remove(new Link(maxPairTarget, next));
                            //        var pair = new Link(maxPairResult, next);
                            //        if (!set.Add(pair)) _maxPair = pair;
                            //        //DecrementFrequency(new Link(maxPairTarget, next));
                            //        //IncrementFrequency(new Link(maxPairResult, next));
                            //    }
                            //}

                            copy[w++] = maxPairResult;
                            r++;
                            newLength--;
                        }
                        else
                        {
                            //if (_maxPair.IsNull() && w > 0) // 8 sec
                            //{
                            //    var pair = new Link(copy[w - 1], copy[w]);
                            //    if (!set.Add(pair)) _maxPair = pair;
                            //}

                            if (_maxPair.IsNull()) // 4 sec
                            {
                                var pair = new Link(copy[r], copy[r + 1]);
                                if (!set.Add(pair)) _maxPair = pair;
                            }
                            copy[w++] = copy[r];

                            //if (_maxPair.IsNull()) // 8 sec
                            //{
                            //    var pair = new Link(copy[w - 1], copy[w]);
                            //    if (!set.Add(pair)) _maxPair = pair;
                            //}
                        }
                    }
                    //if (_maxPair.IsNull()) // 8 sec
                    //{
                    //    var pair = new Link(copy[w - 1], copy[w]);
                    //    if (!set.Add(pair)) _maxPair = pair;
                    //}
                    copy[w] = copy[r];

                    //_pairsFrequencies.Remove(_maxPair);

                    //_maxPair = Link.Null;
                    //set.Clear();

                    oldLength = newLength;

                    //for (var i = 1; i < newLength; i++)
                    //{
                    //    var pair = new Link(copy[i - 1], copy[i]);

                    //    //UpdateMaxPair(pair, IncrementFrequency(pair));
                    //    //if (_maxFrequency >= 2)
                    //    //    break;

                    //    if (!set.Add(pair))
                    //    {
                    //        _maxPair = pair;
                    //        //_maxFrequency = 2;
                    //        break; ;
                    //    }
                    //}

                    // Медленный вариант UpdateMaxPair
                    //_maxPair = Link.Null;
                    //_maxFrequency = 1;

                    // TODO: Разобраться почему, если переместить сюда строчку "_pairsFrequencies.Remove(_maxPair);" алгоритм зацикливается

                    //foreach (var pairsFrequency in _pairsFrequencies)
                    //    UpdateMaxPair(pairsFrequency.Key, pairsFrequency.Value);

                    // Быстрее
                    //UpdateMaxPair2();
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
                    return null;

                if (sequence.Length == 1)
                    return sequence;

                var oldLength = sequence.Length;
                var newLength = sequence.Length;

                // Can be faster if source sequence allowed to be changed
                var copy = new ulong[sequence.Length];
                copy[0] = sequence[0];

                for (var i = 1; i < sequence.Length; i++)
                {
                    copy[i] = sequence[i];

                    var pair = new Link(sequence[i - 1], sequence[i]);
                    UpdateMaxPair(pair, IncrementFrequency(pair));
                }

                while (!_maxPair.IsNull())
                {
                    var maxPairSource = _maxPair.Source;
                    var maxPairTarget = _maxPair.Target;
                    var maxPairResult = _links.Create(maxPairSource, maxPairTarget);

                    oldLength--;
                    //var oldLengthMinusTwo = oldLength - 1;

                    // Substitute all usages
                    int w = 0, r = 0; // (r == read, w == write)
                    for (; r < oldLength; r++)
                    {
                        if (copy[r] == maxPairSource && copy[r + 1] == maxPairTarget)
                        {
                            //if (r > 0)
                            //{
                            //    var previous = copy[w - 1];
                            //    DecrementFrequency(new Link(previous, maxPairSource));
                            //    IncrementFrequency(new Link(previous, maxPairResult));
                            //}
                            //if (r < oldLengthMinusTwo)
                            //{
                            //    var next = copy[r + 2];
                            //    DecrementFrequency(new Link(maxPairTarget, next));
                            //    IncrementFrequency(new Link(maxPairResult, next));
                            //}

                            copy[w++] = maxPairResult;
                            r++;
                            newLength--;
                        }
                        else
                        {
                            copy[w++] = copy[r];
                        }
                    }
                    copy[w] = copy[r];

                    _pairsFrequencies.Remove(_maxPair);

                    oldLength = newLength;

                    // Медленный вариант UpdateMaxPair
                    //_maxPair = Link.Null;
                    //_maxFrequency = 1;

                    // TODO: Разобраться почему, если переместить сюда строчку "_pairsFrequencies.Remove(_maxPair);" алгоритм зацикливается

                    //foreach (var pairsFrequency in _pairsFrequencies)
                    //    UpdateMaxPair(pairsFrequency.Key, pairsFrequency.Value);

                    // Быстрее
                    UpdateMaxPair2();
                }

                var final = new ulong[newLength];
                Array.Copy(copy, final, newLength);

                return final;
            }

            public ulong Compress(ulong[] sequence)
            {
                var precompressedSequence = Precompress1(sequence);
                return _sequences.CreateBalancedVariant(precompressedSequence);
            }

            private void ResetMaxPair()
            {
                _maxPair = Link.Null;
                _maxFrequency = 1;
                _maxPair2 = Link.Null;
                _maxFrequency2 = 1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void UpdateMaxPair(Link pair, ulong frequency)
            {
                if (frequency > 1)
                {
                    if (_maxFrequency < frequency)
                    {
                        _maxFrequency = frequency;
                        _maxPair = pair;
                    }
                    else if (_maxFrequency == frequency &&
                        (pair.Source + pair.Target) > (_maxPair.Source + _maxPair.Target))
                    {
                        _maxPair = pair;
                    }
                }
            }

            private void UpdateMaxPair2(Link pair, ulong frequency)
            {
                if (!_maxPair.Equals(pair))
                {
                    if (_maxPair2.Equals(pair))
                    {
                        _maxFrequency2 = frequency;
                    }
                    else if (_maxFrequency2 < frequency)
                    {
                        _maxFrequency2 = frequency;
                        _maxPair2 = pair;
                    }
                    else if (_maxFrequency2 == frequency &&
                             (pair.Source + pair.Target) > (_maxPair2.Source + _maxPair2.Target))
                    {
                        _maxPair = pair;
                    }
                }
            }

            private void UpdateMaxPair()
            {
                ResetMaxPair();

                var entries = _pairsFrequencies.entries;
                for (var i = 0; i < entries.Length; i++)
                {
                    if (entries[i].hashCode >= 0)
                    {
                        if (entries[i].value > 1)
                        {
                            if (_maxFrequency < entries[i].value)
                            {
                                _maxFrequency = entries[i].value;
                                _maxPair = entries[i].key;
                            }
                            else if (_maxFrequency == entries[i].value &&
                                (entries[i].key.Source + entries[i].key.Target) > (_maxPair.Source + _maxPair.Target))
                            {
                                _maxPair = entries[i].key;
                            }
                        }
                    }
                }
            }

            private void UpdateMaxPair2()
            {
                ResetMaxPair();

                var entries = _pairsFrequencies.entries;
                for (var i = 0; i < entries.Length; i++)
                {
                    if (entries[i].hashCode >= 0)
                    {
                        var frequency = entries[i].value;
                        if (frequency > 1)
                        {
                            if (_maxFrequency > frequency)
                                continue;

                            if (_maxFrequency < frequency)
                            {
                                _maxFrequency = frequency;
                                _maxPair = entries[i].key;
                            }
                            else if (_maxFrequency == frequency &&
                                (entries[i].key.Source + entries[i].key.Target) > (_maxPair.Source + _maxPair.Target))
                            {
                                _maxPair = entries[i].key;
                            }
                        }
                    }
                }
            }
        }

        public static ulong CreateBalancedVariant(this Sequences sequences, List<ulong[]> groupedSequence)
        {
            var finalSequence = new ulong[groupedSequence.Count];

            for (var i = 0; i < finalSequence.Length; i++)
            {
                var part = groupedSequence[i];
                finalSequence[i] = part.Length == 1 ? part[0] : sequences.CreateBalancedVariant(part);
            }

            return sequences.CreateBalancedVariant(finalSequence);
        }
    }
}