using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Platform.Interfaces;
using Platform.Disposables;
using Platform.Helpers.Reflection;
using Platform.Helpers.Collections.List;

namespace Platform.Helpers
{
    public class Scope : DisposableBase
    {
        private readonly bool _autoInclude;
        private readonly bool _autoExplore;
        private readonly Stack<object> _dependencies = new Stack<object>();

        private readonly HashSet<object> _excludes = new HashSet<object>();
        private readonly HashSet<object> _includes = new HashSet<object>();

        private readonly HashSet<object> _blocked = new HashSet<object>();

        private readonly Dictionary<Type, object> _resolutions = new Dictionary<Type, object>();

        public Scope()
        {
        }

        public Scope(bool autoInclude = false, bool autoExplore = false)
        {
            _autoInclude = autoInclude;
            _autoExplore = autoExplore;
        }

        #region Exclude

        public void ExcludeAssemblyOf<T>() => ExcludeAssemblyOfType(typeof(T));

        public void ExcludeAssemblyOfType(Type type) => ExcludeAssembly(type.GetAssembly());

        public void ExcludeAssembly(Assembly assembly) => assembly.GetCachedLoadableTypes().ForEach(Exclude);

        public void Exclude<T>() => Exclude(typeof(T));

        public void Exclude(object @object) => _excludes.Add(@object);

        #endregion

        #region Include

        public void IncludeAssemblyOf<T>() => IncludeAssemblyOfType(typeof(T));

        public void IncludeAssemblyOfType(Type type) => IncludeAssembly(type.GetAssembly());

        public void IncludeAssembly(Assembly assembly) => assembly.GetCachedLoadableTypes().ForEach(Include);

        public void Include<T>()
        {
            var types = Types.Get<T>();

            if (types.Length > 0)
                types.ForEach(Include);
            else
                Include(typeof(T));
        }

        public void Include(object @object)
        {
            if (@object == null)
                return;

            if (_includes.Add(@object))
            {
                var type = @object as Type;
                if (type == null)
                    return;
                type.GetInterfaces().ForEach(Include);
                Include(type.GetBaseType());
            }
        }

        #endregion

        #region Use

        /// <remarks>
        /// TODO: Use Default[T].Instance if the only constructor object has is parameterless.
        /// TODO: Think of interface chaining IDoubletLinks[T] (default) -> IDoubletLinks[T] (checker) -> IDoubletLinks[T] (synchronizer) (may be UseChain[IDoubletLinks[T], Types[DefaultLinks, DefaultLinksDependencyChecker, DefaultSynchronizedLinks]]
        /// TODO: Add support for factories
        /// </remarks>
        public T Use<T>()
        {
            if (_excludes.Contains(typeof(T)))
                throw new Exception($"Type {typeof(T).Name} is excluded and cannot be used.");

            if (_autoInclude)
                Include<T>();

            T resolved;
            if (!TryResolve(out resolved))
                throw new Exception($"Dependency of type {typeof(T).Name} cannot be resolved.");

            if (!_autoInclude)
                Include<T>();

            Use(resolved);
            return resolved;
        }

        public T UseSingleton<T>(IFactory<T> factory) => UseAndReturn(Singleton.Get(factory));

        public T UseSingleton<T>(Func<T> creator) => UseAndReturn(Singleton.Get(creator));

        public T UseAndReturn<T>(T @object)
        {
            Use(@object);
            return @object;
        }

        public void Use(object @object)
        {
            Include(@object);
            _dependencies.Push(@object);
        }

        #endregion

        #region Resolve

        public bool TryResolve<T>(out T resolved)
        {
            resolved = default(T);

            object resolvedObject;
            var result = TryResolve(typeof(T), out resolvedObject);

            if (result) resolved = (T)resolvedObject;
            return result;
        }

        public bool TryResolve(Type requiredType, out object resolved)
        {
            resolved = null;

            if (!_blocked.Add(requiredType))
                return false;

            try
            {
                if (_excludes.Contains(requiredType))
                    return false;

                if (_resolutions.TryGetValue(requiredType, out resolved))
                    return true;

                if (_autoExplore)
                    IncludeAssemblyOfType(requiredType);

                var resultInstances = new List<object>();
                var resultConstructors = new List<ConstructorInfo>();

                foreach (var include in _includes)
                {
                    if (_excludes.Contains(include))
                        continue;

                    var type = include as Type;

                    if (type != null)
                    {
                        if (requiredType.IsAssignableFrom(type))
                            resultConstructors.AddRange(GetValidConstructors(type));
                        else if (type.GetTypeInfo().IsGenericTypeDefinition && requiredType.GetTypeInfo().IsGenericType && type.GetInterfaces().Any(x => x.Name == requiredType.Name))
                        {
                            var genericType = type.MakeGenericType(requiredType.GenericTypeArguments);
                            if (requiredType.IsAssignableFrom(genericType))
                                resultConstructors.AddRange(GetValidConstructors(genericType));
                        }
                    }
                    else if (requiredType.IsInstanceOfType(include) || requiredType.IsAssignableFrom(include.GetType()))
                        resultInstances.Add(include);
                }

                if (resultInstances.Count == 0 && resultConstructors.Count == 0)
                    return false;
                else if (resultInstances.Count > 0)
                    resolved = resultInstances[0];
                else //if (resultConstructors.Count > 0)
                {
                    SortConstructors(resultConstructors);

                    if (!TryResolveInstance(resultConstructors, out resolved))
                        return false;
                }

                _resolutions.Add(requiredType, resolved);
                return true;
            }
            finally
            {
                _blocked.Remove(requiredType);
            }
        }

        protected virtual void SortConstructors(List<ConstructorInfo> resultConstructors)
        {
            resultConstructors.Sort((x, y) => -x.GetParameters().Length.CompareTo(y.GetParameters().Length));
        }

        protected virtual bool TryResolveInstance(List<ConstructorInfo> constructors, out object resolved)
        {
            for (var i = 0; i < constructors.Count; i++)
            {
                try
                {
                    var resultConstructor = constructors[i];

                    object[] arguments;
                    if (!TryResolveConstructorArguments(resultConstructor, out arguments))
                        continue;

                    resolved = resultConstructor.Invoke(arguments);
                    return true;
                }
                catch (Exception exception)
                {
                    Global.OnIgnoredException(exception);
                }
            }

            resolved = null;
            return false;
        }

        private ConstructorInfo[] GetValidConstructors(Type type)
        {
            var constructors = type.GetConstructors();

            if (!_autoExplore)
                constructors = constructors.ToArray(x =>
                {
                    var parameters = x.GetParameters();
                    for (var i = 0; i < parameters.Length; i++)
                        if (!_includes.Contains(parameters[i].ParameterType))
                            return false;
                    return true;
                });

            return constructors;
        }

        private bool TryResolveConstructorArguments(ConstructorInfo constructor, out object[] arguments)
        {
            var parameters = constructor.GetParameters();

            arguments = new object[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                object argument;
                if (!TryResolve(parameters[i].ParameterType, out argument))
                    return false;

                Use(argument);

                arguments[i] = argument;
            }

            return true;
        }

        #endregion

        protected override void DisposeCore(bool manual, bool wasDisposed)
        {
            while (_dependencies.Count > 0)
                Disposable.TryDispose(_dependencies.Pop());
        }
    }

    public class Scope<TInclude> : Scope
    {
        public Scope()
            : this(false, false)
        {
        }

        public Scope(bool autoInclude = false, bool autoExplore = false)
            : base(autoInclude, autoExplore)
        {
            Include<TInclude>();
        }
    }
}
