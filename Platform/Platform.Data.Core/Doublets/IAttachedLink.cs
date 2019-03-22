using System;

namespace Platform.Data.Core.Doublets
{
    /// <summary>
    /// Represents a link, that is attached to corresponding links storage.
    /// Представляет связь, прекреплённую к соответствующему хранилищу связей.
    /// </summary>
    public interface IAttachedLink : IEquatable<IAttachedLink>
    {
        IAttachedLink Source { get; }
        IAttachedLink Target { get; }
    }
}