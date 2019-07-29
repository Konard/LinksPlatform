using System.Collections.Generic;

namespace Platform.Data.Triplets.Sequences
{
    internal class CompressionExperiments
    {
        public static void RightJoin(ref Link subject, Link @object)
        {
            if (subject.Linker == Net.And && subject.ReferersBySourceCount == 0 && subject.ReferersByTargetCount == 0)
            {
                var subJoint = Link.Search(subject.Target, Net.And, @object);
                if (subJoint != null && subJoint != subject)
                {
                    Link.Update(ref subject, subject.Source, Net.And, subJoint);
                    return;
                }
            }
            subject = Link.Create(subject, Net.And, @object);
        }

        //public static Link RightJoinUnsafe(Link subject, Link @object)
        //{
        //    if (subject.Linker == Net.And && subject.ReferersBySourceCount == 0 && subject.ReferersByTargetCount == 0)
        //    {
        //        Link subJoint = Link.Search(subject.Target, Net.And, @object);
        //        if (subJoint != null && subJoint != subject)
        //        {
        //            Link.Update(ref subject, subject.Source, Net.And, subJoint);
        //            return subject;
        //        }
        //    }
        //    return Link.Create(subject, Net.And, @object);
        //}

        ////public static void LeftJoin(ref Link subject, Link @object)
        ////{
        ////    if (subject.Linker == Net.And && subject.ReferersBySourceCount == 0 && subject.ReferersByTargetCount == 0)
        ////    {
        ////        Link subJoint = Link.Search(@object, Net.And, subject.Source);
        ////        if (subJoint != null && subJoint != subject)
        ////        {
        ////            Link.Update(ref subject, subJoint, Net.And, subject.Target);
        ////            return;
        ////        }
        ////    }
        ////    subject = Link.Create(@object, Net.And, subject);
        ////}

        public static void LeftJoin(ref Link subject, Link @object)
        {
            if (subject.Linker == Net.And && subject.ReferersBySourceCount == 0 && subject.ReferersByTargetCount == 0)
            {
                var subJoint = Link.Search(@object, Net.And, subject.Source);
                if (subJoint != null && subJoint != subject)
                {
                    Link.Update(ref subject, subJoint, Net.And, subject.Target);
                    //var prev = Link.Search(@object, Net.And, subject);
                    //if (prev != null)
                    //{
                    //    Link.Update(ref prev, subJoint, Net.And, subject.Target);
                    //}
                    return;
                }
            }
            subject = Link.Create(@object, Net.And, subject);
        }

        // Сначала сжатие налево, а затем направо (так эффективнее)
        // Не приятный момент, что обе связи, и первая и вторая могут быть изменены в результате алгоритма.
        //public static Link CombinedJoin(ref Link first, ref Link second)
        //{
        //    Link atomicConnection = Link.Search(first, Net.And, second);
        //    if (atomicConnection != null)
        //    {
        //        return atomicConnection;
        //    }
        //    else
        //    {
        //        if (second.Linker == Net.And)
        //        {
        //            Link subJoint = Link.Search(first, Net.And, second.Source);
        //            if (subJoint != null && subJoint != second)// && subJoint.TotalReferers > second.TotalReferers)
        //            {
        //                //if (first.Linker == Net.And)
        //                //{
        //                //    // TODO: ...
        //                //}
        //                if (second.TotalReferers > 0)
        //                {
        //                    // В данный момент это никак не влияет, из-за того что добавлено условие по требованию
        //                    // использования атомарного соедининеия если оно есть

        //                    // В целом же приоритет между обходным соединением и атомарным нужно определять по весу.
        //                    // И если в сети обнаружено сразу два варианта прохода - простой и обходной - нужно перебрасывать
        //                    // пути с меньшим весом на использование путей с большим весом. (Это и технически эффективнее и более оправдано
        //                    // с точки зрения смысла).

        //                    // Положительный эффект текущей реализации, что она быстро "успокаивается" набирает критическую массу
        //                    // и перестаёт вести себя не предсказуемо

        //                    // Неприятность учёта веса в том, что нужно обрабатывать большое количество комбинаций.
        //                    // Но вероятно это оправдано.

