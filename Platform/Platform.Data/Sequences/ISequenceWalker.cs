using System.Collections.Generic;

namespace Platform.Data.Sequences
{
    public interface ISequenceWalker<TLink>
    {
        IEnumerable<IList<TLink>> Walk(TLink sequence);
    }
}
