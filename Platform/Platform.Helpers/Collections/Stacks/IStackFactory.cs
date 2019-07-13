using Platform.Interfaces;

namespace Platform.Helpers.Collections.Stacks
{
    public interface IStackFactory<TElement> : IFactory<IStack<TElement>>
    {
    }
}
