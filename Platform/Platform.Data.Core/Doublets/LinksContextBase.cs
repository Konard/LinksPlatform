using Platform.Helpers;
using Platform.Helpers.Disposables;
using Platform.Memory;

namespace Platform.Data.Core.Doublets
{
    // Решить, нужна ли строго типизированная версия, или достаточно использование Scope?
    internal static class LinksContextHelpers
    {
        public static ILinksContext<TLink, TConstants, TMemory, TLinks> Create<TLink, TConstants, TMemory, TLinks>()
            where TConstants : LinksConstants<TLink>, new()
            where TMemory : class, IMemory
            where TLinks : class, ILinks<TLink>
            where TLink : struct
        {
            return new DefaultLinksContext<TLink, TConstants, TMemory, TLinks>();
        }

        //public static void Test()
        //{
        //    Create<>()
        //}
    }

    internal class DefaultLinksContext<TLink, TConstants, TMemory, TLinks> :
        LinksContextBase<TLink, TConstants, TMemory, TLinks>
        where TConstants : LinksConstants<TLink>, new()
        where TMemory : class, IMemory
        where TLinks : class, ILinks<TLink>
        where TLink : struct
    {
    }

    internal abstract class LinksContextBase<TLink, TConstants, TMemory, TLinks> : DisposableBase, ILinksContext<TLink, TConstants, TMemory, TLinks>
        where TConstants : LinksConstants<TLink>, new()
        where TMemory : class, IMemory
        where TLinks : class, ILinks<TLink>
        where TLink : struct
    {
        public TConstants Constants { get; }
        public TMemory Memory { get; protected set; }
        public TLinks Links { get; protected set; }

        public LinksContextBase()
        {
            Constants = Default<TConstants>.Instance;

            //var factory = new DefaultLinksFactory<TLink>();

            //Links = factory.Create();
        }

        public LinksContextBase(TMemory memory, TLinks links)
        {
            Constants = Default<TConstants>.Instance;
            Memory = memory;
            Links = links;
        }

        protected override void DisposeCore(bool manual, bool wasDisposed)
        {
            Disposable.TryDispose(Links);
            Disposable.TryDispose(Memory);
        }
    }

    internal abstract class UInt8LinksContextBase<TMemory, TLinks> : LinksContextBase<byte, LinksConstants<byte, sbyte>, TMemory, TLinks>
        where TMemory : class, IMemory
        where TLinks : class, ILinks<byte>
    {
    }

    internal abstract class UInt16LinksContextBase<TMemory, TLinks> : LinksContextBase<ushort, LinksConstants<ushort, short>, TMemory, TLinks>
        where TMemory : class, IMemory
        where TLinks : class, ILinks<ushort>
    {
    }

    internal abstract class UInt32LinksContextBase<TMemory, TLinks> : LinksContextBase<uint, LinksConstants<uint, int>, TMemory, TLinks>
        where TMemory : class, IMemory
        where TLinks : class, ILinks<uint>
    {
    }

    internal abstract class UInt64LinksContextBase<TMemory, TLinks> : LinksContextBase<ulong, LinksConstants<ulong, long>, TMemory, TLinks>
        where TMemory : class, IMemory
        where TLinks : class, ILinks<ulong>
    {
    }
}
