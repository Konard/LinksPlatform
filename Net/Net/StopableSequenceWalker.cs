using System;
using System.Collections.Generic;

namespace NetLibrary
{
    /// <remarks>
    /// Реализованный внутри алгоритм наглядно показывает,
    /// что совершенно не обязательна рекурсивная реализация (с вложенным вызовом функцией самой себя),
    /// так как стэк можно использовать намного эффективнее при ручном управлении.
    /// </remarks>
    public class StopableSequenceWalker
    {
        private readonly Link _root;
        private readonly Func<Link, bool> _visit;
        private readonly Stack<Link> _stack;

        public StopableSequenceWalker(Link sequence, Func<Link, bool> visit)
        {
            _root = sequence;
            _visit = visit;
            _stack = new Stack<Link>();
        }

        public static bool WalkRight(Link sequence, Func<Link, bool> visit)
        {
            return (new StopableSequenceWalker(sequence, visit)).WalkFromLeftToRight();
        }

        public bool WalkFromLeftToRight()
        {
            Link element = _root;

            if (element.Linker != Net.And)
                return _visit(element);

            while (true)
            {
                if (element.Linker == Net.And)
                {
                    _stack.Push(element);

                    element = element.Source;
                }
                else
                {
                    if (_stack.Count == 0)
                        return true;

                    element = _stack.Pop();

                    Link source = element.Source;
                    Link target = element.Target;

                    // Обработка элемента
                    if (source.Linker != Net.And && !_visit(source))
                        return false;
                    if (target.Linker != Net.And && !_visit(target))
                        return false;

                    element = target;
                }
            }
        }

        public static bool WalkLeft(Link sequence, Func<Link, bool> visit)
        {
            return (new StopableSequenceWalker(sequence, visit)).WalkFromRightToLeft();
        }

        public bool WalkFromRightToLeft()
        {
            Link element = _root;

            if (element.Linker != Net.And)
                return _visit(element);

            while (true)
            {
                if (element.Linker == Net.And)
                {
                    _stack.Push(element);

                    element = element.Target;
                }
                else
                {
                    if (_stack.Count == 0)
                        return true;

                    element = _stack.Pop();

                    Link target = element.Target;
                    Link source = element.Source;

                    // Обработка элемента
                    if (target.Linker != Net.And && !_visit(target))
                        return false;
                    if (source.Linker != Net.And && !_visit(source))
                        return false;

                    element = source;
                }
            }
        }
    }
}
