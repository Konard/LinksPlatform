using System;
using System.Collections.Generic;

namespace Platform.Links.DataBase.CoreUnsafe.Sequences
{
    /// <remarks>
    /// Реализованный внутри алгоритм наглядно показывает,
    /// что совершенно не обязательна рекурсивная реализация (с вложенным вызовом функцией самой себя),
    /// так как стэк можно использовать намного эффективнее при ручном управлении.
    /// </remarks>
    public class SequenceWalker<TLink>
    {
        private readonly TLink _root;
        private readonly Func<TLink, TLink> _getSource;
        private readonly Func<TLink, TLink> _getTarget;
        private readonly Func<TLink, bool> _isElement;
        private readonly Action<TLink> _visit;
        private readonly Stack<TLink> _stack;

        public SequenceWalker(TLink sequence, Func<TLink, TLink> getSource, Func<TLink, TLink> getTarget, Func<TLink, bool> isElement, Action<TLink> visit)
        {
            _root = sequence;
            _getSource = getSource;
            _getTarget = getTarget;
            _isElement = isElement;
            _visit = visit;
            _stack = new Stack<TLink>();
        }

        public static void WalkRight(TLink sequence, Func<TLink, TLink> getSource, Func<TLink, TLink> getTarget, Func<TLink, bool> isElement, Action<TLink> visit)
        {
            (new SequenceWalker<TLink>(sequence, getSource, getTarget, isElement, visit)).WalkFromLeftToRight();
        }

        public void WalkFromLeftToRight()
        {
            var element = _root;

            if (_isElement(element))
                _visit(element);
            else
                while (true)
                {
                    if (_isElement(element))
                    {
                        if (_stack.Count == 0)
                            break;

                        element = _stack.Pop();

                        var source = _getSource(element);
                        var target = _getTarget(element);

                        // Обработка элемента
                        if (_isElement(source)) _visit(source);
                        if (_isElement(target)) _visit(target);

                        element = target;
                    }
                    else
                    {
                        _stack.Push(element);

                        element = _getSource(element);
                    }
                }
        }

        public static void WalkLeft(TLink sequence, Func<TLink, TLink> getSource, Func<TLink, TLink> getTarget, Func<TLink, bool> isElement, Action<TLink> visit)
        {
            (new SequenceWalker<TLink>(sequence, getSource, getTarget, isElement, visit)).WalkFromRightToLeft();
        }

        public void WalkFromRightToLeft()
        {
            var element = _root;

            if (_isElement(element))
                _visit(element);
            else
                while (true)
                {
                    if (_isElement(element))
                    {
                        if (_stack.Count == 0)
                            break;

                        element = _stack.Pop();

                        var source = _getSource(element);
                        var target = _getTarget(element);

                        // Обработка элемента
                        if (_isElement(target)) _visit(target);
                        if (_isElement(source)) _visit(source);

                        element = source;
                    }
                    else
                    {
                        _stack.Push(element);

                        element = _getTarget(element);
                    }
                }
        }
    }
}
