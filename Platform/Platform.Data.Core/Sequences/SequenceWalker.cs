using System;
using System.Collections.Generic;

namespace Platform.Data.Core.Sequences
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
    /// Альтернативой защиты от закливания может быть заранее известное ограничение на погружение вглубь.
    /// </remarks>
    public class SequenceWalker
    {
        public static void WalkRight<TLink>(TLink sequence, Func<TLink, TLink> getSource, Func<TLink, TLink> getTarget, Func<TLink, bool> isElement, Action<TLink> visit)
        {
            var stack = new Stack<TLink>();
            var element = sequence;

            if (isElement(element))
                visit(element);
            else
                while (true)
                {
                    if (isElement(element))
                    {
                        if (stack.Count == 0)
                            break;

                        element = stack.Pop();

                        var source = getSource(element);
                        var target = getTarget(element);

                        // Обработка элемента
                        if (isElement(source)) visit(source);
                        if (isElement(target)) visit(target);

                        element = target;
                    }
                    else
                    {
                        stack.Push(element);

                        element = getSource(element);
                    }
                }
        }

        public static void WalkLeft<TLink>(TLink sequence, Func<TLink, TLink> getSource, Func<TLink, TLink> getTarget, Func<TLink, bool> isElement, Action<TLink> visit)
        {
            var stack = new Stack<TLink>();
            var element = sequence;

            if (isElement(element))
                visit(element);
            else
                while (true)
                {
                    if (isElement(element))
                    {
                        if (stack.Count == 0)
                            break;

                        element = stack.Pop();

                        var source = getSource(element);
                        var target = getTarget(element);

                        // Обработка элемента
                        if (isElement(target)) visit(target);
                        if (isElement(source)) visit(source);

                        element = source;
                    }
                    else
                    {
                        stack.Push(element);

                        element = getTarget(element);
                    }
                }
        }
    }
}