        //                    //var prev = Link.Search(first, Net.And, second);
        //                    //if (prev != null && subJoint != prev) // && prev.TotalReferers < subJoint.TotalReferers)
        //                    //{
        //                    //    Link.Update(ref prev, subJoint, Net.And, second.Target);
        //                    //    if (second.TotalReferers == 0)
        //                    //    {
        //                    //        Link.Delete(ref second);
        //                    //    }
        //                    //    return prev;
        //                    //}
        //                    //return Link.Create(subJoint, Net.And, second.Target);
        //                }
        //                else
        //                {
        //                    Link.Update(ref second, subJoint, Net.And, second.Target);
        //                    return second;
        //                }
        //            }
        //        }
        //        if (first.Linker == Net.And)
        //        {
        //            Link subJoint = Link.Search(first.Target, Net.And, second);
        //            if (subJoint != null && subJoint != first)// && subJoint.TotalReferers > first.TotalReferers)
        //            {
        //                if (first.TotalReferers > 0)
        //                {
        //                    //var prev = Link.Search(first, Net.And, second);
        //                    //if (prev != null && subJoint != prev) // && prev.TotalReferers < subJoint.TotalReferers)
        //                    //{
        //                    //    Link.Update(ref prev, first.Source, Net.And, subJoint);
        //                    //    if (first.TotalReferers == 0)
        //                    //    {
        //                    //        Link.Delete(ref first);
        //                    //    }
        //                    //    return prev;
        //                    //}
        //                    //return Link.Create(first.Source, Net.And, subJoint);
        //                }
        //                else
        //                {
        //                    Link.Update(ref first, first.Source, Net.And, subJoint);
        //                    return first;
        //                }
        //            }
        //        }
        //        return Link.Create(first, Net.And, second);
        //    }
        //}

        public static int CompressionsCount;

