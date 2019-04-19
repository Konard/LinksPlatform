namespace Platform.Helpers
{
    public interface ICounter<TResult>
    {
        TResult Count();
    }

    public interface ICounter<TArgument, TResult>
    {
        TResult Count(TArgument argument);
    }
}
