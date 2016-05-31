namespace Platform.Helpers
{
    public interface IFactory<T>
    {
        T Create();
    }
}
