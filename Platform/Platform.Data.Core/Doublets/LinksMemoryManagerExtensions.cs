using System;
using System.Runtime.CompilerServices;
using Platform.Helpers;

namespace Platform.Data.Core.Doublets
{
    public static class LinksMemoryManagerExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Each<TLink>(this ILinksMemoryManager<TLink> memoryManager, Func<TLink, bool> handler, params TLink[] restrictions) => memoryManager.Each(handler, restrictions);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink Count<TLink>(this ILinksMemoryManager<TLink> memoryManager, params TLink[] restrictions) => memoryManager.Count(restrictions);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLinkValue<TLink>(this ILinksMemoryManager<TLink> memoryManager, params TLink[] parts) => memoryManager.SetLinkValue(parts);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink SearchOrDefault<TLink>(this ILinksMemoryManager<TLink> memoryManager, TLink source, TLink target)
        {
            var setter = new Setter<TLink>();
            memoryManager.Each(setter.SetAndReturnFalse, memoryManager.Constants.Any, source, target);
            return setter.Result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T CreatePoint<T>(this ILinksMemoryManager<T> memoryManager)
        {
            var link = memoryManager.AllocateLink();
            memoryManager.SetLinkValue(link, link, link);
            return link;
        }
    }
}
