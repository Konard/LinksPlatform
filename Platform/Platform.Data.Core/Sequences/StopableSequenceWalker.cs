using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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
    public class StopableSequenceWalker
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WalkRight<TLink>(TLink sequence, Func<TLink, TLink> getSource, Func<TLink, TLink> getTarget, Func<TLink, bool> isElement, Func<TLink, bool> visit)
        {
            var stack = new Stack<TLink>();
            var element = sequence;

            if (isElement(element))
                return visit(element);

            while (true)
            {
                if (isElement(element))
                {
                    if (stack.Count == 0)
                        return true;

                    element = stack.Pop();

                    var source = getSource(element);
                    var target = getTarget(element);

                    // Обработка элемента
                    if (isElement(source) && !visit(source))
                        return false;
                    if (isElement(target) && !visit(target))
                        return false;

                    element = target;
                }
                else
                {
                    stack.Push(element);

                    element = getSource(element);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WalkLeft<TLink>(TLink sequence, Func<TLink, TLink> getSource, Func<TLink, TLink> getTarget, Func<TLink, bool> isElement, Func<TLink, bool> visit)
        {
            var stack = new Stack<TLink>();
            var element = sequence;

            if (isElement(element))
                return visit(element);

            while (true)
            {
                if (isElement(element))
                {
                    if (stack.Count == 0)
                        return true;

                    element = stack.Pop();

                    var source = getSource(element);
                    var target = getTarget(element);

                    // Обработка элемента
                    if (isElement(target) && !visit(target))
                        return false;
                    if (isElement(source) && !visit(source))
                        return false;

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
