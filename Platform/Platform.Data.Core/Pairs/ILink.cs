using System;

namespace Platform.Data.Core.Pairs
{
    public interface ILink : IEquatable<ILink>
    {
        ILink Source { get; }
        ILink Target { get; }
    }
}