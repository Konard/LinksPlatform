/*

using System;
using System.Collections.Generic;
using Abot.Crawler;
using Abot.Poco;
using Abot.Util;
using AbotX.Crawler;
using AbotX.Parallel;
using AbotX.Poco;
using log4net.Config;

namespace Platform.Sandbox
{
    class WebCrawler
    {
        static public void TestEngine()
        {
            var config = new CrawlConfigurationX
            {
                MaxConcurrentSiteCrawls = 3,
                SitesToCrawlBatchSizePerRequest = 25,
                MinSiteToCrawlRequestDelayInSecs = 15,
                IsJavascriptRenderingEnabled = false,
                JavascriptRenderingWaitTimeInMilliseconds = 3500
            };

            var siteToCrawlProvider = new SiteToCrawlProvider();
            siteToCrawlProvider.AddSitesToCrawl(new List<SiteToCrawl>
            {
                new SiteToCrawl{ Uri = new Uri("http://somesitetocrawl1.com/") },
                new SiteToCrawl{ Uri = new Uri("http://somesitetocrawl2.com/") },
                new SiteToCrawl{ Uri = new Uri("http://somesitetocrawl3.com/") },
            });


            //Now you must pass it into the constructor of CrawlerX or ParallelCrawlerEngine var crawler = new CrawlerX(config);
            var crawlerEngine = new ParallelCrawlerEngine(config, new WebCrawlerFactory(), new RateLimiter(1, TimeSpan.FromTicks(200)), siteToCrawlProvider);

            crawlerEngine.Start();


            //crawler.PageCrawlStartingAsync += crawler_ProcessPageCrawlStarting;
            //crawler.PageCrawlCompletedAsync += crawler_ProcessPageCrawlCompleted;
            //crawler.PageCrawlDisallowedAsync += crawler_PageCrawlDisallowed;
            //crawler.PageLinksCrawlDisallowedAsync += crawler_PageLinksCrawlDisallowed;


            //Register for site level events
            crawlerEngine.AllCrawlsCompleted += (sender, eventArgs) =>
            {
                Console.WriteLine("Completed crawling all sites");
            };
            crawlerEngine.SiteCrawlCompleted += (sender, eventArgs) =>
            {
                Console.WriteLine("Completed crawling site {0}", eventArgs.CrawledSite.SiteToCrawl.Uri);
            };
            crawlerEngine.CrawlerInstanceCreated += (sender, eventArgs) =>
            {
                //Register for crawler level events. These are Abot's events!!!
                eventArgs.Crawler.PageCrawlCompleted += (abotSender, abotEventArgs) =>
                {
                    Console.WriteLine("You have the crawled page here in abotEventArgs.CrawledPage...");
                };
            };

            crawlerEngine.Start();//Asynchronously start the crawl

            Console.WriteLine("Press enter key to stop");
            Console.Read();
        }

        static public void TestSingle()
        {
            var config = new CrawlConfigurationX
            {
                MaxConcurrentSiteCrawls = 3,
                SitesToCrawlBatchSizePerRequest = 25,
                MinSiteToCrawlRequestDelayInSecs = 15,
                IsJavascriptRenderingEnabled = false,
                JavascriptRenderingWaitTimeInMilliseconds = 3500
            };

            var crawler = new CrawlerX(config);

            crawler.PageCrawlStartingAsync += crawler_ProcessPageCrawlStarting;
            crawler.PageCrawlCompletedAsync += crawler_ProcessPageCrawlCompleted;
            crawler.PageCrawlDisallowedAsync += crawler_PageCrawlDisallowed;
            crawler.PageLinksCrawlDisallowedAsync += crawler_PageLinksCrawlDisallowed;

            crawler.CrawlAsync(new Uri("https://abotx.org/"));


            Console.WriteLine("Press enter key to stop");
            Console.Read();
        }

        private static void crawler_PageLinksCrawlDisallowed(object sender, PageLinksCrawlDisallowedArgs e)
        {
            Console.WriteLine("crawler_PageLinksCrawlDisallowed");
        }

        private static void crawler_PageCrawlDisallowed(object sender, PageCrawlDisallowedArgs e)
        {
            Console.WriteLine("crawler_PageCrawlDisallowed");
        }

        private static void crawler_ProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            var str = e.CrawledPage.Content.Text;

            Console.WriteLine("crawler_ProcessPageCrawlCompleted");
        }

        private static void crawler_ProcessPageCrawlStarting(object sender, PageCrawlStartingArgs e)
        {
            var str = e.PageToCrawl.PageBag.ToJSON();

            Console.WriteLine("crawler_ProcessPageCrawlStarting");
        }

        static public void TestSingle1()
        {

            XmlConfigurator.Configure();

            //var config = new CrawlConfigurationX
            //{
            //    MaxConcurrentSiteCrawls = 3,
            //    SitesToCrawlBatchSizePerRequest = 25,
            //    MinSiteToCrawlRequestDelayInSecs = 15,
            //    IsJavascriptRenderingEnabled = false,
            //    JavascriptRenderingWaitTimeInMilliseconds = 3500
            //};

            var crawlConfig = new CrawlConfiguration();
            crawlConfig.CrawlTimeoutSeconds = 100;
            crawlConfig.MaxConcurrentThreads = 10;
            crawlConfig.MaxPagesToCrawl = 1000;
            crawlConfig.UserAgentString = "abot v1.0 http://code.google.com/p/abot";
            crawlConfig.ConfigurationExtensions.Add("SomeCustomConfigValue1", "1111");
            crawlConfig.ConfigurationExtensions.Add("SomeCustomConfigValue2", "2222");

            var crawler = new PoliteWebCrawler();

            crawler.PageCrawlStartingAsync += crawler_ProcessPageCrawlStarting;
            crawler.PageCrawlCompletedAsync += crawler_ProcessPageCrawlCompleted;
            crawler.PageCrawlDisallowedAsync += crawler_PageCrawlDisallowed;
            crawler.PageLinksCrawlDisallowedAsync += crawler_PageLinksCrawlDisallowed;

            //crawler.CrawlAsync(new Uri("https://abotx.org/"));

            crawler.Crawl(new Uri("https://abotx.org/"));

            Console.WriteLine("Press enter key to stop");
            Console.Read();
        }

    }
} */
