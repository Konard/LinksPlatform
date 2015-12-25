using Platform.Data.Core.Collections.Lists;

namespace Platform.Data.Core.Pairs
{
    unsafe partial class LinksMemoryManager
    {
        private class UnusedLinksListMethods : CircularDoublyLinkedListMethods
        {
            private readonly Link* _links;
            private readonly LinksHeader* _header;

            public UnusedLinksListMethods(LinksMemoryManager links, LinksHeader* header)
            {
                _links = links._links;
                _header = header;
            }

            protected override ulong GetFirst()
            {
                return _header->FirstFreeLink;
            }

            protected override ulong GetLast()
            {
                return _header->LastFreeLink;
            }

            protected override ulong GetPrevious(ulong element)
            {
                return _links[element].Source;
            }

            protected override ulong GetNext(ulong element)
            {
                return _links[element].Target;
            }

            protected override ulong GetSize()
            {
                return _header->FreeLinks;
            }

            protected override void SetFirst(ulong element)
            {
                _header->FirstFreeLink = element;
            }

            protected override void SetLast(ulong element)
            {
                _header->LastFreeLink = element;
            }

            protected override void SetPrevious(ulong element, ulong previous)
            {
                _links[element].Source = previous;
            }

            protected override void SetNext(ulong element, ulong next)
            {
                _links[element].Target = next;
            }

            protected override void SetSize(ulong size)
            {
                _header->FreeLinks = size;
            }
        }
    }
}
