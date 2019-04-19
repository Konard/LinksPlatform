namespace Platform.Helpers
{
    public interface IIncrementer<T>
    {
        T Increment(T number);
    }
}
