using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NetLibrary;
using Utils;

namespace WebTester.Models
{
    public class LinkModel
    {
        public Link Link { get; set; }
        public List<LinkModel> ReferersModels { get; set; }

        public LinkModel(Link link)
            : this(link, new List<LinkModel>())
        {
        }

        public LinkModel(Link link, List<LinkModel> referersModels)
        {
            this.Link = link;
            this.ReferersModels = referersModels;
        }

        static public LinkModel CreateLinkModel(Link link, int nestingLevel = 3)
        {
            LinkModel result = null;

            //ThreadHelpers.SyncInvokeWithExtendedStack(() =>
            //{
                int currentLevel = 0;
                HashSet<Link> visitedLinks = new HashSet<Link>();
                result = CreateLinkModel(link, visitedLinks, currentLevel, nestingLevel);
            //});

            return result;
        }

        static private LinkModel CreateLinkModel(Link link, HashSet<Link> visitedLinks, int currentLevel, int maxLevel)
        {
            LinkModel model = new LinkModel(link);

            if (currentLevel < maxLevel)
            {
                if (currentLevel == 0 || link.TotalReferers < 10)
                {
                    currentLevel++;

                    link.WalkThroughReferers((referer) =>
                    {
                        if (link != referer && visitedLinks.Add(referer))
                        {
                            model.ReferersModels.Add(CreateLinkModel(referer, visitedLinks, currentLevel, maxLevel));
                        }
                    });

                    model.ReferersModels.Sort((x, y) => x.Link.GetPointer().ToInt64().CompareTo(y.Link.GetPointer().ToInt64()));
                }
            }

            return model;
        }
    }
}