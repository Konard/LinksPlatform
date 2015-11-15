using System;
using System.Runtime.CompilerServices;
using Platform.Helpers.Disposal;
using Platform.Memory;

#pragma warning disable 0649
#pragma warning disable 169

namespace Platform.Data.Core.Pairs
{
    public unsafe partial class LinksMemoryManager : DisposalBase, ILinksMemoryManager<ulong>
    {
        /// <summary>Возвращает размер одной связи в байтах.</summary>
        /// <remarks>
        /// Используется только во вне класса, не рекомедуется использовать внутри.
        /// Так как во вне не обязательно будет доступен unsafe С#.
        /// </remarks>
        public static readonly int LinkSizeInBytes = sizeof(Link);

        private struct Link
        {
            public ulong Source;
            public ulong Target;
            public ulong LeftAsSource;
            public ulong RightAsSource;
            public ulong SizeAsSource;
            public ulong LeftAsTarget;
            public ulong RightAsTarget;
            public ulong SizeAsTarget;
        }

        private struct LinksHeader
        {
            public ulong AllocatedLinks;
            public ulong ReservedLinks;
            public ulong FreeLinks;
            public ulong FirstFreeLink;
            public ulong FirstAsSource;
            public ulong FirstAsTarget;
            public ulong Reserved7;
            public ulong Reserved8;
        }

        private readonly long _memoryReservationStep;

        private readonly IMemory _memory;
        private LinksHeader* _header;
        private Link* _links;

        private LinksTargetsTreeMethods _targetsTreeMethods;
        private LinksSourcesTreeMethods _sourcesTreeMethods;

        /// <summary>
        /// Возвращает общее число связей находящихся в хранилище.
        /// </summary>
        public ulong Total
        {
            get { return _header->AllocatedLinks - _header->FreeLinks; }
        }

