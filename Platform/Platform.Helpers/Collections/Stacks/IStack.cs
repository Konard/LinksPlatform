namespace Platform.Helpers.Collections.Stacks
{
    public interface IStack<TElement>
    {
        void Push(TElement element);
        TElement Pop();
        TElement Peek();
    }
}
