namespace Platform.Helpers
{
    public interface ISpecificPropertyOperator<TObject, TValue> : IProvider<TValue, TObject>
    {
        void Set(TObject @object, TValue value);
    }
}
