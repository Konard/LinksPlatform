using System.Collections.Generic;

namespace Platform.Data.Core.Sequences
{
    public interface ISequenceWalker<TLink>
    {
        IEnumerable<IList<TLink>> Walk(TLink sequence);
    }
}
