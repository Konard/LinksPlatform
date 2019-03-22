using System;
using Platform.Data.Core.Collections.Lists;
using Platform.Helpers;

namespace Platform.Data.Core.Pairs
{
    partial class LinksMemoryManager<T>
    {
        private class UnusedLinksListMethods : CircularDoublyLinkedListMethods<T>
        {
            private readonly IntPtr _links;
            private readonly IntPtr _header;

            public UnusedLinksListMethods(IntPtr links, IntPtr header)
            {
                _links = links;
                _header = header;
            }

            protected override T GetFirst()
            {
                return (_header + LinksHeader.FirstFreeLinkOffset).GetValue<T>();
            }

            protected override T GetLast()
            {
                return (_header + LinksHeader.LastFreeLinkOffset).GetValue<T>();
            }

            protected override T GetPrevious(T element)
            {
                return (_links.GetElement(LinkSizeInBytes, element) + Link.SourceOffset).GetValue<T>();
            }

            protected override T GetNext(T element)
            {
                return (_links.GetElement(LinkSizeInBytes, element) + Link.TargetOffset).GetValue<T>();
            }

            protected override T GetSize()
            {
                return (_header + LinksHeader.FreeLinksOffset).GetValue<T>();
            }

            protected override void SetFirst(T element)
            {
                (_header + LinksHeader.FirstFreeLinkOffset).SetValue(element);
            }

            protected override void SetLast(T element)
            {
                (_header + LinksHeader.LastFreeLinkOffset).SetValue(element);
            }

            protected override void SetPrevious(T element, T previous)
            {
                (_links.GetElement(LinkSizeInBytes, element) + Link.SourceOffset).SetValue(previous);
            }

            protected override void SetNext(T element, T next)
            {
                (_links.GetElement(LinkSizeInBytes, element) + Link.TargetOffset).SetValue(next);
            }

            protected override void SetSize(T size)
            {
                (_header + LinksHeader.FreeLinksOffset).SetValue(size);
            }
        }
    }
}
