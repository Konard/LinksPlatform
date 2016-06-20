using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Helpers.Collections;

namespace Platform.Data.Core.Pairs
{
    /// <remarks>
    /// What does empty pattern (for condition or substitution) mean? Nothing or Everything?
    /// Now we go with nothing. And nothing is something one, but empty, and cannot be changed by itself. But can cause creation (update from nothing) or deletion (update to nothing).
    /// </remarks>
    public class UniLinks<T> : LinksBase<T, T, T>, IUniLinks<T>
    {
        /// <remarks>
        /// TODO: Decide to change to IPairLinks or not to change. (Better to create DefaultUniLinksBase, that contains logic itself and can be implemented using both IPairLinks and ILinksMemoryManager.)
        /// </remarks>
        public UniLinks(ILinksMemoryManager<T> memory, ILinksCombinedConstants<T, T, T> constants)
            : base(memory, constants)
        {
        }

        public UniLinks(ILinksMemoryManager<T> memory)
            : base(memory)
        {
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
                if (matchHandler != null && Equals(matchHandler(before), Constants.Break))
                    return Constants.Break;

                var after = (IList<T>)substitution.ToArray();

                if (Equals(after[0], default(T)))
                {
                    var newLink = Memory.AllocateLink();
                    after[0] = newLink;
                }

                if (substitution.Count == 1)
                    after = Memory.GetLinkValue(substitution[0]);
                else if (substitution.Count == 3)
                    Memory.SetLinkValue(after);
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
                    var before = Memory.GetLinkValue(linkToDelete);

                    if (matchHandler != null && Equals(matchHandler(before), Constants.Break))
                        return Constants.Break;

                    var after = ArrayPool<T>.Empty;

                    Memory.SetLinkValue(linkToDelete, Constants.Null, Constants.Null);
                    Memory.FreeLink(linkToDelete);

                    if (matchHandler != null)
                        return substitutionHandler(before, after);
                    return Constants.Continue;
                }
                else
                    throw new NotSupportedException();
            }
            else // Replace / Update
            {
                if (patternOrCondition.Count == 1)
                {
                    var linkToUpdate = patternOrCondition[0];
                    var before = Memory.GetLinkValue(linkToUpdate);

                    if (matchHandler != null && Equals(matchHandler(before), Constants.Break))
                        return Constants.Break;

                    var after = (IList<T>)substitution.ToArray();

                    if (Equals(after[0], default(T)))
                        after[0] = linkToUpdate;

                    if (substitution.Count == 1)
                    {
                        if (!Equals(substitution[0], linkToUpdate))
                        {
                            after = Memory.GetLinkValue(substitution[0]);

                            Memory.SetLinkValue(linkToUpdate, Constants.Null, Constants.Null);
                            Memory.FreeLink(linkToUpdate);
                        }
                    }
                    else if (substitution.Count == 3)
                    {
                        Memory.SetLinkValue(after);
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

        public T Count(IList<T> restrictions)
        {
            throw new NotImplementedException();
        }
    }
}

