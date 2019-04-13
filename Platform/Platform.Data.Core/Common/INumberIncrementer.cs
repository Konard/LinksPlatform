namespace Platform.Data.Core.Common
{
    public interface INumberIncrementer<TLink>
    {
        TLink Increment(TLink number);
    }
}
