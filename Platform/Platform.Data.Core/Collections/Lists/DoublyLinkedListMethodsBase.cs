using System.Runtime.CompilerServices;

namespace Platform.Data.Core.ListMethods
{
    public abstract class DoublyLinkedListMethodsBase<TElement>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract TElement GetFirst();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract TElement GetLast();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract TElement GetPrevious(TElement element);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract TElement GetNext(TElement element);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract ulong GetSize();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void SetFirst(TElement element);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void SetLast(TElement element);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void SetPrevious(TElement element, TElement previous);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void SetNext(TElement element, TElement next);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void SetSize(ulong size);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void IncrementSize()
        {
            SetSize(GetSize() + 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DecrementSize()
        {
            SetSize(GetSize() - 1);
        }
    }
}
