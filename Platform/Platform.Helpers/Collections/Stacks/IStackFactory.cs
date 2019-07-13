using Platform.Interfaces;

namespace Platform.Helpers.Collections.Stack
{
    public interface IStackFactory<TElement> : IFactory<IStack<TElement>>
    {
    }
}
