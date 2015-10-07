using System.Collections.Generic;
using Platform.Links.DataBase.CoreNet.Triplets;

namespace Platform.Links.DataBase.WebTerminal.Models
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
            Link = link;
            ReferersModels = referersModels;
        }

        public static LinkModel CreateLinkModel(Link link, int nestingLevel = 5)
        {
            //ThreadHelpers.SyncInvokeWithExtendedStack(() =>
            //{
            const int currentLevel = 0;
            var visitedLinks = new HashSet<Link>();
            LinkModel result = CreateLinkModel(link, visitedLinks, currentLevel, nestingLevel);
            //});

            return result;
        }

        private static LinkModel CreateLinkModel(Link link, HashSet<Link> visitedLinks, int currentLevel, int maxLevel)
        {
            var model = new LinkModel(link);

            if (currentLevel < maxLevel)
            {
                if (currentLevel == 0 || link.TotalReferers < 10)
                {
                    currentLevel++;

                    link.WalkThroughReferers(referer =>
                    {
                        if (link != referer && visitedLinks.Add(referer))
                        {
                            model.ReferersModels.Add(CreateLinkModel(referer, visitedLinks, currentLevel, maxLevel));
                        }
                    });

                    model.ReferersModels.Sort((x, y) => x.Link.ToInt().CompareTo(y.Link.ToInt()));
                }
            }

            return model;
        }
    }
}