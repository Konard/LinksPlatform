using System;
using System.Collections.Generic;
using Platform.Links.DataBase.CoreUnsafe.Exceptions;
using Platform.Links.System.Helpers.Disposal;
using Platform.Links.System.Helpers.Synchronization;
using Platform.Links.System.Memory;

#pragma warning disable 0649
#pragma warning disable 169

namespace Platform.Links.DataBase.CoreUnsafe.Pairs
{
    /// <summary>
    /// Представляет объект для работы с базой данных (файлом) в формате Links (массива взаимосвязей).
    /// </summary>
    /// <remarks>
    /// 
    /// Заменить везде 0 на Null.
    /// 
    /// Возможные оптимизации:
    /// Объединение в одном поле Source и Target с уменьшением до 32 бит. 
    ///     + меньше объём БД
    ///     - меньше производительность
    ///     - больше ограничение на количество связей в БД)
    /// Ленивое хранение размеров поддеревьев (расчитываемое по мере использования БД)
    ///     + меньше объём БД
    ///     - больше сложность
    /// 
    ///     AVL - высота дерева может позволить точно расчитать размер дерева, нет необходимости в SBT.
    ///     AVL дерево можно прошить.
    /// 
    /// Текущее теоретическое ограничение на размер связей - long.MaxValue
    /// Желательно реализовать поддержку переключения между деревьями и битовыми индексами (битовыми строками) - вариант матрицы (выстраеваемой лениво).
    /// </remarks>
    public sealed unsafe partial class Links : DisposalBase, ILinks<ulong>
    {
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
        /// <remarks>Возможно нужно зарезервировать отдельное значение, тогда можно будет создавать все варианты последовательностей в функции Create.</remarks>
        public const ulong Any = 0;

        /// <summary>Возвращает размер одной связи в байтах.</summary>
        /// <remarks>
        /// Используется только во вне класса, не рекомедуется использовать внутри.
        /// Так как во вне не обязательно будет доступен unsafe С#.
        /// </remarks>
        public static readonly int LinkSizeInBytes = sizeof(Link);

        #endregion

        #region Low level structures

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

        #endregion

        #region Structure

        private readonly ISyncronization _sync = new UnsafeSynchronization();
        private readonly long _memoryReservationStep;

        private readonly IMemory _memory;
        private LinksHeader* _header;
        private Link* _links;
        private LinksTargetsTreeMethods _targetsTreeMethods;
        private LinksSourcesTreeMethods _sourcesTreeMethods;

        #endregion

        #region Properties

        /// <summary>
        /// Возвращает общее число связей находящихся в хранилище.
        /// </summary>
        public ulong Total
        {
            get { return _sync.ExecuteReadOperation(() => _header->AllocatedLinks - _header->FreeLinks); }
        }

        #endregion

        #region Links Logic

        /// <summary>
        /// Создаёт экземпляр базы данных Links в файле по указанному адресу, с указанным минимальным шагом расширения базы данных.
        /// </summary>
        /// <param name="address">Полный пусть к файлу базы данных.</param>
        /// <param name="size">Минимальный шаг расширения базы данных в байтах.</param>
        public Links(string address, long size)
        {
            _memoryReservationStep = size;
            _memory = new FileMappedMemory(address, _memoryReservationStep);

            UpdatePointers(_memory);

            // Гарантия корректности _memory.UsedCapacity относительно _header->AllocatedLinks
            _memory.UsedCapacity = (long)_header->AllocatedLinks * sizeof(Link) + sizeof(LinksHeader);

            // Гарантия корректности _header->ReservedLinks относительно _memory.ReservedCapacity
            _header->ReservedLinks = (ulong)((_memory.ReservedCapacity - sizeof(LinksHeader)) / sizeof(Link));
        }

        /// <summary>
        /// Возвращает индекс начальной (Source) связи для указанной связи.
        /// </summary>
        /// <param name="link">Индекс связи.</param>
        /// <returns>Индекс начальной связи для указанной связи.</returns>
        public ulong GetSource(ulong link)
        {
            return _sync.ExecuteReadOperation(() =>
            {
                if (!ExistsCore(link))
                    throw new ArgumentLinkDoesNotExistsException<ulong>(link);

                return GetSourceCore(link);
            });
        }

        private ulong GetSourceCore(ulong link)
        {
            // Связь "точка" не имеет начала и конца
            //if (_links[link].Target == Null)
            //    return 0;

            return _links[link].Source;
        }

