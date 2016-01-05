using System;
using System.Linq;
using System.Threading;
using Abot.Crawler;
using Abot.Poco;
using Platform.Data.Core.Pairs;
using Platform.Data.Core.Sequences;

namespace Platform.Data.CrawlerServer
{
    internal class Crawler
    {
        private readonly Links _links;
        private readonly Sequences _sequences;
        private readonly ulong _pageMarker;

        public Crawler(Links links, Sequences sequences, ulong pageMarker)
        {
            _links = links;
            _sequences = sequences;
            _pageMarker = pageMarker;
        }

        public void Start(Uri uri)
        {
            var crawler = new PoliteWebCrawler();

            crawler.PageCrawlCompleted += crawler_ProcessPageCrawlCompleted;

            crawler.ShouldCrawlPage(DecisionMaker);

            crawler.Crawl(uri);
        }

        private CrawlDecision DecisionMaker(PageToCrawl pageToCrawl, CrawlContext crawlContext)
        {
            if (!Program.LinksServerRunning)
                return new CrawlDecision { ShouldStopCrawl = true, Reason = "Сервер остановлен." };

            var maxTimestamp = GetUriLastCrawledTimestamp(pageToCrawl.Uri);

            if ((DateTime.UtcNow - maxTimestamp) < TimeSpan.FromDays(1))
                return new CrawlDecision { Allow = false, Reason = "Страница уже запрашивалась в течение 24 часов." };

            return new CrawlDecision { Allow = true };
        }

        private void crawler_ProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            if (!Program.LinksServerRunning)
                e.CrawlContext.IsCrawlStopRequested = true;
            else
                StoreCrawledPage(e.CrawledPage);
        }

        private DateTime GetUriLastCrawledTimestamp(Uri uri)
        {
            var url = uri.ToString();
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

            return maxTimestamp;
        }

        private void StoreCrawledPage(CrawledPage page)
        {
            var url = page.Uri.ToString();
            var content = page.Content.Text;
            var timestamp = DateTime.UtcNow.ToString("u");

            var urlSequence = _sequences.Create(UnicodeMap.FromStringToLinkArray(url));
            var contentSequence = _sequences.Create(UnicodeMap.FromStringToLinkArray(content));
            var timestampSequence = _sequences.Create(UnicodeMap.FromStringToLinkArray(timestamp));

            _links.Create(_pageMarker, _links.Create(urlSequence, _links.Create(timestampSequence, contentSequence)));
        }
    }
}