        public static Link CombinedJoin(ref Link first, ref Link second)
        {
            // Перестроение работает хорошо только когда одна из связей является парой и аккумулятором одновременно
            // Когда обе связи - пары - нужно использовать другой алгоритм, иначе сжатие будет отсутствовать.
            //if ((first.Linker == Net.And && second.Linker != Net.And)
            // || (second.Linker == Net.And && first.Linker != Net.And))
            //{
            //    Link connection = TryReconstructConnection(first, second);
            //    if (connection != null)
            //    {
            //        CompressionsCount++;
            //        return connection;
            //    }
            //}
            //return first & second;
            //long totalDoublets = Net.And.ReferersByLinkerCount;
            if (first == null || second == null)
            {
            }
            var directConnection = Link.Search(first, Net.And, second);
            if (directConnection == null)
            {
                directConnection = TryReconstructConnection(first, second);
            }
            Link rightCrossConnection = null;
            if (second.Linker == Net.And)
            {
                var assumedRightCrossConnection = Link.Search(first, Net.And, second.Source);
                if (assumedRightCrossConnection != null && second != assumedRightCrossConnection)
                {
                    rightCrossConnection = assumedRightCrossConnection;
                }
                else
                {
                    rightCrossConnection = TryReconstructConnection(first, second.Source);
                }
            }
            Link leftCrossConnection = null;
            if (first.Linker == Net.And)
            {
                var assumedLeftCrossConnection = Link.Search(first.Target, Net.And, second);
                if (assumedLeftCrossConnection != null && first != assumedLeftCrossConnection)
                {
                    leftCrossConnection = assumedLeftCrossConnection;
                }
                else
                {
                    leftCrossConnection = TryReconstructConnection(first.Target, second);
                }
            }
            // Наверное имеет смысл только в "безвыходной" ситуации
            //if (directConnection == null && rightCrossConnection == null && leftCrossConnection == null)
            //{
            //    directConnection = TryReconstructConnection(first, second);
            //    // Может давать более агрессивное сжатие, но теряется стабильность
            //    //if (directConnection == null)
            //    //{
            //    //    //if (second.Linker == Net.And)
            //    //    //{
            //    //    //    Link assumedRightCrossConnection = TryReconstructConnection(first, second.Source);
            //    //    //    if (assumedRightCrossConnection != null && second != assumedRightCrossConnection)
            //    //    //    {
            //    //    //        rightCrossConnection = assumedRightCrossConnection;
            //    //    //    }
            //    //    //}
            //    //    //if (rightCrossConnection == null)
            //    //    //{
            //    //    //if (first.Linker == Net.And)
            //    //    //{
            //    //    //    Link assumedLeftCrossConnection = TryReconstructConnection(first.Target, second);
            //    //    //    if (assumedLeftCrossConnection != null && first != assumedLeftCrossConnection)
            //    //    //    {
            //    //    //        leftCrossConnection = assumedLeftCrossConnection;
            //    //    //    }
            //    //    //}
            //    //    //}
            //    //}
            //}
            //Link middleCrossConnection = null;
            //if (second.Linker == Net.And && first.Linker == Net.And)
            //{
            //    Link assumedMiddleCrossConnection = Link.Search(first.Target, Net.And, second.Source);
            //    if (assumedMiddleCrossConnection != null && first != assumedMiddleCrossConnection && second != assumedMiddleCrossConnection)
            //    {
            //        middleCrossConnection = assumedMiddleCrossConnection;
            //    }
            //}
            //Link rightMiddleCrossConnectinon = null;
            //if (middleCrossConnection != null)
            //{
            //}
            if (directConnection != null
            && (rightCrossConnection == null || directConnection.TotalReferers >= rightCrossConnection.TotalReferers)
            && (leftCrossConnection == null || directConnection.TotalReferers >= leftCrossConnection.TotalReferers))
            {
                if (rightCrossConnection != null)
                {
                    var prev = Link.Search(rightCrossConnection, Net.And, second.Target);
                    if (prev != null && directConnection != prev)
                    {
                        Link.Update(ref prev, first, Net.And, second);
                    }
                    if (rightCrossConnection.TotalReferers == 0)
                    {
                        Link.Delete(ref rightCrossConnection);
                    }
                }
                if (leftCrossConnection != null)
                {
                    var prev = Link.Search(first.Source, Net.And, leftCrossConnection);
                    if (prev != null && directConnection != prev)
                    {
                        Link.Update(ref prev, first, Net.And, second);
                    }
                    if (leftCrossConnection.TotalReferers == 0)
                    {
                        Link.Delete(ref leftCrossConnection);
                    }
                }
                TryReconstructConnection(first, second);
                return directConnection;
            }
            else if (rightCrossConnection != null
                 && (directConnection == null || rightCrossConnection.TotalReferers >= directConnection.TotalReferers)
                 && (leftCrossConnection == null || rightCrossConnection.TotalReferers >= leftCrossConnection.TotalReferers))
            {
                if (directConnection != null)
                {
                    var prev = Link.Search(first, Net.And, second);
                    if (prev != null && rightCrossConnection != prev)
                    {
                        Link.Update(ref prev, rightCrossConnection, Net.And, second.Target);
                    }
                }
                if (leftCrossConnection != null)
                {
                    var prev = Link.Search(first.Source, Net.And, leftCrossConnection);
                    if (prev != null && rightCrossConnection != prev)
                    {
                        Link.Update(ref prev, rightCrossConnection, Net.And, second.Target);
                    }
                }
                //TryReconstructConnection(first, second.Source);
                //TryReconstructConnection(rightCrossConnection, second.Target); // ухудшает стабильность
                var resultConnection = rightCrossConnection & second.Target;
                //if (second.TotalReferers == 0)
                //    Link.Delete(ref second);
                return resultConnection;
            }
            else if (leftCrossConnection != null
                 && (directConnection == null || leftCrossConnection.TotalReferers >= directConnection.TotalReferers)
                 && (rightCrossConnection == null || leftCrossConnection.TotalReferers >= rightCrossConnection.TotalReferers))
            {
                if (directConnection != null)
                {
                    var prev = Link.Search(first, Net.And, second);
                    if (prev != null && leftCrossConnection != prev)
                    {
                        Link.Update(ref prev, first.Source, Net.And, leftCrossConnection);
                    }
                }
                if (rightCrossConnection != null)
                {
                    var prev = Link.Search(rightCrossConnection, Net.And, second.Target);
                    if (prev != null && rightCrossConnection != prev)
                    {
                        Link.Update(ref prev, first.Source, Net.And, leftCrossConnection);
                    }
                }
                //TryReconstructConnection(first.Target, second);
                //TryReconstructConnection(first.Source, leftCrossConnection); // ухудшает стабильность
                var resultConnection = first.Source & leftCrossConnection;
                //if (first.TotalReferers == 0)
                //    Link.Delete(ref first);
                return resultConnection;
            }
            else
            {
                if (directConnection != null)
                {
                    return directConnection;
                }
                if (rightCrossConnection != null)
                {
                    return rightCrossConnection & second.Target;
                }
                if (leftCrossConnection != null)
                {
                    return first.Source & leftCrossConnection;
                }
            }
            // Можно фиксировать по окончанию каждой из веток, какой эффект от неё происходит (на сколько уменьшается/увеличивается количество связей)
            directConnection = first & second;
            //long difference = Net.And.ReferersByLinkerCount - totalDoublets;
            //if (difference != 1)
            //{
            //    Console.WriteLine(Net.And.ReferersByLinkerCount - totalDoublets);
            //}
            return directConnection;
        }

