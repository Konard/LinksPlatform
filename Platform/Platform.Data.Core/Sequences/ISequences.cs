using System;

namespace Platform.Data.Core.Sequences
{
    internal interface ISequences<TLink>
    {
        ulong Count(params TLink[] sequence);
        bool Each(Func<TLink, bool> handler, params TLink[] sequence);
        bool EachPart(Func<TLink, bool> handler, TLink sequence);
        ulong Create(params TLink[] sequence);
        ulong Update(TLink[] sequence, TLink[] newSequence);
        void Delete(params TLink[] sequence);
    }
}
