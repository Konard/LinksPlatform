namespace Platform.Helpers
{
    public interface IFactory<TProduct>
    {
        TProduct Create();
    }
}
