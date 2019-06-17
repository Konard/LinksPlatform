using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data.Core.Common;
using Platform.Helpers;
using Platform.Helpers.Collections;

namespace Platform.Data.Core.Doublets
{
    /// <remarks>
    /// What does empty pattern (for condition or substitution) mean? Nothing or Everything?
    /// Now we go with nothing. And nothing is something one, but empty, and cannot be changed by itself. But can cause creation (update from nothing) or deletion (update to nothing).
    /// 
    /// TODO: Decide to change to IDoubletLinks or not to change. (Better to create DefaultUniLinksBase, that contains logic itself and can be implemented using both IDoubletLinks and ILinks.)
    /// </remarks>
    internal class UniLinks<T> : LinksDecoratorBase<T>, IUniLinks<T>
    {
        public UniLinks(ILinks<T> links)
            : base(links)
        {
        }

        private struct Transition
        {
            public IList<T> Before;
            public IList<T> After;

            public Transition(IList<T> before, IList<T> after)
            {
                Before = before;
                After = after;
            }
        }

        private static readonly T NullConstant = Use<ILinksCombinedConstants<T, T, int>>.Single.Null;
        private static readonly IList<T> NullLink = new List<T> { NullConstant, NullConstant, NullConstant };



        // TODO: Подумать о том, как реализовать древовидный Restriction и Substitution (Links-Expression)

        public T Trigger(IList<T> restriction, Func<IList<T>, IList<T>, T> matchedHandler, IList<T> substitution, Func<IList<T>, IList<T>, T> substitutedHandler)
        {
            ////List<Transition> transitions = null;

            ////if (!restriction.IsNullOrEmpty())
            ////{
            ////    // Есть причина делать проход (чтение)

            ////    if (matchedHandler != null)
            ////    {
            ////        if (!substitution.IsNullOrEmpty())
            ////        {
            ////            // restriction => { 0, 0, 0 } | { 0 } // Create
            ////            // substitution => { itself, 0, 0 } | { itself, itself, itself } // Create / Update
            ////            // substitution => { 0, 0, 0 } | { 0 } // Delete

            ////            transitions = new List<Transition>();

            ////            if (Equals(substitution[Constants.IndexPart], Constants.Null))
            ////            {
            ////                // If index is Null, that means we always ignore every other value (they are also Null by definition)


            ////                var matchDecision = matchedHandler(, NullLink);

            ////                if (Equals(matchDecision, Constants.Break))
            ////                    return false;

            ////                if (!Equals(matchDecision, Constants.Skip))
            ////                    transitions.Add(new Transition(matchedLink, newValue));
            ////            }
            ////            else
            ////            {
            ////                Func<T, bool> handler;

            ////                handler = link =>
            ////                {
            ////                    var matchedLink = Memory.GetLinkValue(link);
            ////                    var newValue = Memory.GetLinkValue(link);
            ////                    newValue[Constants.IndexPart] = Constants.Itself;
            ////                    newValue[Constants.SourcePart] = Equals(substitution[Constants.SourcePart], Constants.Itself) ? matchedLink[Constants.IndexPart] : substitution[Constants.SourcePart];
            ////                    newValue[Constants.TargetPart] = Equals(substitution[Constants.TargetPart], Constants.Itself) ? matchedLink[Constants.IndexPart] : substitution[Constants.TargetPart];

            ////                    var matchDecision = matchedHandler(matchedLink, newValue);

            ////                    if (Equals(matchDecision, Constants.Break))
            ////                        return false;

            ////                    if (!Equals(matchDecision, Constants.Skip))
            ////                        transitions.Add(new Transition(matchedLink, newValue));

            ////                    return true;
            ////                };

            ////                if (!Memory.Each(handler, restriction))
            ////                    return Constants.Break;
            ////            }
            ////        }
            ////        else
            ////        {
            ////            Func<T, bool> handler = link =>
            ////            {
            ////                var matchedLink = Memory.GetLinkValue(link);

            ////                var matchDecision = matchedHandler(matchedLink, matchedLink);

            ////                return !Equals(matchDecision, Constants.Break);
            ////            };

            ////            if (!Memory.Each(handler, restriction))
            ////                return Constants.Break;
            ////        }
            ////    }
            ////    else
            ////    {
            ////        if (substitution != null)
            ////        {
            ////            transitions = new List<IList<T>>();

            ////            Func<T, bool> handler = link =>
            ////            {
            ////                var matchedLink = Memory.GetLinkValue(link);
            ////                transitions.Add(matchedLink);
            ////                return true;
            ////            };

            ////            if (!Memory.Each(handler, restriction))
            ////                return Constants.Break;
            ////        }
            ////        else
            ////        {
            ////            return Constants.Continue;
            ////        }
            ////    }
            ////}

            ////if (substitution != null)
            ////{
            ////    // Есть причина делать замену (запись)

            ////    if (substitutedHandler != null)
            ////    {

            ////    }
            ////    else
            ////    {

            ////    }
            ////}


            ////return Constants.Continue;


            //if (restriction.IsNullOrEmpty()) // Create
            //{
            //    substitution[Constants.IndexPart] = Memory.AllocateLink();
            //    Memory.SetLinkValue(substitution);
            //}
            //else if (substitution.IsNullOrEmpty()) // Delete
            //{
            //    Memory.FreeLink(restriction[Constants.IndexPart]);
            //}
            //else if (restriction.EqualTo(substitution)) // Read or ("repeat" the state) // Each
            //{
            //    // No need to collect links to list
            //    // Skip == Continue

            //    // No need to check substituedHandler

            //    if (!Memory.Each(link => !Equals(matchedHandler(Memory.GetLinkValue(link)), Constants.Break), restriction))
            //        return Constants.Break;
            //}
            //else // Update
            //{
            //    //List<IList<T>> matchedLinks = null;

            //    if (matchedHandler != null)
            //    {
            //        matchedLinks = new List<IList<T>>();

            //        Func<T, bool> handler = link =>
            //        {
            //            var matchedLink = Memory.GetLinkValue(link);

            //            var matchDecision = matchedHandler(matchedLink);

            //            if (Equals(matchDecision, Constants.Break))
            //                return false;

            //            if (!Equals(matchDecision, Constants.Skip))
            //                matchedLinks.Add(matchedLink);

            //            return true;
            //        };

            //        if (!Memory.Each(handler, restriction))
            //            return Constants.Break;
            //    }

            //    if (!matchedLinks.IsNullOrEmpty())
            //    {
            //        var totalMatchedLinks = matchedLinks.Count;
            //        for (var i = 0; i < totalMatchedLinks; i++)
            //        {
            //            var matchedLink = matchedLinks[i];

            //            if (substitutedHandler != null)
            //            {
            //                var newValue = new List<T>(); // TODO: Prepare value to update here

            //                // TODO: Decide is it actually needed to use Before and After substitution handling.

            //                var substitutedDecision = substitutedHandler(matchedLink, newValue);

            //                if (Equals(substitutedDecision, Constants.Break))
            //                    return Constants.Break;

            //                if (Equals(substitutedDecision, Constants.Continue))
            //                {
            //                    // Actual update here

            //                    Memory.SetLinkValue(newValue);
            //                }

            //                if (Equals(substitutedDecision, Constants.Skip))
            //                {
            //                    // Cancel the update. TODO: decide use separate Cancel constant or Skip is enough?
            //                }

            //            }

            //        }
            //    }
            //}



            return Constants.Continue;
        }