        /// <summary>
        /// Возвращает индекс конечной (Target) связи для указанной связи.
        /// </summary>
        /// <param name="link">Индекс связи.</param>
        /// <returns>Индекс конечной связи для указанной связи.</returns>
        public ulong GetTarget(ulong link)
        {
            return _sync.ExecuteReadOperation(() =>
            {
                if (!ExistsCore(link))
                    throw new ArgumentLinkDoesNotExistsException<ulong>(link);

                return GetTargetCore(link);
            });
        }

        public ulong GetTargetCore(ulong link)
        {
            return _links[link].Target;
        }

        /// <summary>
        /// Возвращает уникальную связь для указанного индекса.
        /// </summary>
        /// <param name="link">Индекс связи.</param>
        /// <returns>Уникальную связь.</returns>
        public Structures.Link GetLink(ulong link)
        {
            return _sync.ExecuteReadOperation(() =>
            {
                if (!ExistsCore(link))
                    throw new ArgumentLinkDoesNotExistsException<ulong>(link);

                return GetLinkCore(link);
            });
        }

        public Structures.Link GetLinkCore(ulong link)
        {
            return new Structures.Link(GetSourceCore(link), GetTargetCore(link));
        }

        /// <summary>
        /// Возвращает значение, определяющее существует ли связь с указанным индексом в базе данных.
        /// </summary>
        /// <param name="link">Индекс проверяемой на существование связи.</param>
        /// <returns>Значение, определяющее существует ли связь.</returns>
        public bool Exists(ulong link)
        {
            return _sync.ExecuteReadOperation(() => ExistsCore(link));
        }

        private bool ExistsCore(ulong link)
        {
            return link != Null && !IsUnusedLink(link) && link <= _header->AllocatedLinks;
        }

        /// <summary>
        /// Выполняет поиск связи с указанными Source (началом) и Target (концом)
        /// </summary>
        /// <param name="source">Индекс связи, которая является началом для искомой связи.</param>
        /// <param name="target">Индекс связи, которая является концом для искомой связи.</param>
        /// <returns>Индекс искомой связи с указанными Source (началом) и Target (концом)</returns>
        public ulong Search(ulong source, ulong target)
        {
            return _sync.ExecuteReadOperation(() =>
            {
                if (source == Null || !ExistsCore(source))
                    throw new ArgumentLinkDoesNotExistsException<ulong>(source, "source");
                if (target == Null || !ExistsCore(target))
                    throw new ArgumentLinkDoesNotExistsException<ulong>(target, "target");

                return _sourcesTreeMethods.Search(source, target);
            });
        }

        private ulong SearchCore(ulong source, ulong target)
        {
            return _sourcesTreeMethods.Search(source, target);
        }

        public ulong CalculateReferences(ulong link)
        {
            return _sync.ExecuteReadOperation(() =>
            {
                if (link == Null || !ExistsCore(link))
                    throw new ArgumentLinkDoesNotExistsException<ulong>(link, "link");

                return CalculateReferencesCore(link);
            });
        }

        private ulong CalculateReferencesCore(ulong link)
        {
            return _sourcesTreeMethods.CalculateReferences(link) + _targetsTreeMethods.CalculateReferences(link);
        }

        /// <summary>
        /// Выполняет проход по всем связям, соответствующим шаблону, вызывая обработчик (handler) для каждой подходящей связи.
        /// </summary>
        /// <param name="source">Значение, определяющее соответствующие шаблону связи. (0 - любое начало, 1..∞ конкретное начало)</param>
        /// <param name="target">Значение, определяющее соответствующие шаблону связи. (0 - любое конец, 1..∞ конкретный конец)</param>
        /// <param name="handler">Обработчик каждой подходящей связи.</param>
        /// <returns>True, в случае если проход по связям не был прерван и False в обратном случае.</returns>
        public bool Each(ulong source, ulong target, Func<ulong, bool> handler)
        {
            return _sync.ExecuteReadOperation(() =>
            {
                if (source != Null && !ExistsCore(source))
                    throw new ArgumentLinkDoesNotExistsException<ulong>(source, "source");
                if (target != Null && !ExistsCore(target))
                    throw new ArgumentLinkDoesNotExistsException<ulong>(target, "target");

                if (source == Null && target == Null)
                {
                    // Этот блок используется в GetEnumerator, CopyTo, Clear
                    for (ulong link = 1; link <= _header->AllocatedLinks; link++)
                        if (ExistsCore(link))
                            if (handler(link) == Break)
                                return false;
                }
                else if (source == Null)
                {
                    _targetsTreeMethods.EachReference(target, handler);
                }
                else if (target == Null)
                {
                    _sourcesTreeMethods.EachReference(source, handler);
                }
                else //if(source != Null && target != Null)
                {
                    var link = _sourcesTreeMethods.Search(source, target);

                    if (link != Null)
                        if (handler(link) == Break)
                            return false;
                }

                return true;
            });
        }

