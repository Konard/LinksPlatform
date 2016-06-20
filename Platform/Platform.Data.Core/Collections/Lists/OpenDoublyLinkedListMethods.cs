namespace Platform.Data.Core.Collections.Lists
{
    /// <remarks>
    /// Based on https://en.wikipedia.org/wiki/Doubly_linked_list
    /// </remarks>
    public abstract class OpenDoublyLinkedListMethods<TElement> : DoublyLinkedListMethodsBase<TElement>
    {
        public void AttachBefore(TElement baseElement, TElement newElement)
        {
            var baseElementPrevious = GetPrevious(baseElement);

            SetPrevious(newElement, baseElementPrevious);
            SetNext(newElement, baseElement);

            if (EqualToZero(baseElementPrevious))
                SetFirst(newElement);
            else
                SetNext(baseElementPrevious, newElement);

            SetPrevious(baseElement, newElement);

            IncrementSize();
        }

        public void AttachAfter(TElement baseElement, TElement newElement)
        {
            var baseElementNext = GetNext(baseElement);

            SetPrevious(newElement, baseElement);
            SetNext(newElement, baseElementNext);

            if (EqualToZero(baseElementNext))
                SetLast(newElement);
            else
                SetPrevious(baseElementNext, newElement);

            SetNext(baseElement, newElement);

            IncrementSize();
        }

        public void AttachAsFirst(TElement element)
        {
            var first = GetFirst();
            if (EqualToZero(first))
            {
                SetFirst(element);
                SetLast(element);
                SetPrevious(element, GetZero());
                SetNext(element, GetZero());

                IncrementSize();
            }
            else
                AttachBefore(first, element);
        }

        public void AttachAsLast(TElement element)
        {
            var last = GetLast();
            if (EqualToZero(last))
                AttachAsFirst(element);
            else
                AttachAfter(last, element);
        }

        public void Detach(TElement element)
        {
            var elementPrevious = GetPrevious(element);
            var elementNext = GetNext(element);

            if (EqualToZero(elementPrevious))
                SetFirst(elementNext);
            else
                SetNext(elementPrevious, elementNext);

            if (EqualToZero(elementNext))
                SetLast(elementPrevious);
            else
                SetPrevious(elementNext, elementPrevious);

            SetPrevious(element, GetZero());
            SetNext(element, GetZero());

            DecrementSize();
        }
    }
}