        public T Trigger(IList<T> patternOrCondition, Func<IList<T>, T> matchHandler, IList<T> substitution, Func<IList<T>, IList<T>, T> substitutionHandler)
        {
            if (patternOrCondition.IsNullOrEmpty() && substitution.IsNullOrEmpty())
                return Constants.Continue;
            else if (patternOrCondition.EqualTo(substitution)) // Should be Each here TODO: Check if it is a correct condition
            {
                // Or it only applies to trigger without matchHandler.
                throw new NotImplementedException();
            }
            else if (!substitution.IsNullOrEmpty()) // Creation
            {
                var before = ArrayPool<T>.Empty;

                // Что должно означать False здесь? Остановиться (перестать идти) или пропустить (пройти мимо) или пустить (взять)?
                if (matchHandler != null && MathHelpers<T>.IsEquals(matchHandler(before), Constants.Break))
                    return Constants.Break;

                var after = (IList<T>)substitution.ToArray();

                if (MathHelpers<T>.IsEquals(after[0], default))
                {
                    var newLink = Links.Create();
                    after[0] = newLink;
                }

                if (substitution.Count == 1)
                    after = Links.GetLink(substitution[0]);
                else if (substitution.Count == 3)
                    Links.Update(after);
                else
                    throw new NotSupportedException();

                if (matchHandler != null)
                    return substitutionHandler(before, after);
                return Constants.Continue;
            }
            else if (!patternOrCondition.IsNullOrEmpty()) // Deletion
            {
                if (patternOrCondition.Count == 1)
                {
                    var linkToDelete = patternOrCondition[0];
                    var before = Links.GetLink(linkToDelete);

                    if (matchHandler != null && MathHelpers<T>.IsEquals(matchHandler(before), Constants.Break))
                        return Constants.Break;

                    var after = ArrayPool<T>.Empty;

                    Links.Update(linkToDelete, Constants.Null, Constants.Null);
                    Links.Delete(linkToDelete);

                    if (matchHandler != null)
                        return substitutionHandler(before, after);
                    return Constants.Continue;
                }
                else
                    throw new NotSupportedException();
            }
            else // Replace / Update
            {
                if (patternOrCondition.Count == 1) //-V3125
                {
                    var linkToUpdate = patternOrCondition[0];
                    var before = Links.GetLink(linkToUpdate);

                    if (matchHandler != null && MathHelpers<T>.IsEquals(matchHandler(before), Constants.Break))
                        return Constants.Break;

                    var after = (IList<T>)substitution.ToArray(); //-V3125

                    if (MathHelpers<T>.IsEquals(after[0], default))
                        after[0] = linkToUpdate;

                    if (substitution.Count == 1)
                    {
                        if (!MathHelpers<T>.IsEquals(substitution[0], linkToUpdate))
                        {
                            after = Links.GetLink(substitution[0]);

                            Links.Update(linkToUpdate, Constants.Null, Constants.Null);
                            Links.Delete(linkToUpdate);
                        }
                    }
                    else if (substitution.Count == 3)
                    {
                        Links.Update(after);
                    }
                    else
                        throw new NotSupportedException();

                    if (matchHandler != null)
                        return substitutionHandler(before, after);
                    return Constants.Continue;
                }
                else
                    throw new NotSupportedException();
            }
        }

        /// <remarks>
        /// IList[IList[IList[T]]]
        /// |     |     |      |||
        /// |     |      ------ ||
        /// |     |       link  ||
        /// |      ------------- |
        /// |         change     |
        ///  --------------------
        ///        changes
        /// </remarks>
        public IList<IList<IList<T>>> Trigger(IList<T> condition, IList<T> substitution)
        {
            var changes = new List<IList<IList<T>>>();

            Trigger(condition, AlwaysContinue, substitution, (before, after) =>
            {
                var change = new[] { before, after };
                changes.Add(change);

                return Constants.Continue;
            });

            return changes;
        }

        private T AlwaysContinue(IList<T> linkToMatch) => Constants.Continue;
    }
}