        /// <summary>
        /// Создаёт экземпляр базы данных Links в файле по указанному адресу, с указанным минимальным шагом расширения базы данных.
        /// </summary>
        /// <param name="address">Полный пусть к файлу базы данных.</param>
        /// <param name="size">Минимальный шаг расширения базы данных в байтах.</param>
        public LinksMemoryManager(string address, long size)
        {
            _memoryReservationStep = size;
            _memory = new FileMappedMemory(address, _memoryReservationStep);

            UpdatePointers(_memory);

            // Гарантия корректности _memory.UsedCapacity относительно _header->AllocatedLinks
            _memory.UsedCapacity = (long)_header->AllocatedLinks * sizeof(Link) + sizeof(LinksHeader);

            // Гарантия корректности _header->ReservedLinks относительно _memory.ReservedCapacity
            _header->ReservedLinks = (ulong)((_memory.ReservedCapacity - sizeof(LinksHeader)) / sizeof(Link));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists(ulong link)
        {
            return link != LinksConstants.Null && !IsUnusedLink(link) && link <= _header->AllocatedLinks;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong CalculateLinkTotalReferences(ulong link)
        {
            return _sourcesTreeMethods.CalculateReferences(link) + _targetsTreeMethods.CalculateReferences(link);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Each(Func<ulong, bool> handler, params ulong[] valuesRestriction)
        {
            var source = valuesRestriction[LinksConstants.SourcePart];
            var target = valuesRestriction[LinksConstants.TargetPart];
            if (source == LinksConstants.Any && target == LinksConstants.Any)
            {
                // Этот блок используется в GetEnumerator, CopyTo, Clear
                for (ulong link = 1; link <= _header->AllocatedLinks; link++)
                    if (Exists(link))
                        if (handler(link) == LinksConstants.Break)
                            return LinksConstants.Break;
            }
            else if (source == LinksConstants.Any)
            {
                return _targetsTreeMethods.EachReference(target, handler);
            }
            else if (target == LinksConstants.Any)
            {
                return _sourcesTreeMethods.EachReference(source, handler);
            }
            else //if(source != Null && target != Null)
            {
                var link = _sourcesTreeMethods.Search(source, target);

                if (link != LinksConstants.Null)
                    if (handler(link) == LinksConstants.Break)
                        return LinksConstants.Break;
            }
            return LinksConstants.Continue;
        }

        public void SetLinkValue(ulong linkIndex, params ulong[] values)
        {
            var link = &_links[linkIndex];

            // Будет корректно работать только в том случае, если пространство выделенной связи предварительно заполнено нулями
            if (link->Source != LinksConstants.Null) _sourcesTreeMethods.RemoveUnsafe(linkIndex, &_header->FirstAsSource);
            if (link->Target != LinksConstants.Null) _targetsTreeMethods.RemoveUnsafe(linkIndex, &_header->FirstAsTarget);

            link->Source = values[LinksConstants.SourcePart];
            link->Target = values[LinksConstants.TargetPart];

            if (link->Source != LinksConstants.Null) _sourcesTreeMethods.AddUnsafe(linkIndex, &_header->FirstAsSource);
            if (link->Target != LinksConstants.Null) _targetsTreeMethods.AddUnsafe(linkIndex, &_header->FirstAsTarget);
        }

        public ulong[] GetLinkValue(ulong linkIndex)
        {
            var values = new ulong[2];
            var link = &_links[linkIndex];

            values[LinksConstants.SourcePart] = link->Source;
            values[LinksConstants.TargetPart] = link->Target;

            return values;
        }

        /// <summary>
        /// Выделяет следующую свободную связь и возвращает её индекс.
        /// </summary>
        /// <returns>Индекс свободной связи.</returns>
        /// <remarks>TODO: Возможно нужно будет заполнение нулями, если внешнее API ими не заполняет пространство</remarks>
        public ulong AllocateLink()
        {
            var freeLink = _header->FirstFreeLink;

            if (freeLink != LinksConstants.Null)
                DetachFromFreeLinkList(freeLink);
            else
            {
                if (_header->AllocatedLinks == long.MaxValue)
                    throw new Exception(string.Format("Количество связей в базе данных не может превышать {0}.",
                        long.MaxValue));

                if (_header->AllocatedLinks >= (_header->ReservedLinks - 1))
                {
                    _memory.ReservedCapacity += _memoryReservationStep;
                    UpdatePointers(_memory);
                    _header->ReservedLinks = (ulong)(_memory.ReservedCapacity / sizeof(Link));
                }

                _header->AllocatedLinks++;
                _memory.UsedCapacity += sizeof(Link);
                freeLink = _header->AllocatedLinks;
            }

            return freeLink;
        }

        /// <summary>
        /// Высвобождает используемое связью пространство.
        /// </summary>
        /// <param name="link">Индекс высвобождаемой связи.</param>
        public void FreeLink(ulong link)
        {
            if (link < _header->AllocatedLinks)
            {
                AttachToFreeLinkList(link);
            }
            else if (link == _header->AllocatedLinks)
            {
                _header->AllocatedLinks--;
                _memory.UsedCapacity -= sizeof(Link);

                // Убираем все связи, находящиеся в списке свободных в конце файла, до тех пор, пока не дойдём до первой существующей связи
                // Позволяет оптимизировать количество выделенных связей (AllocatedLinks)
                while (_header->AllocatedLinks > 0 && IsUnusedLink(_header->AllocatedLinks))
                {
                    DetachFromFreeLinkList(_header->AllocatedLinks);

                    _header->AllocatedLinks--;
                    _memory.UsedCapacity -= sizeof(Link);
                }
            }
        }

        /// <summary>
        /// Выполняет обновление указателей на массив связей и заголовок базы данных.
        /// </summary>
        /// <param name="memory">Объект для работы с файлом как виртуальным блоком памяти.</param>
        private void UpdatePointers(IMemory memory)
        {
            _header = (LinksHeader*)memory.Pointer;

            // Указатель this.links может быть в том же месте, 
            // так как 0-я связь не используется и имеет такой же размер как Header,
            // поэтому header размещается в том же месте, что и 0-я связь
            _links = (Link*)memory.Pointer;

            _sourcesTreeMethods = new LinksSourcesTreeMethods(this, _header);
            _targetsTreeMethods = new LinksTargetsTreeMethods(this, _header);
        }

        // 3<1>2 1<2>3 2<3>1 V
        private void AttachToFreeLinkList(ulong link)
        {
            if (_header->FirstFreeLink == LinksConstants.Null)
            {
                _links[link].Source = link;
                _links[link].Target = link;

                _header->FirstFreeLink = link;
            }
            else
            {
                _links[link].Source = _links[_header->FirstFreeLink].Target;
                _links[link].Target = _header->FirstFreeLink;

                _links[_links[_header->FirstFreeLink].Target].Target = link;
                _links[_header->FirstFreeLink].Source = link;
            }

            _header->FreeLinks++;
        }

        private void DetachFromFreeLinkList(ulong freeLink)
        {
            if (_header->FirstFreeLink == freeLink)
            {
                if (_links[freeLink].Source == freeLink)
                {
                    _header->FirstFreeLink = 0;
                }
                else
                {
                    _links[_links[freeLink].Source].Target = _links[freeLink].Target;
                    _links[_links[freeLink].Target].Source = _links[freeLink].Source;

                    _header->FirstFreeLink = 0;
                }
            }
            else
            {
                _links[_links[freeLink].Source].Target = _links[freeLink].Target;
                _links[_links[freeLink].Target].Source = _links[freeLink].Source;
            }

            _header->FreeLinks--;
        }

        /// <summary>
        /// Проверяет находится ли связь в списке свободных связей.
        /// </summary>
        /// <param name="link">Индекс проверяемой связи.</param>
        /// <returns>Значение, определяющие включена ли связь в список свободных связей.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsUnusedLink(ulong link)
        {
            return _header->FirstFreeLink == link
                   || (_links[link].SizeAsSource == LinksConstants.Null && _links[link].Source != LinksConstants.Null);
        }

        protected override void DisposeCore(bool manual)
        {
            _links = null;
            if (_memory != null)
                _memory.Dispose();
        }
    }
}
