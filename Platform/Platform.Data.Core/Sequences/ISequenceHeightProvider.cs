namespace Platform.Data.Core.Sequences
{
    public interface ISequenceHeightProvider<TLink>
    {
        TLink GetHeight(TLink sequence);
    }
}
