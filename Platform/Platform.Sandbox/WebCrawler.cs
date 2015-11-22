using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using Platform.Data.Core.Pairs;
using Platform.Data.Core.Sequences;
using Platform.Data.Core.Structures;
using Platform.Helpers.Collections;

namespace Platform.Sandbox
{
    public static class WebCrawler
    {
        public static void Test()
        {
            File.Delete("web.links");

            using (var memoryManager = new LinksMemoryManager("web.links", 8 * 1024 * 1024))
            using (var links = new Links(memoryManager))
            {
                var utfMap = new UTF16Map(links);
                utfMap.Init();

                var sequences = new Sequences(links);

                using (var client = new HttpClient())
                {
                    var url = "https://en.wikipedia.org/wiki/Main_Page";
                    //var response = client.GetStringAsync(url).AwaitResult();
                    var response = File.ReadAllText("response.html");

                    var totalChars = url.Length + response.Length;

                    var urlLink = sequences.CreateBalancedVariant(UTF16Map.FromStringToLinkArray(url));

                    //var responseLink = sequences.CreateBalancedVariant(UTF16Map.FromStringToLinkArray(response));

                    //var sw0 = Stopwatch.StartNew();
                    //var groups = UTF16Map.FromStringToLinkArrayGroups(response);
                    //var responseLink = sequences.CreateBalancedVariant(groups); sw0.Stop();

              
                    var responseSourceArray = UTF16Map.FromStringToLinkArray(response);

                    //var sw1 = Stopwatch.StartNew();
                    //var responseCompressedArray1 = links.PrecompressSequence1(responseSourceArray); sw1.Stop();

                    var sw2 = Stopwatch.StartNew();
                    var responseCompressedArray2 = links.PrecompressSequence2(responseSourceArray); sw2.Stop();

                    //for (int i = 0; i < responseCompressedArray1.Length; i++)
                    //{
                    //    if (responseCompressedArray1[i] != responseCompressedArray2[i])
                    //    {
                            
                    //    }
                    //}

                    //var responseLink1 = sequences.CreateBalancedVariant(responseCompressedArray1);
                    var responseLink2 = sequences.CreateBalancedVariant(responseCompressedArray2);

                    //var decompress1 = sequences.FormatSequence(responseLink1);
                    //var decompress2 = sequences.FormatSequence(responseLink2);

                    //for (int i = 0; i < decompress1.Length; i++)
                    //{
                    //    if (decompress1[i] != decompress2[i])
                    //    {

                    //    }
                    //}

                    // TODO: Combine Groups and Compression

                    var totalLinks = links.Total - UTF16Map.MapSize;

                    links.Create(urlLink, responseLink2);

                    var divLinksArray = UTF16Map.FromStringToLinkArray("div");

                    var fullyMatched = sequences.GetAllMatchingSequences1(divLinksArray);
                    var partiallyMatched = sequences.GetAllPartiallyMatchingSequences1(divLinksArray);

                    // TODO: Реализовать распаковку в Unicode строку для сравнения
                }
            }
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
                    var pair = pairsFrequency.Key;
                    var frequency = pairsFrequency.Value;

                    if (maxFrequency < frequency)
                    {
                        maxFrequency = frequency;
                        maxPair = pair;
                    }
                    if (frequency > 1 && maxFrequency == frequency &&
                        (pair.Source + pair.Target) > (maxPair.Source + maxPair.Target))
                    {
                        maxPair = pair;
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

        public static ulong CreateBalancedVariant(this Sequences sequences, List<ulong[]> groupedSequence)
        {
            var finalSequence = new ulong[groupedSequence.Count];

            for (int i = 0; i < finalSequence.Length; i++)
            {
                var part = groupedSequence[i];
                finalSequence[i] = part.Length == 1 ? part[0] : sequences.CreateBalancedVariant(part);
            }

            return sequences.CreateBalancedVariant(finalSequence);
        }
    }
}
