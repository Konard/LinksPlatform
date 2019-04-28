namespace Platform.Helpers
{
    public interface IProvider<TProvided>
    {
        TProvided Get();
    }

    public interface IProvider<TProvided, TArgument>
    {
        TProvided Get(TArgument argument);
    }
}
