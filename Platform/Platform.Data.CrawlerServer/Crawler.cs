using System;
using System.Linq;
using System.Threading;
using Abot.Crawler;
using Abot.Poco;
using AbotX.Crawler;
using Platform.Data.Core.Pairs;
using Platform.Data.Core.Sequences;
using Platform.Helpers.Disposal;
using Platform.Helpers.Threading;

namespace Platform.Data.CrawlerServer
{
    internal class Crawler : DisposalBase
    {
        private readonly LinksMemoryManager _memoryManager;
        private readonly Links _links;
        private readonly Sequences _sequences;
        private readonly ulong _pageMarker;

        public Crawler(string databasePath)
        {
            _memoryManager = new LinksMemoryManager(databasePath, 256 * 1024 * 1024);
            _links = new Links(_memoryManager);

            UnicodeMap.InitNew(_links);

            var options = new SequencesOptions { UseCompression = true };

            _sequences = new Sequences(_links, options);

            _pageMarker = _links.Create(LinksConstants.Itself, LinksConstants.Itself);


            //var sw3 = Stopwatch.StartNew();
            //var compressor = new Data.Core.Sequences.Compressor(links, sequences, minFrequency);
            //ulong[] responseCompressedArray3 = compressor.Precompress(responseSourceArray);
            //sequences.CreateBalancedVariant(responseCompressedArray3); sw3.Stop();

            //var totalLinks = links.Count() - UnicodeMap.MapSize;

            //Console.WriteLine("{0}, {1}, {2}, {3}", sw3.Elapsed, minFrequency, responseSourceArray.Length, totalLinks);
        }

        public Crawler(Links links, Sequences sequences, ulong pageMarker)
        {
            _links = links;
            _sequences = sequences;
            _pageMarker = pageMarker;
        }

        public void Start(Uri uri)
        {
            var crawler = new PoliteWebCrawler();

            crawler.PageCrawlCompletedAsync += crawler_ProcessPageCrawlCompleted;

            //crawler.PageCrawlStarting += crawler_PageCrawlStarting;

            crawler.ShouldCrawlPage(DecisionMaker);

            crawler.Crawl(uri);
        }

        private CrawlDecision DecisionMaker(PageToCrawl pageToCrawl, CrawlContext crawlContext)
        {
            if (!Program.LinksServerRunning)
            {
                //crawlContext.IsCrawlStopRequested = true;
                return new CrawlDecision { ShouldStopCrawl = true, Reason = "Сервер остановлен." };
            }

            var url = pageToCrawl.Uri.ToString();
            var urlSequence = _sequences.Create(UnicodeMap.FromStringToLinkArray(url));

            var maxTimestamp = default(DateTime);

            _links.Each(urlSequence, LinksConstants.Any, contentAtDate =>
            {
                var timestampSequence = _links.GetSource(_links.GetTarget(contentAtDate));

                var timestampString = UnicodeMap.FromSequenceLinkToString(timestampSequence, _links);

                DateTime timestamp;
                if (DateTime.TryParse(timestampString, out timestamp) && maxTimestamp < timestamp)
                    maxTimestamp = timestamp;

                return true;
            });

            var now = DateTime.UtcNow;

            if ((now - maxTimestamp) < TimeSpan.FromDays(1))
                return new CrawlDecision { Allow = false, Reason = "Страница уже запрашивалась в течение 24 часов." };

            return new CrawlDecision { Allow = true };
        }

        //void crawler_PageCrawlStarting(object sender, PageCrawlStartingArgs e)
        //{
        //    if (!Program.Threads.Contains(Thread.CurrentThread))
        //        Program.Threads.Add(Thread.CurrentThread);

        //    if (!Program.LinksServerRunning)
        //    {
        //        e.CrawlContext.IsCrawlStopRequested = true;
        //        return;
        //    }

        //    var url = e.PageToCrawl.Uri.ToString();
        //    var urlSequence = _sequences.Create(UnicodeMap.FromStringToLinkArray(url));

        //    var maxTimestamp = default(DateTime);

        //    _links.Each(urlSequence, LinksConstants.Any, contentAtDate =>
        //    {
        //        var timestampSequence = _links.GetSource(_links.GetTarget(contentAtDate));

        //        var timestampString = UnicodeMap.FromSequenceLinkToString(timestampSequence, _links);

        //        DateTime timestamp;
        //        if (DateTime.TryParse(timestampString, out timestamp) && maxTimestamp < timestamp)
        //            maxTimestamp = timestamp;

        //        return true;
        //    });

        //    var now = DateTime.UtcNow;

        //    if ((now - maxTimestamp) < TimeSpan.FromDays(1))
        //        e.CrawlContext.CancellationTokenSource.Cancel();
        //}

        private void crawler_ProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            if (!Program.Threads.Contains(Thread.CurrentThread))
                Program.Threads.Add(Thread.CurrentThread);

            if (!Program.LinksServerRunning)
            {
                e.CrawlContext.IsCrawlStopRequested = true;
                return;
            }

            var url = e.CrawledPage.Uri.ToString();
            var content = e.CrawledPage.Content.Text;
            var timestamp = DateTime.UtcNow.ToString("u");

            var urlSequence = _sequences.Create(UnicodeMap.FromStringToLinkArray(url));
            var contentSequence = _sequences.Create(UnicodeMap.FromStringToLinkArray(content));
            var timestampSequence = _sequences.Create(UnicodeMap.FromStringToLinkArray(timestamp));

            _links.Create(_pageMarker, _links.Create(urlSequence, _links.Create(timestampSequence, contentSequence)));
        }

        protected override void DisposeCore(bool manual)
        {
            _links.Dispose();
            _memoryManager.Dispose();
        }
    }
}