        private static Link TryReconstructConnection(Link first, Link second)
        {
            Link directConnection = null;
            if (second.ReferersBySourceCount < first.ReferersBySourceCount)
            {
                //  o_|      x_o ... 
                // x_|        |___|
                //
                // <-
                second.WalkThroughReferersAsSource(couple =>
                {
                    if (couple.Linker == Net.And && couple.ReferersByTargetCount == 1 && couple.ReferersBySourceCount == 0)
                    {
                        var neighbour = couple.FirstRefererByTarget;
                        if (neighbour.Linker == Net.And && neighbour.Source == first)
                        {
                            if (directConnection == null)
                            {
                                directConnection = first & second;
                            }
                            Link.Update(ref neighbour, directConnection, Net.And, couple.Target);
                            //Link.Delete(ref couple); // Можно заменить удалением couple
                        }
                    }
                    if (couple.Linker == Net.And)
                    {
                        var neighbour = couple.FirstRefererByTarget;
                        if (neighbour.Linker == Net.And && neighbour.Source == first)
                        {

                        }
                    }
                });
            }
            else
            {
                //  o_|     x_o ... 
                // x_|       |___|
                //
                // ->
                first.WalkThroughReferersAsSource(couple =>
                {
                    if (couple.Linker == Net.And)
                    {
                        var neighbour = couple.Target;
                        if (neighbour.Linker == Net.And && neighbour.Source == second)
                        {
                            if (neighbour.ReferersByTargetCount == 1 && neighbour.ReferersBySourceCount == 0)
                            {
                                if (directConnection == null)
                                {
                                    directConnection = first & second;
                                }
                                Link.Update(ref couple, directConnection, Net.And, neighbour.Target);
                                //Link.Delete(ref neighbour);
                            }
                        }
                    }
                });
            }

            if (second.ReferersByTargetCount < first.ReferersByTargetCount)
            {
                // |_x      ... x_o
                //  |_o      |___|
                //
                //   <-
                second.WalkThroughReferersAsTarget(couple =>
                {
                    if (couple.Linker == Net.And)
                    {
                        var neighbour = couple.Source;
                        if (neighbour.Linker == Net.And && neighbour.Target == first)
                        {
                            if (neighbour.ReferersByTargetCount == 0 && neighbour.ReferersBySourceCount == 1)
                            {
                                if (directConnection == null)
                                {
                                    directConnection = first & second;
                                }
                                Link.Update(ref couple, neighbour.Source, Net.And, directConnection);
                                //Link.Delete(ref neighbour);
                            }
                        }
                    }
                });
            }
            else
            {
                // |_x      ... x_o
                //  |_o      |___|
                //
                //   ->
                first.WalkThroughReferersAsTarget((couple) =>
                {
                    if (couple.Linker == Net.And && couple.ReferersByTargetCount == 0 && couple.ReferersBySourceCount == 1)
                    {
                        var neighbour = couple.FirstRefererBySource;
                        if (neighbour.Linker == Net.And && neighbour.Target == second)
                        {
                            if (directConnection == null)
                            {
                                directConnection = first & second;
                            }
                            Link.Update(ref neighbour, couple.Source, Net.And, directConnection);
                            Link.Delete(ref couple);
                        }
                    }
                });
            }
            if (directConnection != null)
            {
                CompressionsCount++;
            }
            return directConnection;
        }

