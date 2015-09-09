using System;
using System.Collections.Generic;

namespace NetLibrary
{
    /// <remarks>
    /// Реализованный внутри алгоритм наглядно показывает,
    /// что совершенно не обязательна рекурсивная реализация (с вложенным вызовом функцией самой себя),
    /// так как стэк можно использовать намного эффективнее при ручном управлении.
    /// </remarks>
    public class SequenceWalker
    {
        private readonly Link _root;
        private readonly Action<Link> _visit;
        private readonly Stack<Link> _stack;

        public SequenceWalker(Link sequence, Action<Link> visit)
        {
            _root = sequence;
            _visit = visit;
            _stack = new Stack<Link>();
        }

        public void WalkFromLeftToRight()
        {
            Link element = _root;

            if (element.Linker == Net.And)
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
                            break;

                        element = _stack.Pop();

                        Link source = element.Source;
                        Link target = element.Target;

                        // Оптимизация проходов вправо вверх,
                        // нет необходимости регистировать такие проходы в стэке.
                        // Исключая из алгоритма только этот блок можно сохранить его работоспособность.
                        //while (target.Linker == Net.And && source.Linker != Net.And)
                        //{
                        //    _visit(source);
                        //    source = target.Source;
                        //    target = target.Target;
                        //}

                        // Обработка элемента
                        if (source.Linker != Net.And) _visit(source);
                        if (target.Linker != Net.And) _visit(target);

                        element = target;
                    }
                }
            else
                _visit(element);
        }

        public void WalkFromRightToLeft()
        {
            Link element = _root;

            if (element.Linker == Net.And)
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
                            break;

                        element = _stack.Pop();

                        Link target = element.Target;
                        Link source = element.Source;

                        // Оптимизация проходов влево вверх,
                        // нет необходимости регистировать такие проходы в стэке.
                        // Исключая из алгоритма только этот блок можно сохранить его работоспособность.
                        //while (source.Linker == Net.And && target.Linker != Net.And)
                        //{
                        //    _visit(target);
                        //    target = source.Target;
                        //    source = source.Source;
                        //}

                        // Обработка элемента
                        if (target.Linker != Net.And) _visit(target);
                        if (source.Linker != Net.And) _visit(source);

                        element = source;
                    }
                }
            else
                _visit(element);
        }
    }
}
