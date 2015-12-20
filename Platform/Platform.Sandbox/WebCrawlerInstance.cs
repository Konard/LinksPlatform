/* using System;
using Abot.Crawler;
using log4net.Config;
using Platform.Data.Core.Pairs;
using Platform.Data.Core.Sequences;
using Platform.Helpers.Disposal;

namespace Platform.Sandbox
{
    public class WebCrawlerInstance : DisposalBase
    {
        private readonly LinksMemoryManager _memoryManager;
        private readonly Links _links;
        private readonly Sequences _sequences;

        public WebCrawlerInstance(string databasePath)
        {
            _memoryManager = new LinksMemoryManager(databasePath, 256 * 1024 * 1024);
            _links = new Links(_memoryManager);
            UnicodeMap.InitNew(_links);

            var options = new SequencesOptions { UseCompression = true };

            _sequences = new Sequences(_links, options);

            //var sw3 = Stopwatch.StartNew();
            //var compressor = new Data.Core.Sequences.Compressor(links, sequences, minFrequency);
            //ulong[] responseCompressedArray3 = compressor.Precompress(responseSourceArray);
            //sequences.CreateBalancedVariant(responseCompressedArray3); sw3.Stop();

            //var totalLinks = links.Count() - UnicodeMap.MapSize;

            //Console.WriteLine("{0}, {1}, {2}, {3}", sw3.Elapsed, minFrequency, responseSourceArray.Length, totalLinks);
        }


        public void Start(Uri uri)
        {
            XmlConfigurator.Configure();

            var crawler = new PoliteWebCrawler();

            crawler.PageCrawlCompletedAsync += crawler_ProcessPageCrawlCompleted;

            crawler.Crawl(uri);
        }


        private void crawler_ProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            var url = e.CrawledPage.Uri.ToString();
            var content = e.CrawledPage.Content.Text;

            var urlSequence = _sequences.Create(UnicodeMap.FromStringToLinkArray(url));
            var contentSequence = _sequences.Create(UnicodeMap.FromStringToLinkArray(content));

            _links.Create(urlSequence, contentSequence);
        }

        protected override void DisposeCore(bool manual)
        {
            _links.Dispose();
            _memoryManager.Dispose();
        }
    }
}*/
