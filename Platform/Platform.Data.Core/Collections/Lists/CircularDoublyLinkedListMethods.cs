namespace Platform.Data.Core.Collections.Lists
{
    /// <remarks>
    /// Based on https://en.wikipedia.org/wiki/Doubly_linked_list
    /// </remarks>
    public abstract class CircularDoublyLinkedListMethods : DoublyLinkedListMethodsBase<ulong>
    {
        public void AttachBefore(ulong baseElement, ulong newElement)
        {
            var baseElementPrevious = GetPrevious(baseElement);

            SetPrevious(newElement, baseElementPrevious);
            SetNext(newElement, baseElement);

            if (baseElement == GetFirst())
                SetFirst(newElement);

            SetNext(baseElementPrevious, newElement);
            SetPrevious(baseElement, newElement);

            IncrementSize();
        }

        public void AttachAfter(ulong baseElement, ulong newElement)
        {
            var baseElementNext = GetNext(baseElement);

            SetPrevious(newElement, baseElement);
            SetNext(newElement, baseElementNext);

            if (baseElement == GetLast())
                SetLast(newElement);

            SetPrevious(baseElementNext, newElement);
            SetNext(baseElement, newElement);

            IncrementSize();
        }

        public void AttachAsFirst(ulong element)
        {
            var first = GetFirst();
            if (first == 0)
            {
                SetFirst(element);
                SetLast(element);
                SetPrevious(element, element);
                SetNext(element, element);

                IncrementSize();
            }
            else
                AttachBefore(first, element);
        }

        public void AttachAsLast(ulong element)
        {
            var last = GetLast();
            if (last == 0)
                AttachAsFirst(element);
            else
                AttachAfter(last, element);
        }

        public void Detach(ulong element)
        {
            var elementPrevious = GetPrevious(element);
            var elementNext = GetNext(element);

            if (elementNext == element)
            {
                SetFirst(0);
                SetLast(0);
            }
            else
            {
                SetNext(elementPrevious, elementNext);
                SetPrevious(elementNext, elementPrevious);

                if (element == GetFirst())
                    SetFirst(elementNext);
                if (element == GetLast())
                    SetLast(elementPrevious);
            }

            SetPrevious(element, 0);
            SetNext(element, 0);

            DecrementSize();
        }
    }
}
