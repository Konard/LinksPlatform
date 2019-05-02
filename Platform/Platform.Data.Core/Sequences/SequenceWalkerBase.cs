using Platform.Data.Core.Common;
using Platform.Data.Core.Doublets;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Platform.Data.Core.Sequences
{
    public abstract class SequenceWalkerBase<TLink> : LinksOperatorBase<TLink>, ISequenceWalker<TLink>
    {
        private readonly System.Collections.Generic.Stack<IList<TLink>> _stack;

        protected SequenceWalkerBase(ILinks<TLink> links) : base(links) => _stack = new System.Collections.Generic.Stack<IList<TLink>>();

        public IEnumerable<IList<TLink>> Walk(TLink sequence)
        {
            if (_stack.Count > 1)
                _stack.Clear();

            var element = Links.GetLink(sequence);

            if (IsElement(element))
                yield return element;
            else
                while (true)
                {
                    if (IsElement(element))
                    {
                        if (_stack.Count == 0)
                            break;

                        element = _stack.Pop();

                        foreach (var output in WalkContents(element))
                            yield return output;

                        element = GetNextElementAfterPop(element);
                    }
                    else
                    {
                        _stack.Push(element);

                        element = GetNextElementAfterPush(element);
                    }
                }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool IsElement(IList<TLink> elementLink) => Point<TLink>.IsPartialPointUnchecked(elementLink);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract IList<TLink> GetNextElementAfterPop(IList<TLink> element);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract IList<TLink> GetNextElementAfterPush(IList<TLink> element);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract IEnumerable<IList<TLink>> WalkContents(IList<TLink> element);
    }
}