        /// <summary>
        /// Создаёт связь (если она не существовала), либо возвращает индекс существующей связи с указанными Source (началом) и Target (концом).
        /// </summary>
        /// <param name="source">Индекс связи, которая является началом на создаваемой связи.</param>
        /// <param name="target">Индекс связи, которая является концом для создаваемой связи.</param>
        /// <returns>Индекс связи, с указанным Source (началом) и Target (концом)</returns>
        public ulong Create(ulong source, ulong target)
        {
            return _sync.ExecuteWriteOperation(() =>
            {
                if (source != Null && !ExistsCore(source))
                    throw new ArgumentLinkDoesNotExistsException<ulong>(source, "source");
                if (target != Null && !ExistsCore(target))
                    throw new ArgumentLinkDoesNotExistsException<ulong>(target, "target");

                if (source != Null && target != Null)
                {
                    var linkIndex = _sourcesTreeMethods.Search(source, target);

                    if (linkIndex == Null)
                    {
                        linkIndex = AllocateLink();
                        var link = &_links[linkIndex];

                        link->Source = source;
                        link->Target = target;

                        _sourcesTreeMethods.AddUnsafe(linkIndex, &_header->FirstAsSource);
                        _targetsTreeMethods.AddUnsafe(linkIndex, &_header->FirstAsTarget);
                    }

                    return linkIndex;
                }
                else
                {
                    var linkIndex = AllocateLink();
                    var link = &_links[linkIndex];

                    link->Source = source == Null ? linkIndex : source;
                    link->Target = target == Null ? linkIndex : target;

                    _sourcesTreeMethods.AddUnsafe(linkIndex, &_header->FirstAsSource);
                    _targetsTreeMethods.AddUnsafe(linkIndex, &_header->FirstAsTarget);

                    return linkIndex;
                }

                throw new Exception("Невозможно");
            });
        }

        public ulong Update(ulong prevLink, ulong newLink)
        {
            return Update(GetSource(prevLink), GetTarget(prevLink), GetSource(newLink), GetTarget(newLink));
        }

        /// <summary>
        /// Обновляет связь с указанными началом (Source) и концом (Target)
        /// на связь с указанными началом (NewSource) и концом (NewTarget).
        /// </summary>
        /// <param name="source">Индекс связи, которая является началом обновляемой связи.</param>
        /// <param name="target">Индекс связи, которая является концом обновляемой связи.</param>
        /// <param name="newSource">Индекс связи, которая является началом связи, на которую выполняется обновление.</param>
        /// <param name="newTarget">Индекс связи, которая является концом связи, на которую выполняется обновление.</param>
        /// <returns>Индекс обновлённой связи.</returns>
        public ulong Update(ulong source, ulong target, ulong newSource, ulong newTarget)
        {
            return _sync.ExecuteWriteOperation(() =>
            {
                if (!ExistsCore(source))
                    throw new ArgumentLinkDoesNotExistsException<ulong>(source, "source");
                if (!ExistsCore(target))
                    throw new ArgumentLinkDoesNotExistsException<ulong>(target, "target");
                if (!ExistsCore(newSource))
                    throw new ArgumentLinkDoesNotExistsException<ulong>(newSource, "newSource");
                if (!ExistsCore(newTarget))
                    throw new ArgumentLinkDoesNotExistsException<ulong>(newTarget, "newTarget");

                var linkIndex = SearchCore(source, target);

                if (linkIndex == Null)
                {
                    var x = Create(newSource, newTarget);
                    return x;
                }
                //throw new Exception(string.Format("Link with source {0} and target {1} is not exists.", source, target));

                if (newSource != newTarget &&
                    (newSource == Null || newSource == linkIndex || newTarget == Null || newTarget == linkIndex))
                    throw new Exception("Not passible.");

                if (newSource == source && newTarget == target)
                    return linkIndex;

                var newLink = SearchCore(newSource, newTarget);

                if (newLink == Null)
                {
                    _sourcesTreeMethods.RemoveUnsafe(linkIndex, &_header->FirstAsSource);
                    _targetsTreeMethods.RemoveUnsafe(linkIndex, &_header->FirstAsTarget);

                    _links[linkIndex].Source = newSource;
                    _links[linkIndex].Target = newTarget;

                    _sourcesTreeMethods.AddUnsafe(linkIndex, &_header->FirstAsSource);
                    _targetsTreeMethods.AddUnsafe(linkIndex, &_header->FirstAsTarget);
                }
                else
                {
                    var referencesAsSource = new List<ulong>();
                    var referencesAsTarget = new List<ulong>();

                    _sourcesTreeMethods.EachReference(linkIndex, x =>
                    {
                        referencesAsSource.Add(x);
                        return true;
                    });
                    _targetsTreeMethods.EachReference(linkIndex, x =>
                    {
                        referencesAsTarget.Add(x);
                        return true;
                    });

                    for (var i = 0; i < referencesAsSource.Count; i++)
                    {
                        var reference = referencesAsSource[i];
                        var referenceSource = _links[reference].Source;
                        var referenceTarget = _links[reference].Target;

                        // TODO: Заменить на UpdateCore
                        Update(referenceSource, referenceTarget, newLink, referenceTarget);
                    }

                    for (var i = 0; i < referencesAsTarget.Count; i++)
                    {
                        var reference = referencesAsTarget[i];
                        var referenceSource = _links[reference].Source;
                        var referenceTarget = _links[reference].Target;

                        // TODO: Заменить на UpdateCore
                        Update(referenceSource, referenceTarget, referenceSource, newLink);
                    }

                    DeleteCore(linkIndex);
                }

                return newLink;
            });
        }

