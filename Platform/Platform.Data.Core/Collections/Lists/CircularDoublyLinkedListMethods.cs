namespace Platform.Data.Core.Collections.Lists
{
    /// <remarks>
    /// Based on https://en.wikipedia.org/wiki/Doubly_linked_list
    /// </remarks>
    public abstract class CircularDoublyLinkedListMethods<TElement> : DoublyLinkedListMethodsBase<TElement>
    {
        public void AttachBefore(TElement baseElement, TElement newElement)
        {
            var baseElementPrevious = GetPrevious(baseElement);

            SetPrevious(newElement, baseElementPrevious);
            SetNext(newElement, baseElement);

            if (IsEquals(baseElement, GetFirst()))
                SetFirst(newElement);

            SetNext(baseElementPrevious, newElement);
            SetPrevious(baseElement, newElement);

            IncrementSize();
        }

        public void AttachAfter(TElement baseElement, TElement newElement)
        {
            var baseElementNext = GetNext(baseElement);

            SetPrevious(newElement, baseElement);
            SetNext(newElement, baseElementNext);

            if (IsEquals(baseElement, GetLast()))
                SetLast(newElement);

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
                SetPrevious(element, element);
                SetNext(element, element);

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

            if (IsEquals(elementNext, element))
            {
                SetFirst(GetZero());
                SetLast(GetZero());
            }
            else
            {
                SetNext(elementPrevious, elementNext);
                SetPrevious(elementNext, elementPrevious);

                if (IsEquals(element, GetFirst()))
                    SetFirst(elementNext);
                if (IsEquals(element, GetLast()))
                    SetLast(elementPrevious);
            }

            SetPrevious(element, GetZero());
            SetNext(element, GetZero());

            DecrementSize();
        }
    }
}
