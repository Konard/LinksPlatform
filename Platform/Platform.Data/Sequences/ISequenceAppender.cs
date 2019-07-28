namespace Platform.Data.Sequences
{
    public interface ISequenceAppender<TLink>
    {
        TLink Append(TLink sequence, TLink appendant);
    }
}
