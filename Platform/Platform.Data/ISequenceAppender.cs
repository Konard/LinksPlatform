namespace Platform.Data.Core.Sequences
{
    public interface ISequenceAppender<TLink>
    {
        TLink Append(TLink sequence, TLink appendant);
    }
}
