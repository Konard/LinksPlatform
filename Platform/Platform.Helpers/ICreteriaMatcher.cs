namespace Platform.Helpers
{
    public interface ICreteriaMatcher<TArgument>
    {
        bool IsMatched(TArgument argument);
    }
}
