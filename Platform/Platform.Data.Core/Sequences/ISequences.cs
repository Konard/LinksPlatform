using System;

namespace Platform.Data.Core.Sequences
{
    public interface ISequences<TLink>
    {
        ulong Count(params TLink[] sequence);
        bool Each(Func<TLink, bool> handler, params TLink[] sequence);
        bool EachPart(Func<TLink, bool> handler, TLink sequence);
        TLink Create(params TLink[] sequence);
        TLink Update(TLink[] sequence, TLink[] newSequence);
        void Delete(params TLink[] sequence);
    }
}
