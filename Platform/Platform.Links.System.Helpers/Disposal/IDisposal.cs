using System;

namespace Platform.Links.System.Helpers.Disposal
{
    public interface IDisposal : IDisposable
    {
        void Destruct();
    }
}