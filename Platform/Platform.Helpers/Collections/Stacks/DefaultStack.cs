using System.Collections.Generic;

namespace Platform.Helpers.Collections.Stacks
{
    public class DefaultStack<TElement> : IStack<TElement>
    {
        private readonly Stack<TElement> _stack;

        public DefaultStack() => _stack = new Stack<TElement>();

        public TElement Peek() => _stack.Peek();

        public TElement Pop() => _stack.Pop();

        public void Push(TElement element) => _stack.Push(element);
    }
}
