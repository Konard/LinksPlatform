using Platform.Helpers;
using Platform.Helpers.Disposables;
using Platform.Memory;

namespace Platform.Data.Core.Doublets
{
    // Решить, нужна ли строго типизированная версия, или достаточно использование Scope?
    public static class LinksContextHelpers
    {
        public static ILinksContext<TLink, TConstants, TMemory, TManager, TLinks> Create<TLink, TConstants, TMemory, TManager, TLinks>()
            where TConstants : LinksConstants<TLink>, new()
            where TMemory : class, IMemory
            where TManager : class, ILinksMemoryManager<TLink>
            where TLinks : class, ILinks<TLink>
            where TLink : struct
        {
            return new DefaultLinksContext<TLink, TConstants, TMemory, TManager, TLinks>();
        }

        //public static void Test()
        //{
        //    Create<>()
        //}
    }

    public class DefaultLinksContext<TLink, TConstants, TMemory, TManager, TLinks> :
        LinksContextBase<TLink, TConstants, TMemory, TManager, TLinks>
        where TConstants : LinksConstants<TLink>, new()
        where TMemory : class, IMemory
        where TManager : class, ILinksMemoryManager<TLink>
        where TLinks : class, ILinks<TLink>
        where TLink : struct
    {
    }

    public abstract class LinksContextBase<TLink, TConstants, TMemory, TManager, TLinks> : DisposableBase, ILinksContext<TLink, TConstants, TMemory, TManager, TLinks>
        where TConstants : LinksConstants<TLink>, new()
        where TMemory : class, IMemory
        where TManager : class, ILinksMemoryManager<TLink>
        where TLinks : class, ILinks<TLink>
        where TLink : struct
    {
        public TConstants Constants { get; }
        public TMemory Memory { get; protected set; }
        public TManager MemoryManager { get; protected set; }
        public TLinks Links { get; protected set; }

        public LinksContextBase()
        {
            Constants = Default<TConstants>.Instance;

            //var factory = new DefaultLinksFactory<TLink>();

            //Links = factory.Create();
        }

        public LinksContextBase(TMemory memory, TManager memoryManager, TLinks links)
        {
            Constants = Default<TConstants>.Instance;
            Memory = memory;
            MemoryManager = memoryManager;
            Links = links;
        }

        protected override void DisposeCore(bool manual)
        {
            Disposable.TryDispose(Links);
            Disposable.TryDispose(MemoryManager);
            Disposable.TryDispose(Memory);
        }
    }

    public abstract class UInt8LinksContextBase<TMemory, TManager, TLinks> : LinksContextBase<byte, LinksConstants<byte, sbyte>, TMemory, TManager, TLinks>
        where TMemory : class, IMemory
        where TManager : class, ILinksMemoryManager<byte>
        where TLinks : class, ILinks<byte>
    {
    }

    public abstract class UInt16LinksContextBase<TMemory, TManager, TLinks> : LinksContextBase<ushort, LinksConstants<ushort, short>, TMemory, TManager, TLinks>
        where TMemory : class, IMemory
        where TManager : class, ILinksMemoryManager<ushort>
        where TLinks : class, ILinks<ushort>
    {
    }

    public abstract class UInt32LinksContextBase<TMemory, TManager, TLinks> : LinksContextBase<uint, LinksConstants<uint, int>, TMemory, TManager, TLinks>
        where TMemory : class, IMemory
        where TManager : class, ILinksMemoryManager<uint>
        where TLinks : class, ILinks<uint>
    {
    }

    public abstract class UInt64LinksContextBase<TMemory, TManager, TLinks> : LinksContextBase<ulong, LinksConstants<ulong, long>, TMemory, TManager, TLinks>
        where TMemory : class, IMemory
        where TManager : class, ILinksMemoryManager<ulong>
        where TLinks : class, ILinks<ulong>
    {
    }
}
