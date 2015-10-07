using System;
using System.Collections.Generic;

namespace Platform.Links.DataBase.CoreUnsafe.Sequences
{
    /// <remarks>
    /// Реализованный внутри алгоритм наглядно показывает,
    /// что совершенно не обязательна рекурсивная реализация (с вложенным вызовом функцией самой себя),
    /// так как стэк можно использовать намного эффективнее при ручном управлении.
    /// 
    /// При оптимизации можно использовать встроенную поддержку стеков в процессор.
    /// 
    /// Решить объединять ли логику в одну функцию, или оставить 4 отдельных реализации?
    /// Решить встраивать ли защиту от зацикливания.
    /// </remarks>
    public class StopableSequenceWalker<TLink>
    {
        private readonly TLink _root;
        private readonly Func<TLink, TLink> _getSource;
        private readonly Func<TLink, TLink> _getTarget;
        private readonly Func<TLink, bool> _isElement;
        private readonly Func<TLink, bool> _visit;
        private readonly Stack<TLink> _stack;

        public StopableSequenceWalker(TLink sequence, Func<TLink, TLink> getSource, Func<TLink, TLink> getTarget, Func<TLink, bool> isElement, Func<TLink, bool> visit)
        {
            _root = sequence;
            _getSource = getSource;
            _getTarget = getTarget;
            _isElement = isElement;
            _visit = visit;
            _stack = new Stack<TLink>();
        }

        public static bool WalkRight(TLink sequence, Func<TLink, TLink> getSource, Func<TLink, TLink> getTarget, Func<TLink, bool> isElement, Func<TLink, bool> visit)
        {
            return (new StopableSequenceWalker<TLink>(sequence, getSource, getTarget, isElement, visit)).WalkFromLeftToRight();
        }

        public bool WalkFromLeftToRight()
        {
            var element = _root;

            if (_isElement(element))
                return _visit(element);

            while (true)
            {
                if (_isElement(element))
                {
                    if (_stack.Count == 0)
                        return true;

                    element = _stack.Pop();

                    var source = _getSource(element);
                    var target = _getTarget(element);

                    // Обработка элемента
                    if (_isElement(source) && !_visit(source))
                        return false;
                    if (_isElement(target) && !_visit(target))
                        return false;

                    element = target;
                }
                else
                {
                    _stack.Push(element);

                    element = _getSource(element);
                }
            }
        }

        public static bool WalkLeft(TLink sequence, Func<TLink, TLink> getSource, Func<TLink, TLink> getTarget, Func<TLink, bool> isElement, Func<TLink, bool> visit)
        {
            return (new StopableSequenceWalker<TLink>(sequence, getSource, getTarget, isElement, visit)).WalkFromRightToLeft();
        }

        public bool WalkFromRightToLeft()
        {
            var element = _root;

            if (_isElement(element))
                return _visit(element);

            while (true)
            {
                if (_isElement(element))
                {
                    if (_stack.Count == 0)
                        return true;

                    element = _stack.Pop();

                    var source = _getSource(element);
                    var target = _getTarget(element);

                    // Обработка элемента
                    if (_isElement(target) && !_visit(target))
                        return false;
                    if (_isElement(source) && !_visit(source))
                        return false;

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
