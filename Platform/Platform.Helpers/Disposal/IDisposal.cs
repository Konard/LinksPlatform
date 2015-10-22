using System;

namespace Platform.Helpers.Disposal
{
    public interface IDisposal : IDisposable
    {
        bool Disposed { get; }
        void Destruct();
    }
}