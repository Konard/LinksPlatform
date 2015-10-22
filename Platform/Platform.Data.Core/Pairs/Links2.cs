using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data.Core.Exceptions;
using Platform.Data.Core.Pairs;
using Platform.Data.Core.Structures;

namespace Platform.Data.Core.Pairs
{
    public class Links : ILinks<ILink>
    {
        public Links()
        {
            _links = new HashSet<ILink>();
            _linksBySource = new Dictionary<ILink, HashSet<ILink>>();
            _linksByTarget = new Dictionary<ILink, HashSet<ILink>>();
        }

        public ILink Create(ILink source, ILink target)
        {
            var link = new Link(this, source, target);
            _links.Add(link);
            _linksBySource.Add(link, new HashSet<ILink>());
            _linksByTarget.Add(link, new HashSet<ILink>());
            _linksBySource[source].Add(link);
            _linksByTarget[target].Add(link);
            return link;
        }

        public void Delete(ILink link)
        {
            if (!_links.Contains(link))
                throw new ArgumentException("Связь не находится в этом хранилище.");

            _links.Remove(link);
            _linksBySource[link.Source].Remove(link);
            _linksByTarget[link.Target].Remove(link);
            _linksBySource.Remove(link);
            _linksByTarget.Remove(link);
        }

        public bool Exists(ILink link)
        {
            return _links.Contains(link);
        }

        public bool Each(ILink source, ILink target, Func<ILink, bool> handler)
        {
            if (source != null && !Exists(source))
                throw new ArgumentLinkDoesNotExistsException<ILink>(source, "source");
            if (target != null && !Exists(target))
                throw new ArgumentLinkDoesNotExistsException<ILink>(target, "target");

            if (source == null && target == null)
                return _links.All(link => handler(link) != Break);
            if (source == null)
            {
                HashSet<ILink> linksByTargetSet;
                if (!_linksByTarget.TryGetValue(target, out linksByTargetSet))
                    return true;

                return linksByTargetSet.All(link => handler(link) != Break);
            }
            if (target == null)
            {
                HashSet<ILink> linksBySourceSet;
                if (!_linksBySource.TryGetValue(source, out linksBySourceSet))
                    return true;

                return linksBySourceSet.All(link => handler(link) != Break);
            }
            {
                ILink link = SearchCore(source, target);
                if (link != null && handler(link) == Break)
                    return false;
            }

            return true;
        }

        public ulong Total
        {
            get { return (ulong) _links.Count; }
        }

        public ILink GetSource(ILink link)
        {
            return link.Source;
        }

        public ILink GetTarget(ILink link)
        {
            return link.Target;
        }

        public ILink Search(ILink source, ILink target)
        {
            if (source == null || !Exists(source))
                throw new ArgumentLinkDoesNotExistsException<ILink>(source, "source");
            if (target == null || !Exists(target))
                throw new ArgumentLinkDoesNotExistsException<ILink>(target, "target");

            return SearchCore(source, target);
        }

        public CoreUnsafe.Structures.Link GetLink(ILink link)
        {
            return CoreUnsafe.Structures.Link.Create(link);
        }

        public ILink Update(ILink link, ILink newSource, ILink newTarget)
        {
            if (!_links.Contains(link))
                throw new ArgumentException("Связь не находится в этом хранилище.");

            Delete(link);
            return Create(newSource, newTarget);
        }

        private ILink SearchCore(ILink source, ILink target)
        {
            HashSet<ILink> linksBySourceSet;
            if (!_linksBySource.TryGetValue(source, out linksBySourceSet))
                return null;

            HashSet<ILink> linksByTargetSet;
            if (!_linksByTarget.TryGetValue(target, out linksByTargetSet))
                return null;

            HashSet<ILink> copy = linksBySourceSet.Count < linksByTargetSet.Count
                ? new HashSet<ILink>(linksBySourceSet)
                : new HashSet<ILink>(linksByTargetSet);

            copy.IntersectWith(linksByTargetSet);
            return copy.SingleOrDefault();
        }

        public class Link : ILink, IEquatable<Link>
        {
            #region Structure

            private readonly Links _links;
            private readonly ILink _source;
            private readonly ILink _target;

            #endregion

            #region Contructors

            public Link(Links links)
            {
                _links = links;
                _source = this;
                _target = this;
            }

            public Link(Links links, ILink source, ILink target)
            {
                _links = links;
                _source = source;
                _target = target;
            }

            #endregion

            #region Properties

            public ILink Source
            {
                get { return _source; }
            }

            public ILink Target
            {
                get { return _target; }
            }

            #endregion

            #region Methods

            public void WalkThroughReferersBySource(Action<ILink> walker)
            {
                foreach (ILink link in _links._linksBySource[this])
                    walker(link);
            }

            public void WalkThroughReferersByTarget(Action<ILink> walker)
            {
                foreach (ILink link in _links._linksByTarget[this])
                    walker(link);
            }

            public override string ToString()
            {
                return CoreUnsafe.Structures.Link.ToString(this);
            }

            #endregion

            #region IEquatable

            public bool Equals(Link other)
            {
                return Equals(other as ILink);
            }

            public bool Equals(ILink other)
            {
                if (other == null)
                    return false;

                return _source.Equals(other.Source) && _target.Equals(other.Target);
            }

            public override int GetHashCode()
            {
                // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
                return base.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as ILink);
            }

            #endregion
        }

        #region Constants

        /// <summary>Возвращает булевское значение, обозначающее продолжение прохода по связям.</summary>
        /// <remarks>Используется в функции обработчике, который передаётся в функцию Each.</remarks>
        public const bool Continue = true;

        /// <summary>Возвращает булевское значение, обозначающее остановку прохода по связям.</summary>
        /// <remarks>Используется в функции обработчике, который передаётся в функцию Each.</remarks>
        public const bool Break = false;

        /// <summary>Возвращает значение ulong, обозначающее отсутствие связи.</summary>
        public const ulong Null = 0;

        /// <summary>Возвращает значение ulong, обозначающее любую связь.</summary>
        /// <remarks>
        ///     Возможно нужно зарезервировать отдельное значение, тогда можно будет создавать все варианты
        ///     последовательностей в функции Create.
        /// </remarks>
        public const ulong Any = 0;

        #endregion

        #region Structure

        private readonly HashSet<ILink> _links;
        private readonly Dictionary<ILink, HashSet<ILink>> _linksBySource;
        private readonly Dictionary<ILink, HashSet<ILink>> _linksByTarget;

        #endregion
    }
}