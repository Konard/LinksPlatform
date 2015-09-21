namespace ConsoleTester
{

#if IncludeTextSearchTest


    public class TextSearchTest
    {
        public static void Run()
        {
            List<Artist> artists = new List<Artist>();

            for (int i = 0; i < 5; i++)
            {
                // После конвертации в связь, исполнитель уже
                // находится в сети, и нет необходимости вести какой либо список

                var artist = GenerateArtist();
                Link artistLink = artist.ToLink();
            }

            Console.Write("Generated tracks: ");
            Console.WriteLine(Artist.Link.ReferersBySourceCount);

            var sw = Stopwatch.StartNew();

            var result = new List<Artist>();

            for (int i = 0; i < 100; i++)
            {
                result = Search("asa ka", true);
            }

            sw.Stop();

            Console.Write("Found: ");
            Console.Write(result.Count);
            Console.Write(", search took ");
            Console.WriteLine(sw.Elapsed);

            sw = Stopwatch.StartNew();

            for (int i = 0; i < 100; i++)
            {
                result = Search(artists, "as xs ya ka", true);
            }

            sw.Stop();

            Console.Write("Found: ");
            Console.Write(result.Count);
            Console.Write(", search took ");
            Console.WriteLine(sw.Elapsed);

            Console.ReadLine();
        }

        public class Artist
        {
            public static readonly Link Link;

            static Artist()
            {
                Link = Link.Create(Link.Itself, Net.IsA, Net.Thing);
                Link.SetName("artist");
            }

            public string Name { get; set; }
            public Track[] Tracks { get; set; }

            public Link ToLink()
            {
                // Artist who has (name "..." and (track ... and (track ... )))
                return Link.Create(Artist.Link, Net.WhoHas,
                    Link.Create(    
                        Link.Create(Net.Name, Net.ConsistsOf, (Link)this.Name),
                        Net.And,
                        (Link)this.Tracks.Select(x => x.ToLink()).ToList())); // Будет работать только есть как минимум один трек.
            }

            public static Artist FromLink(Link link)
            {
                if (IsArtistLink(link))
                {
                    var artist = new Artist();

                    var name = link.Target.Source;
                    var tracks = link.Target.Target;

                    artist.Name = LinkConverter.ToString(name.Target);
                    artist.Tracks = LinkConverter.ToList(link).Select(x => Track.FromLink(x)).ToArray();

                    return artist;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Переданная связь не представляет собой исполнителя.");
                }
            }

            public static bool IsArtistLink(Link link)
            {
                return link.Source == Artist.Link
                    && link.Linker == Net.WhoHas
                    && link.Target.Linker == Net.And;
            }
        }

        public class Track
        {
            public static readonly Link Link;

            static Track()
            {
                Track.Link = Link.Create(Link.Itself, Net.IsA, Net.Thing);
                Track.Link.SetName("track");
            }

            public string Name { get; set; }

            public Link ToLink()
            {


                // Track which has (name "...")
                return Link.Create(Track.Link, Net.WhichHas,
                    Link.Create(Net.Name, Net.ConsistsOf, (Link)this.Name));
            }

            public static Track FromLink(Link link)
            {
                if (IsTrackLink(link))
                {
                    var track = new Track();

                    var name = link.Target;

                    track.Name = LinkConverter.ToString(name.Target);

                    return track;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Переданная связь не представляет собой композицию.");
                }
            }

            public static bool IsTrackLink(Link link)
            {
                return link.Source == Track.Link
                    && link.Linker == Net.WhichHas;
            }
        }

        private static Random r = new Random();

        private static Artist GenerateArtist()
        {
            var artist = new Artist();

            int tracksNumber = r.Next(1, 4);

            artist.Name = GenerateString();
            artist.Tracks = new Track[tracksNumber];

            var tracks = artist.Tracks;

            for (int i = 0; i < tracksNumber; i++)
            {
                tracks[i] = new Track() { Name = GenerateString() };
            }

            return artist;
        }

        private static string GenerateString()
        {
            StringBuilder sb = new StringBuilder();

            int words = r.Next(1, 4);

            for (int i = 0; i < words; i++)
            {
                if (i > 0)
                {
                    sb.Append(' ');
                }
                sb.Append(GenerateWord());
            }

            return sb.ToString();
        }

        private static string GenerateWord()
        {
            StringBuilder sb = new StringBuilder();

            int length = r.Next(1, 6);

            for (int i = 0; i < length; i++)
            {
                char c = (char)r.Next('a', 'z' + 1);

                sb.Append(c);
            }

            return sb.ToString();
        }

        public static readonly int[] PowersOfTwo = CreatePowersOfTwo();

        public static int[] CreatePowersOfTwo()
        {
            int[] result = new int[32];
            for (int i = 0; i < 32; i++)
                result[i] = (int)Math.Pow(2, i);
            return result;
        }


        public static List<Artist> Search(string request, bool useArtistName)
        {
            string[] words = request.Split(' ');

            Array.Sort(words, (x, y) => -x.Length.CompareTo(y.Length));

            List<Artist> result = new List<Artist>();

            List<Link> substrings = new List<Link>();

            foreach (var word in words)
            {
                List<Link> wordSubstrings = GetSubstringsWith(word);

                if (wordSubstrings.Count == 0)
                {
                    return result;
                }

                substrings.AddRange(wordSubstrings);
            }

            foreach (var substring in substrings)
            {
                var strings = GetStringsWithSubstring(substring);
                var firstString = strings.First();
                var artists = GetArtistsWithString(firstString);
            }

            //if (listOfSubstringsLists.Count == words.Length)
            //{
            //    //Array.Sort(words, (x, y) => -x.Length.CompareTo(y.Length));

            //    result.Add(new Artist() { Name = listOfSubstringsLists.Count.ToString() });
            //}

            return result;
        }

        private static List<Link> GetStringsWithSubstring(Link substring)
        {
            List<Link> result = new List<Link>();
            substring.WalkThroughReferersByTarget(referer => StringsBySubstringCollector(referer, result));
            return result;
        }

        private static void StringsBySubstringCollector(Link currentLink, List<Link> collection)
        {
            if (currentLink.IsString())
            {
                collection.Add(currentLink);
            }
            else if (currentLink.Linker == Net.And)
            {
                currentLink.WalkThroughReferersByTarget(referer => StringsBySubstringCollector(referer, collection));
                currentLink.WalkThroughReferersByTarget(referer => StringsBySubstringCollector(referer, collection));
            }
        }

        private static List<Link> GetArtistsWithString(Link @string)
        {
            HashSet<Link> result = new HashSet<Link>();
            @string.WalkThroughReferersByTarget(referer => ArtistsByStringCollector(referer, result));
            return result.ToList();
        }

        // TODO: Доделать функцию поиска артиста по строке названия
        private static void ArtistsByStringCollector(Link currentLink, HashSet<Link> storage)
        {
            if (Artist.IsArtistLink(currentLink))
            {
                storage.Add(currentLink);
            }
            else if (Track.IsTrackLink(currentLink))
            {
                currentLink.WalkThroughReferersByTarget(referer => ArtistsByStringCollector(referer, storage));
                currentLink.WalkThroughReferersBySource(referer => ArtistsByStringCollector(referer, storage));
            }
            else if (currentLink.Linker == Net.And)
            {
                currentLink.WalkThroughReferersByTarget(referer => ArtistsByStringCollector(referer, storage));
                currentLink.WalkThroughReferersByTarget(referer => ArtistsByStringCollector(referer, storage));
            }
        }

        private static List<Link> GetSubstringsWith(string word)
        {
            List<Link> charLinks = word.Select(x => LinkConverter.FromChar(x)).ToList();

            return SequenceHelpers.CollectMatchingSequences(charLinks);
        }

        public static List<Artist> Search(List<Artist> source, string request, bool useArtistName)
        {
            string[] words = request.Split(' ');

            int mask = PowersOfTwo[words.Length] - 1;

            List<Artist> result = new List<Artist>();

            for (int i = source.Count - 1; i >= 0; i--)
            {
                var artist = source[i];

                if (MatchArtist(artist, words, mask, useArtistName))
                {
                    result.Add(artist);
                }
            }

            return result;
        }

        public static bool MatchArtist(Artist artist, string[] words, int mask, bool useArtistName)
        {
            int matchedWords = 0;

            if (useArtistName)
            {
                MatchWords(ref matchedWords, artist.Name, words, mask);

                if (matchedWords == mask)
                    return true;
            }

            foreach (var track in artist.Tracks) // Fastest, yet simple for arrays
            {
                MatchWords(ref matchedWords, track.Name, words, mask);

                if (matchedWords == mask)
                    return true;
            }

            return false;
        }

        public static void MatchWords(ref int matchedWords, string str, string[] words, int mask)
        {
            for (int i = words.Length - 1; i >= 0; i--)
            {
                if (str.IndexOf(words[i], StringComparison.OrdinalIgnoreCase) >= 0) // 10 times faster in .net 3.5 then 4
                {
                    matchedWords |= PowersOfTwo[i];
                }
            }
        }
    }
#endif

}