        ////public static Link CombinedJoin(Link left, Link right)
        ////{
        ////    Link rightSubJoint = Link.Search(left, Net.And, right.Source);
        ////    if (rightSubJoint != null && rightSubJoint != right)
        ////    {
        ////        long rightSubJointReferers = rightSubJoint.TotalReferers;
        ////        Link leftSubJoint = Link.Search(left.Target, Net.And, right);
        ////        if (leftSubJoint != null && leftSubJoint != left)
        ////        {
        ////            long leftSubJointReferers = leftSubJoint.TotalReferers;
        ////            if (leftSubJointReferers > rightSubJointReferers)
        ////            {
        ////                long leftReferers = left.TotalReferers;
        ////                if (leftReferers > 0)
        ////                {
        ////                    return Link.Create(left.Source, Net.And, leftSubJoint);
        ////                }
        ////                else
        ////                {
        ////                    Link.Update(ref left, left.Source, Net.And, leftSubJoint);
        ////                    return left;
        ////                }
        ////            }
        ////        }
        ////        long rightReferers = right.TotalReferers;
        ////        if (rightReferers > 0)
        ////        {
        ////            return Link.Create(rightSubJoint, Net.And, right.Target);
        ////        }
        ////        else
        ////        {
        ////            Link.Update(ref right, rightSubJoint, Net.And, right.Target);
        ////            return right;
        ////        }
        ////    }
        ////    return Link.Create(left, Net.And, right);
        ////}
        //public static Link CombinedJoin(Link left, Link right)
        //{
        //    long leftReferers = left.TotalReferers;
        //    Link leftSubJoint = Link.Search(left.Target, Net.And, right);
        //    if (leftSubJoint != null && leftSubJoint != left)
        //    {
        //        long leftSubJointReferers = leftSubJoint.TotalReferers;
        //    }
        //    long rightReferers = left.TotalReferers;
        //    Link rightSubJoint = Link.Search(left, Net.And, right.Source);
        //    long rightSubJointReferers = rightSubJoint != null ? rightSubJoint.TotalReferers : long.MinValue;
        //}
        //public static Link LeftJoinUnsafe(Link subject, Link @object)
        //{
        //    if (subject.Linker == Net.And && subject.ReferersBySourceCount == 0 && subject.ReferersByTargetCount == 0)
        //    {
        //        Link subJoint = Link.Search(@object, Net.And, subject.Source);
        //        if (subJoint != null && subJoint != subject)
        //        {
        //            Link.Update(ref subject, subJoint, Net.And, subject.Target);
        //            return subject;
        //        }
        //    }
        //    return Link.Create(@object, Net.And, subject);
        //}

        public static int ChunkSize = 2;

        //public static Link FromList(List<Link> links)
        //{
        //    Link element = links[0];
        //    for (int i = 1; i < links.Count; i += ChunkSize)
        //    {
        //        int j = (i + ChunkSize - 1);
        //        j = j < links.Count ? j : (links.Count - 1);
        //        Link subElement = links[j];
        //        while (--j >= i) LeftJoin(ref subElement, links[j]);
        //        RightJoin(ref element, subElement);
        //    }
        //    return element;
        //}

        //public static Link FromList(Link[] links)
        //{
        //    Link element = links[0];
        //    for (int i = 1; i < links.Length; i += ChunkSize)
        //    {
        //        int j = (i + ChunkSize - 1);
        //        j = j < links.Length ? j : (links.Length - 1);
        //        Link subElement = links[j];
        //        while (--j >= i) LeftJoin(ref subElement, links[j]);
        //        RightJoin(ref element, subElement);
        //    }
        //    return element;
        //}

        //public static Link FromList(IList<Link> links)
        //{
        //    Link element = links[0];
        //    for (int i = 1; i < links.Count; i += ChunkSize)
        //    {
        //        int j = (i + ChunkSize - 1);
        //        j = j < links.Count ? j : (links.Count - 1);
        //        Link subElement = links[j];
        //        while (--j >= i)
        //        {
        //            Link x = links[j];
        //            subElement = CombinedJoin(ref x, ref subElement);
        //        }
        //        element = CombinedJoin(ref element, ref subElement);
        //    }
        //    return element;
        //}
        //public static Link FromList(IList<Link> links)
        //{
        //    int i = 0;
        //    Link element = links[i++];
        //    if (links.Count % 2 == 0)
        //    {
        //        element = CombinedJoin(element, links[i++]);
        //    }
        //    for (; i < links.Count; i += 2)
        //    {
        //        Link doublet = CombinedJoin(links[i], links[i + 1]);
        //        element = CombinedJoin(ref element, ref doublet);
        //    }
        //    return element;
        //}

