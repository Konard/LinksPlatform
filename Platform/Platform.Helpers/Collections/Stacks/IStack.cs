namespace Platform.Helpers.Collections.Stack
{
    public interface IStack<TElement>
    {
        void Push(TElement element);
        TElement Pop();
        TElement Peek();
    }
}
