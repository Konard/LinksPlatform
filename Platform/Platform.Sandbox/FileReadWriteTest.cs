﻿using System;
using System.Globalization;
using System.IO.MemoryMappedFiles;
using Platform.IO;
using Platform.Data.Triplets;

namespace Platform.Sandbox
{
    public static class FileReadWriteTest
    {
        public static void MappedFilesTest()
        {
            //Thread t = new Thread();

            //t.ThreadState == ThreadState.


            var f = MemoryMappedFile.CreateFromFile(@"C:\file.txt");

            var a = f.CreateViewAccessor();


            //    a.Read<


            //MemoryMappedFileSecurity s = new MemoryMappedFileSecurity();


            //f.SetAccessControl(
        }

        public static void Run(string[] args)
        {
            string readableFilename;

            do
            {
                //Net.Recreate();

                readableFilename = ConsoleHelpers.GetOrReadArgument(0, "File to read", args);

                if (!string.IsNullOrWhiteSpace(readableFilename))
                {
                    //Net.Recreate();

                    //int countersCyclesToEquality = 0;

                    //while (true)
                    //{
                    var linksBefore = Net.And.ReferersByLinkerCount;
                    //long charsLinksBefore = Net.Character.ReferersBySourceCount;

                    var totalBytesRead = 0;

                    var chars = FileHelpers.ReadAllChars(readableFilename);

                    SmartTextParsing(chars, 0, chars.Length);

                    //string xmlSnapshotFilename = Path.Combine(Path.GetDirectoryName(readableFilename), Path.GetFileNameWithoutExtension(readableFilename) + "." + countersCyclesToEquality + ".gexf");
                    //XmlGenerator.ToFile(xmlSnapshotFilename, link =>
                    //{
                    //    return (link.ReferersByLinkerCount < (link.ReferersBySourceCount + link.ReferersByTargetCount)) && ((link.ReferersBySourceCount + link.ReferersByTargetCount + link.ReferersByLinkerCount) >= 0);
                    //});

                    totalBytesRead += chars.Length;

                    var linksAfter = Net.And.ReferersByLinkerCount - linksBefore;
                    //long charsLinksAfter = Net.Character.ReferersBySourceCount - charsLinksBefore;

                    Console.Write(totalBytesRead);
                    Console.Write(" ~ ");
                    Console.WriteLine(linksAfter);

                    //if (linksAfter == 0)
                    //{
                    //    break;
                    //}
                    //   countersCyclesToEquality++;
                    //}

                    //Console.WriteLine("Total cycles: {0}.", countersCyclesToEquality);

                    //Console.WriteLine("Total 'and' links used: {0}", linksAfter);
                    //Console.WriteLine("Total 'char' links used: {0}", charsLinksAfter);
                }

            } while (!string.IsNullOrWhiteSpace(readableFilename));

            Console.ReadLine();

            //Console.WriteLine("Xml export started.");

            //readableFilename = @"C:\Texts\result.txt";

            //var xmlFilename = Path.Combine(Path.GetDirectoryName(readableFilename), Path.GetFileNameWithoutExtension(readableFilename) + ".gexf");

            //GexfExporter.ToFile(xmlFilename, link =>
            //{
            //    return (link.ReferersByLinkerCount < (link.ReferersBySourceCount + link.ReferersByTargetCount)) && ((link.ReferersBySourceCount + link.ReferersByTargetCount + link.ReferersByLinkerCount) >= 5);
            //});

            //Console.WriteLine("Xml export finished");

            Console.ReadLine();
        }

        public static Link SmartTextParsing(char[] text, int takeFrom, int takeUntil)
        {
            // Переменная result должна быть со ссылкой от другой связи, чтобы защитить её от удаления,
            // иначе произойдёт исключение

            Link result = null;
            Link group = null;

            var holder = Net.CreateThing();
            var holderDoublet = holder & holder;


            UnicodeCategory? currentUnicodeCategory = null;
            var currentUnicodeCategoryStartIndex = takeFrom;

            for (var i = takeFrom; i < takeUntil; i++)
            {
                var c = text[i];

                var charCategory = CharUnicodeInfo.GetUnicodeCategory(c);

                if (charCategory != currentUnicodeCategory)
                {
                    if (currentUnicodeCategory != null)
                    {
                        group = CreateCharactersGroup(text, currentUnicodeCategoryStartIndex, i);

                        result = result == null ? group : result & group; // LinkConverter.CombinedJoin(ref result, ref group);

                        Link.Update(ref holderDoublet, holder, Net.And, result);
                    }

                    currentUnicodeCategory = charCategory;
                    currentUnicodeCategoryStartIndex = i;
                }
            }

            group = CreateCharactersGroup(text, currentUnicodeCategoryStartIndex, takeUntil);
            result = result == null ? group : result & group; // LinkConverter.CombinedJoin(ref result, ref group);

            return result;
        }

        private static Link CreateCharactersGroup(char[] text, int takeFrom, int takeUntil)
        {
            if (takeFrom < (takeUntil - 1))
            {
                return Link.Create(Net.Group, Net.ThatConsistsOf, LinkConverter.FromChars(text, takeFrom, takeUntil));
            }
            else
            {
                return LinkConverter.FromChar(text[takeFrom]);
            }
        }
    }
}