        // Заглушка, возможно опасная
        private static Link CombinedJoin(Link element, Link link)
        {
            return CombinedJoin(ref element, ref link);
        }

        //public static Link FromList(List<Link> links)
        //{
        //    int i = links.Count - 1;
        //    Link element = links[i];
        //    while (--i >= 0) element = LinkConverterOld.ConnectLinks2(links[i], element, links, ref i);
        //    return element;
        //}
        //public static Link FromList(Link[] links)
        //{
        //    int i = links.Length - 1;
        //    Link element = links[i];
        //    while (--i >= 0) element = LinkConverterOld.ConnectLinks2(links[i], element, links, ref i);
        //    return element;
        //}
        //public static Link FromList(List<Link> links)
        //{
        //    Link element = links[0];
        //    for (int i = 1; i < links.Count; i += ChunkSize)
        //    {
        //        int j = (i + ChunkSize - 1);
        //        j = j < links.Count ? j : (links.Count - 1);
        //        Link subElement = links[j];
        //        while (--j >= i) subElement = CombinedJoin(links[j], subElement);
        //        element = CombinedJoin(element, subElement);
        //    }
        //    return element;
        //}
        //public static Link FromList(Link[] links)
        //{
        //    Link element = links[0];
        //    for (int i = 1; i < links.Length; i += ChunkSize)
        //    {
        //        int j = (i + ChunkSize - 1);
        //        j = j < links.Length ? j : (links.Length - 1);
        //        Link subElement = links[j];
        //        while (--j >= i) subElement = CombinedJoin(links[j], subElement);
        //        element = CombinedJoin(element, subElement);
        //    }
        //    return element;
        //}
        //public static Link FromList(IList<Link> links)
        //{
        //    int leftBound = 0;
        //    int rightBound = links.Count - 1;
        //    if (leftBound == rightBound)
        //    {
        //        return links[0];
        //    }
        //    Link left = links[leftBound];
        //    Link right = links[rightBound];
        //    long leftReferers = left.ReferersBySourceCount + left.ReferersByTargetCount;
        //    long rightReferers = right.ReferersBySourceCount + right.ReferersByTargetCount;
        //    while (true)
        //    {
        //        //if (rightBound % 2 != leftBound % 2)
        //        if (rightReferers >= leftReferers)
        //        {
        //            int nextRightBound = --rightBound;
        //            if (nextRightBound == leftBound)
        //            {
        //                var x = CombinedJoin(ref left, ref right);
        //                return x;
        //            }
        //            else
        //            {
        //                Link nextRight = links[nextRightBound];
        //                right = CombinedJoin(ref nextRight, ref right);
        //                rightReferers = right.ReferersBySourceCount + right.ReferersByTargetCount;
        //            }
        //        }
        //        else
        //        {
        //            int nextLeftBound = ++leftBound;
        //            if (nextLeftBound == rightBound)
        //            {
        //                return CombinedJoin(ref left, ref right);
        //            }
        //            else
        //            {
        //                Link nextLeft = links[nextLeftBound];
        //                left = CombinedJoin(ref left, ref nextLeft);
        //                leftReferers = left.ReferersBySourceCount + left.ReferersByTargetCount;
        //            }
        //        }
        //    }
        //}
        //public static Link FromList(IList<Link> links)
        //{
        //    int i = links.Count - 1;
        //    Link element = links[i];
        //    while (--i >= 0)
        //    {
        //        LeftJoin(ref element, links[i]); // LeftJoin(ref element, links[i]);
        //    }
        //    return element;
        //}

        public static Link FromList(List<Link> links)
        {
            var i = links.Count - 1;
            var element = links[i];
            while (--i >= 0)
            {
                var x = links[i];
                element = CombinedJoin(ref x, ref element); // LeftJoin(ref element, links[i]);
            }
            return element;
        }

        public static Link FromList(Link[] links)
        {
            var i = links.Length - 1;
            var element = links[i];
            while (--i >= 0)
            {
                element = CombinedJoin(ref links[i], ref element); // LeftJoin(ref element, links[i]);
            }
            return element;
        }
    }
}
