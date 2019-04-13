namespace Platform.Helpers
{
    public interface IConverter<TSource, TTarget>
    {
        TTarget Convert(TSource source);
    }
}
