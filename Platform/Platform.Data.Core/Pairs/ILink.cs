using System;

namespace Platform.Data.Core.Pairs
{
    public interface ILink : IEquatable<ILink>
    {
        ILink Source { get; }
        ILink Target { get; }

        void WalkThroughReferersBySource(Action<ILink> walker);
        void WalkThroughReferersByTarget(Action<ILink> walker);
    }
}