        /// <summary>Удаляет связь с указанными началом (Source) и концом (Target).</summary>
        /// <param name="source">Индекс связи, которая является началом удаляемой связи.</param>
        /// <param name="target">Индекс связи, которая является концом удаляемой связи.</param>
        public void Delete(ulong source, ulong target)
        {
            _sync.ExecuteWriteOperation(() =>
            {
                var link = SearchCore(source, target);

                if (link != Null)
                    DeleteCore(link);
            });
        }

        /// <summary>Удаляет связь с указанным индексом.</summary>
        /// <param name="link">Индекс удаляемой связи.</param>
        public void Delete(ref ulong link)
        {
            var linkCopy = link;

            _sync.ExecuteWriteOperation(() =>
            {
                if (linkCopy != Null && Exists(linkCopy))
                    DeleteCore(linkCopy);
            });

            link = 0;
        }

        /// <summary>Удаляет связь с указанным индексом.</summary>
        /// <param name="link">Индекс удаляемой связи.</param>
        /// <remarks>Версия функции без дополнительных проверок для ускорения работы рекурсии.</remarks>
        private void DeleteCore(ulong link)
        {
            if (ExistsCore(link))
            {
                _sourcesTreeMethods.RemoveUnsafe(link, &_header->FirstAsSource);
                _targetsTreeMethods.RemoveUnsafe(link, &_header->FirstAsTarget);

                var references = new List<ulong>();

                _sourcesTreeMethods.EachReference(link, x =>
                {
                    references.Add(x);
                    return true;
                });
                _targetsTreeMethods.EachReference(link, x =>
                {
                    references.Add(x);
                    return true;
                });

                references.ForEach(DeleteCore);

                _links[link].Source = 0;
                _links[link].Target = 0;

                FreeLink(link);
            }
        }

        #endregion

        #region Low Level FileMappedMemory Management

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
            if (_header->FirstFreeLink == Null)
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
        /// Выделяет следующую свободную связь и возвращает её индекс.
        /// </summary>
        /// <returns>Индекс свободной связи.</returns>
        private ulong AllocateLink()
        {
            var freeLink = _header->FirstFreeLink;

            if (freeLink != Null)
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
        /// Проверяет находится ли связь в списке свободных связей.
        /// </summary>
        /// <param name="link">Индекс проверяемой связи.</param>
        /// <returns>Значение, определяющие включена ли связь в список свободных связей.</returns>
        private bool IsUnusedLink(ulong link)
        {
            return _header->FirstFreeLink == link
                   || (_links[link].SizeAsSource == Null && (_links[link].Source != Null));
        }

        /// <summary>
        /// Высвобождает используемое связью пространство.
        /// </summary>
        /// <param name="link">Индекс высвобождаемой связи.</param>
        private void FreeLink(ulong link)
        {
            if (link == _header->AllocatedLinks)
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
            else
            {
                AttachToFreeLinkList(link);
            }
        }

        #endregion

        #region DisposalBase

        protected override void DisposeCore(bool manual)
        {
            _links = null;
            _memory.Dispose();
        }

        #endregion
    }
}