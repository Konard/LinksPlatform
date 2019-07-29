using System;
using Platform.Unsafe;
using Platform.Collections.Methods.Lists;

namespace Platform.Data.Doublets.ResizableDirectMemory
{
    partial class ResizableDirectMemoryLinks<TLink>
    {
        private class UnusedLinksListMethods : CircularDoublyLinkedListMethods<TLink>
        {
            private readonly IntPtr _links;
            private readonly IntPtr _header;

            public UnusedLinksListMethods(IntPtr links, IntPtr header)
            {
                _links = links;
                _header = header;
            }

            protected override TLink GetFirst() => (_header + LinksHeader.FirstFreeLinkOffset).GetValue<TLink>();

            protected override TLink GetLast() => (_header + LinksHeader.LastFreeLinkOffset).GetValue<TLink>();

            protected override TLink GetPrevious(TLink element) => (_links.GetElement(LinkSizeInBytes, element) + Link.SourceOffset).GetValue<TLink>();

            protected override TLink GetNext(TLink element) => (_links.GetElement(LinkSizeInBytes, element) + Link.TargetOffset).GetValue<TLink>();

            protected override TLink GetSize() => (_header + LinksHeader.FreeLinksOffset).GetValue<TLink>();

            protected override void SetFirst(TLink element) => (_header + LinksHeader.FirstFreeLinkOffset).SetValue(element);

            protected override void SetLast(TLink element) => (_header + LinksHeader.LastFreeLinkOffset).SetValue(element);

            protected override void SetPrevious(TLink element, TLink previous) => (_links.GetElement(LinkSizeInBytes, element) + Link.SourceOffset).SetValue(previous);

            protected override void SetNext(TLink element, TLink next) => (_links.GetElement(LinkSizeInBytes, element) + Link.TargetOffset).SetValue(next);

            protected override void SetSize(TLink size) => (_header + LinksHeader.FreeLinksOffset).SetValue(size);
        }
    }
}
