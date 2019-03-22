/*using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data.Core.Exceptions;
using Platform.Helpers;

namespace Platform.Data.Core.Pairs
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// TODO: Переписать реализацию с использованием ILinksMemoryManager
    /// TODO: Реализовать Update.
    /// </remarks>
    public class Links2 : ILinks<ILink>
    {
        private static readonly LinksConstants LinksConstants = Default<LinksConstants>.Instance;

        public Links2()
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

        public ILink Update(ILink source, ILink target, ILink newSource, ILink newTarget)
        {
            throw new NotImplementedException();
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

        public ulong Count(params ILink[] restrictions)
        {
            if (restrictions.Length == 0)
                return (ulong)_links.Count;

            throw new NotImplementedException();
        }

        public bool Each(ILink source, ILink target, Func<ILink, bool> handler)
        {
            if (source != null && !Exists(source))
                throw new ArgumentLinkDoesNotExistsException<ILink>(source, "source");
            if (target != null && !Exists(target))
                throw new ArgumentLinkDoesNotExistsException<ILink>(target, "target");

            if (source == null && target == null)
                return _links.All(link => handler(link) != LinksConstants.Break);
            else if (source == null)
            {
                HashSet<ILink> linksByTargetSet;
                if (!_linksByTarget.TryGetValue(target, out linksByTargetSet))
                    return LinksConstants.Continue;

                return linksByTargetSet.All(link => handler(link) != LinksConstants.Break);
            }
            else if (target == null)
            {
                HashSet<ILink> linksBySourceSet;
                if (!_linksBySource.TryGetValue(source, out linksBySourceSet))
                    return LinksConstants.Continue;

                return linksBySourceSet.All(link => handler(link) != LinksConstants.Break);
            }
            else 
            {
                var link = SearchCore(source, target);
                if (link != null && handler(link) == LinksConstants.Break)
                    return LinksConstants.Break;
            }

            return LinksConstants.Continue;
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

        public IList<ILink> GetLink(ILink link)
        {
            return Pairs.Link.Create(link);
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

            var copy = linksBySourceSet.Count < linksByTargetSet.Count
                ? new HashSet<ILink>(linksBySourceSet)
                : new HashSet<ILink>(linksByTargetSet);

            copy.IntersectWith(linksByTargetSet);
            return copy.SingleOrDefault();
        }

        public class Link : ILink, IEquatable<Link>
        {
            #region Structure

            private readonly Links2 _links;
            private readonly ILink _source;
            private readonly ILink _target;

            #endregion

            #region Contructors

            public Link(Links2 links)
            {
                _links = links;
                _source = this;
                _target = this;
            }

            public Link(Links2 links, ILink source, ILink target)
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

            public void WalkThroughReferersAsSource(Action<ILink> walker)
            {
                foreach (var link in _links._linksBySource[this])
                    walker(link);
            }

            public void WalkThroughReferersAsTarget(Action<ILink> walker)
            {
                foreach (var link in _links._linksByTarget[this])
                    walker(link);
            }

            public override string ToString()
            {
                return Pairs.Link.ToString(this);
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

        #region Structure

        private readonly HashSet<ILink> _links;
        private readonly Dictionary<ILink, HashSet<ILink>> _linksBySource;
        private readonly Dictionary<ILink, HashSet<ILink>> _linksByTarget;

        #endregion
    }
}*/