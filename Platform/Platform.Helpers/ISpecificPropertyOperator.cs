namespace Platform.Helpers
{
    public interface ISpecificPropertyOperator<TObject, TValue>
    {
        TValue GetValue(TObject @object);
        void SetValue(TObject @object, TValue value);
    }
}
