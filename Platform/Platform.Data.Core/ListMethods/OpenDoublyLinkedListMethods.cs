namespace Platform.Data.Core.ListMethods
{
    /// <remarks>
    /// Based on https://en.wikipedia.org/wiki/Doubly_linked_list
    /// </remarks>
    public abstract class OpenDoublyLinkedListMethods : DoublyLinkedListMethodsBase<ulong>
    {
        public void AttachBefore(ulong baseElement, ulong newElement)
        {
            var baseElementPrevious = GetPrevious(baseElement);

            SetPrevious(newElement, baseElementPrevious);
            SetNext(newElement, baseElement);

            if (baseElementPrevious == 0)
                SetFirst(newElement);
            else
                SetNext(baseElementPrevious, newElement);

            SetPrevious(baseElement, newElement);

            IncrementSize();
        }

        public void AttachAfter(ulong baseElement, ulong newElement)
        {
            var baseElementNext = GetNext(baseElement);

            SetPrevious(newElement, baseElement);
            SetNext(newElement, baseElementNext);

            if (baseElementNext == 0)
                SetLast(newElement);
            else
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
                SetPrevious(element, 0);
                SetNext(element, 0);

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

            if (elementPrevious == 0)
                SetFirst(elementNext);
            else
                SetNext(elementPrevious, elementNext);

            if (elementNext == 0)
                SetLast(elementPrevious);
            else
                SetPrevious(elementNext, elementPrevious);

            SetPrevious(element, 0);
            SetNext(element, 0);

            DecrementSize();
        }
    }
}
