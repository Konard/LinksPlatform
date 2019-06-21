using System;
using System.Collections.Concurrent;
using System.Reflection;
using Platform.Helpers.Collections;
using Platform.Helpers.Reflection;

namespace Platform.Helpers
{
    public static class Singleton
    {
        public static Singleton<T> Create<T>(Func<T> creator) => new Singleton<T>(creator);

        public static Singleton<T> Create<T>(IFactory<T> factory) => new Singleton<T>(factory.Create);

        public static T Get<T>(Func<T> creator) => new Singleton<T>(creator).Instance;

        public static T Get<T>(IFactory<T> factory) => new Singleton<T>(factory.Create).Instance;
    }

    public struct Singleton<T>
    {
        private static readonly ConcurrentDictionary<Func<T>, byte[]> Functions = new ConcurrentDictionary<Func<T>, byte[]>();
        private static readonly ConcurrentDictionary<byte[], T> Singletons = new ConcurrentDictionary<byte[], T>(Default<IListEqualityComparer<byte>>.Instance);

        private readonly Func<T> _creator;

        public Singleton(Func<T> creator)
        {
            _creator = creator;
        }

        public T Instance
        {
            get
            {
                var creatorCopy = _creator;
                var bytes = Functions.GetOrAdd(creatorCopy, creatorCopy.GetMethodInfo().GetILBytes());
                return Singletons.GetOrAdd(bytes, key => creatorCopy());
            }
        }
    }
}
