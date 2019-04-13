namespace Platform.Helpers
{
    public interface IPropertyOperator<TObject, TProperty, TValue>
    {
        TValue GetValue(TObject @object, TProperty property);
        void SetValue(TObject @object, TProperty property, TValue value);
    }
}
