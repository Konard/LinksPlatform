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

        static public bool WalkRight(Link sequence, Func<Link, bool> visit)
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

                    // Оптимизация проходов вправо вверх,
                    // нет необходимости регистировать такие проходы в стэке.
                    // Исключая из алгоритма только этот блок можно сохранить его работоспособность.
                    //while (target.Linker == Net.And && source.Linker != Net.And)
                    //{
                    //    if (!_visit(source))
                    //        return false;

                    //    source = target.Source;
                    //    target = target.Target;
                    //}

                    // Обработка элемента
                    if (source.Linker != Net.And && !_visit(source))
                        return false;
                    if (target.Linker != Net.And && !_visit(target))
                        return false;

                    element = target;
                }
            }
        }

        static public bool WalkLeft(Link sequence, Func<Link, bool> visit)
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

                    // Оптимизация проходов влево вверх,
                    // нет необходимости регистировать такие проходы в стэке.
                    // Исключая из алгоритма только этот блок можно сохранить его работоспособность.
                    //while (source.Linker == Net.And && target.Linker != Net.And)
                    //{
                    //    if (!_visit(target))
                    //        return false;

                    //    target = source.Target;
                    //    source = source.Source;
                    //}

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
