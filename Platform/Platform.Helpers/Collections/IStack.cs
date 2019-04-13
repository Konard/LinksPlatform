namespace Platform.Helpers.Collections
{
    public interface IStack<TElement>
    {
        void Push(TElement element);
        TElement Pop();
        TElement Peek();
    }
}
