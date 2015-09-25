namespace Platform.Sandbox
{
    class OperationsExperimentsTrash
    {
        private static void Trash()
        {
            //    Link linkDescription = selectionSample.Target;
            //    Link linkDescriptionParamsSequence = linkDescription.Target;

            //    List<Link> linkDescriptionParams = LinkConverter.ToList(linkDescriptionParamsSequence);

            //    //List<Link> result = new List<Link>();

            //    HashSet<Link> specificLinks = new HashSet<Link>();
            //    CollectAllSpecificLinks(linkDescription, ref specificLinks);

            //    int checksCount = 0;

            //    List<Link> result = new List<Link>();

            //    if (Net.Name.ReferersBySourceCount < Net.ThatIsRepresentedBy.ReferersByLinkerCount)
            //    {
            //        Net.Name.WalkThroughReferersBySource(referer =>
            //        {
            //            checksCount++;
            //            if (referer.Linker == Net.ThatIsRepresentedBy)
            //            {
            //                if (referer.Target.Source == Net.String)
            //                {
            //                    result.Add(referer);
            //                }
            //            }
            //        });
            //    }
            //    else if (true)
            //    {
            //        Net.ThatIsRepresentedBy.WalkThroughReferersByLinker(referer =>
            //        {
            //            checksCount++;
            //            if (referer.Source == Net.Name)
            //            {
            //                if (referer.Target.Source == Net.String)
            //                {
            //                    result.Add(referer);
            //                }
            //            }
            //        });
            //    }

            //    // Наивный вариант обратного сбора даёт 80*80 проходов в худшем случае (непредсказуемое поведение) N*N в худшем случае


            //    Net.String.WalkThroughReferersBySource(stringReferer =>
            //    {
            //        stringReferer.WalkThroughReferersByTarget(stringRefererReferer =>
            //        {
            //            checksCount++;
            //            if (stringRefererReferer.Source == Net.Name && stringRefererReferer.Linker == Net.ThatIsRepresentedBy)
            //            {
            //                result.Add(stringRefererReferer);
            //            }
            //        });
            //    });


            //    // Комбинированный вариант без вложенных циклов (2*80) 2*N в худшем случае

            //    HashSet<Link> secondLevelMatches = new HashSet<Link>();

            //    // Самая не удобная ситуация, когда на одном (втором уровне может быть несколько сборов)
            //    // а хуже всего, когда все три ссылки первого уровня не специфичны, но и из этого варианта есть спасение
            //    // счётчик общего числа связей, которые может предстоять пройти
            //    long targetsTotalReferers = 0;

            //    Net.String.WalkThroughReferersBySource(stringReferer =>
            //    {
            //        targetsTotalReferers += stringReferer.ReferersByTargetCount;

            //        if (targetsTotalReferers > Net.Name.ReferersBySourceCount)
            //        {
            //            // Если количество проверок, необходимых для выполенния привысит,
            //            // того что можно было сделать на первом уровне без этого прохода

            //            return false; // Завершаем сбор, его результаты нам больше не пригодятся
            //        }

            //        secondLevelMatches.Add(stringReferer);

            //        return true;
            //    });

            //    HashSet<Link> firstLevelMatches = new HashSet<Link>();

            //    if (targetsTotalReferers < Net.Name.ReferersBySourceCount)
            //    {
            //        foreach (var secondLevelMatch in secondLevelMatches)
            //        {
            //            secondLevelMatch.WalkThroughReferersByTarget(referer =>
            //            {
            //                if (referer.Source == Net.Name && referer.Linker == Net.ThatIsRepresentedBy)
            //                {
            //                    firstLevelMatches.Add(referer);
            //                }
            //            });
            //        }
            //    }
            //    else
            //    {
            //        Net.Name.WalkThroughReferersBySource(nameReferer =>
            //        {
            //            if (nameReferer.Linker == Net.ThatIsRepresentedBy)
            //            {
            //                if (secondLevelMatches.Contains(nameReferer.Target))
            //                {
            //                    firstLevelMatches.Add(nameReferer);
            //                }
            //            }
            //        });
            //    }




            //    //Func<Link, Link> createCreateOperation = operationDescription =>
            //    //    {

            //    //    };


            //    HashSet<Link> r = ExecuteLinksSelection(selectionSample);


            //}

            //private static void IfTreeExample()
            //{
            //    Link selectionSample = Link.Create(_selection, Net.Of,
            //        Link.Create(_links, _with, Link.Create(Net.Name, _as, _sourceLink) & Link.Create(Net.ThatIsRepresentedBy, _as, _linkerLink)
            //                                     & Link.Create(Link.Create(Net.Link, _with, Link.Create(Net.String, _as, _sourceLink)), _as, _targetLink)));


            //    var x = new
            //    {
            //        FirstLevel = new
            //        {
            //            SourceReferers = Net.Name.ReferersBySourceCount,
            //            LinkerReferers = Net.ThatIsRepresentedBy.ReferersByLinkerCount,
            //        },
            //        SecondLevel = new
            //        {
            //            SourceReferers = Net.String.ReferersBySourceCount
            //        }
            //    };


            //    if (x.FirstLevel.SourceReferers < x.FirstLevel.LinkerReferers)
            //    {
            //        if (x.FirstLevel.SourceReferers < x.SecondLevel.SourceReferers)
            //        {

            //        }
            //        else
            //        {
            //            // SecondLevel is less

            //        }
            //    }
            //    else
            //    {
            //        // LinkerReferers is less

            //        if (x.FirstLevel.LinkerReferers < x.SecondLevel.SourceReferers)
            //        {

            //        }
            //        else
            //        {
            //            // SecondLevel is less

            //        }
            //    }
            //}

            //enum LinkPart
            //{
            //    Source,
            //    Linker,
            //    Target
            //}

            //private static void NaiveIteration()
            //{
            //    Link selectionSample = Link.Create(_selection, Net.Of,
            //        Link.Create(_links, _with, Link.Create(Net.Name, _as, _sourceLink) & Link.Create(Net.ThatIsRepresentedBy, _as, _linkerLink)
            //                                     & Link.Create(Link.Create(Net.Link, _with, Link.Create(Net.String, _as, _sourceLink)), _as, _targetLink)));


            //    /*

            //    Link definition = GetLinksDescriptionDefinition(selectionSample);

            //    Link sourceLink, linkerLink, targetLink;
            //    GetLinks(definition, out sourceLink, out linkerLink, out targetLink);

            //    HashSet<Link> result = new HashSet<Link>();

            //    HashSet<Link>[] possibleResults = new HashSet<Link>[3];
            //    int possibleResultsCount = 0;

            //    definition.WalkThroughSequence(param =>
            //        {
            //            if (IsLinkDescriptionParamSourceSpecific(param.Source))
            //            {
            //                HashSet<Link> referers = new HashSet<Link>();
            //                CollectParamLinkReferers(param, referers);
            //                possibleResults[possibleResultsCount++] = referers;
            //            }
            //            else
            //            {
            //                HashSet<Link> possibleValues = 
            //            }
            //        });


            //    if (bestSpecificLink != null)
            //    {
            //        CollectParamLinkReferers(bestSpecificLink, result);

            //        for (int i = 0; i < nonSpecificLinksCount; i++)
            //        {

            //        }
            //    }
            //    else
            //    {
            //        for (int i = 0; i < nonSpecificLinksCount; i++)
            //        {
            //            var x = nonSpecificLinks[i];
            //        }
            //    }
            //    */


            //    //HashSet<Link> thisLevelLinks = new HashSet<Link>();
            //    //HashSet<Link> linksFromSecondLevelOfTarget = new HashSet<Link>();

            //    //thisLevelLinks.RemoveWhere(x => !linksFromSecondLevelOfTarget.Contains(x.Target));


            //    // Изначально мержинг можно попробовать сделать только на хешах
            //    // Затем если будет медленно, можно будет попробовать сделать склеивание, основывающиеся на факте, что список ссылающихся ссылок отсортирован по возврастанию адреса

            //}
        }
    }
}
