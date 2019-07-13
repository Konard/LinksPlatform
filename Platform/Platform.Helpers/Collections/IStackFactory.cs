using Platform.Interfaces;

namespace Platform.Helpers.Collections
{
    public interface IStackFactory<TElement> : IFactory<IStack<TElement>>
    {
    }
}
