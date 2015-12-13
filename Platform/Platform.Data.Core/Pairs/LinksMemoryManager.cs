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
            public ulong LastFreeLink;
            public ulong Reserved8;
        }

        private readonly long _memoryReservationStep;

        private readonly IMemory _memory;
        private LinksHeader* _header;
        private Link* _links;

        private LinksTargetsTreeMethods _targetsTreeMethods;
        private LinksSourcesTreeMethods _sourcesTreeMethods;

        // TODO: Возможно чтобы гарантированно проверять на то, является ли связь удалённой, нужно
        // TODO: использовать не список а дерево, так как так можно быстрее проверить на наличие связи внутри
        private UnusedLinksListMethods _unusedLinksListMethods;

        /// <summary>
        /// Возвращает общее число связей находящихся в хранилище.
        /// </summary>
        private ulong Total
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

        public ulong Count(params ulong[] restrictions)
        {
            if (restrictions.Length == 0) // нет ограничений
            {
                // Общее число связей находящихся в хранилище.
                return Total;
            }
            if (restrictions.Length == 1)
            {
                // TODO: Подумать на тему объединения с Exists, возможно нужно две функции Count и CountReferences
                // TODO: Или же нужно добавить 0-е ограничение отвечающие именно за индекс, тогда можно учитывать одновременно и адрес и содержимое (и можно полностью заменить Exists)
                // TODO: Ещё один эквалент для Exists это указание всех значений содержимого (например и Source и Target)
                // Сколько есть всего ссылок на эту конкретную связь?
                var link = restrictions[0];
                //if (link == LinksConstants.Null) return 0; // На нулевую связь (пустую) никто никогда не ссылается
                if (link == LinksConstants.Any) return Total; // Null - как отсутствие ограничения
                return _sourcesTreeMethods.CalculateReferences(link) + _targetsTreeMethods.CalculateReferences(link);
            }
            if (restrictions.Length == 2)
            {
                //TODO: var id/index возможно в будущем нужно будет учитывать одновременное ограничение по трём параметрам (это важно чтобы отличать пары от точек)
                var source = restrictions[LinksConstants.SourcePart];
                var target = restrictions[LinksConstants.TargetPart];

                if (source == LinksConstants.Any && target == LinksConstants.Any)
                {
                    return Total;
                }
                else if (source == LinksConstants.Any)
                {
                    return _targetsTreeMethods.CalculateReferences(target);
                }
                else if (target == LinksConstants.Any)
                {
                    return _sourcesTreeMethods.CalculateReferences(source);
                }
                else //if(source != Null && target != Null)
                {
                    // Эквивалент Exists(source, target) => Count(source, target) > 0
                    var link = _sourcesTreeMethods.Search(source, target);
                    return link != LinksConstants.Null ? 1UL : 0UL;
                }
            }
            throw new NotSupportedException("Другие размеры и способы ограничений не поддерживаются.");
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
                _unusedLinksListMethods.Detach(freeLink);
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
                _unusedLinksListMethods.AttachAsFirst(link);
            }
            else if (link == _header->AllocatedLinks)
            {
                _header->AllocatedLinks--;
                _memory.UsedCapacity -= sizeof(Link);

                // Убираем все связи, находящиеся в списке свободных в конце файла, до тех пор, пока не дойдём до первой существующей связи
                // Позволяет оптимизировать количество выделенных связей (AllocatedLinks)
                while (_header->AllocatedLinks > 0 && IsUnusedLink(_header->AllocatedLinks))
                {
                    _unusedLinksListMethods.Detach(_header->AllocatedLinks);

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
            _unusedLinksListMethods = new UnusedLinksListMethods(this, _header);
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
