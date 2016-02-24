using System;
using System.Runtime.CompilerServices;
using Platform.Data.Core.Exceptions;
using Platform.Helpers;
using Platform.Helpers.Disposal;
using Platform.Helpers.Threading;

#pragma warning disable 0649
#pragma warning disable 169

namespace Platform.Data.Core.Pairs
{
    /// <summary>
    /// Представляет объект для работы с базой данных (файлом) в формате Links (массива взаимосвязей).
    /// </summary>
    /// <remarks>
    /// TODO: Избавиться от анонимных функций передаваемых в ExecuteReadOperation и ExecureWriteOperation
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
    /// 
    /// Решить отключать ли проверки при компиляции под Release. Т.е. исключения будут выбрасываться только при #if DEBUG
    /// </remarks>
    public sealed partial class Links : DisposalBase, ILinks<ulong>
    {
        #region Structure

        public readonly ISyncronization Sync = new SafeSynchronization();
        private readonly ILinksMemoryManager<ulong> _memoryManager;

        #endregion

        #region Links Logic

        public Links(string address)
            : this(new LinksMemoryManager(address))
        {
        }

        public Links(ILinksMemoryManager<ulong> memoryManager)
        {
            _memoryManager = memoryManager;
        }

        /// <summary>
        /// Возвращает индекс начальной (Source) связи для указанной связи.
        /// </summary>
        /// <param name="link">Индекс связи.</param>
        /// <returns>Индекс начальной связи для указанной связи.</returns>
        public ulong GetSource(ulong link)
        {
            return Sync.ExecuteReadOperation(() =>
            {
                EnsureLinkExists(link);
                return GetSourceCore(link);
            });
        }

        /// <remarks>
        /// Использовать напрямую небезопасно, рекомендуется GetSource.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong GetSourceCore(ulong link)
        {
            return GetLinkCore(link).Source;
        }

        /// <summary>
        /// Возвращает индекс конечной (Target) связи для указанной связи.
        /// </summary>
        /// <param name="link">Индекс связи.</param>
        /// <returns>Индекс конечной связи для указанной связи.</returns>
        public ulong GetTarget(ulong link)
        {
            return Sync.ExecuteReadOperation(() =>
            {
                EnsureLinkExists(link);
                return GetTargetCore(link);
            });
        }

        /// <remarks>
        /// Использовать напрямую небезопасно, рекомендуется GetTarget.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong GetTargetCore(ulong link)
        {
            return GetLinkCore(link).Target;
        }

        /// <summary>
        /// Возвращает уникальную связь для указанного индекса.
        /// </summary>
        /// <param name="link">Индекс связи.</param>
        /// <returns>Уникальную связь.</returns>
        public Link GetLink(ulong link)
        {
            return Sync.ExecuteReadOperation(() =>
            {
                EnsureLinkExists(link);
                return GetLinkCore(link);
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Link GetLinkCore(ulong link)
        {
            return ((LinksMemoryManager)_memoryManager).GetLinkValueStruct(link);
        }

        /// <summary>
        /// Возвращает значение, определяющее существует ли связь с указанным индексом в базе данных.
        /// </summary>
        /// <param name="link">Индекс проверяемой на существование связи.</param>
        /// <returns>Значение, определяющее существует ли связь.</returns>
        public bool Exists(ulong link)
        {
            return Sync.ExecuteReadOperation(() => ExistsCore(link));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ExistsCore(ulong link)
        {
            return _memoryManager.Count(link) > 0;
        }

        /// <summary>
        /// Выполняет поиск связи с указанными Source (началом) и Target (концом)
        /// </summary>
        /// <param name="source">Индекс связи, которая является началом для искомой связи.</param>
        /// <param name="target">Индекс связи, которая является концом для искомой связи.</param>
        /// <returns>Индекс искомой связи с указанными Source (началом) и Target (концом)</returns>
        public ulong Search(ulong source, ulong target)
        {
            return Sync.ExecuteReadOperation(() =>
            {
                EnsureLinkExists(source, "source");
                EnsureLinkExists(target, "target");
                return SearchCore(source, target);
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong SearchCore(ulong source, ulong target)
        {
            return ((LinksMemoryManager)_memoryManager).Search(source, target);
        }

        public ulong Count(params ulong[] restrictions)
        {
            return Sync.ExecuteReadOperation(() => CountCore(restrictions));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong CountCore(params ulong[] restrictions)
        {
            return _memoryManager.Count(restrictions);
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
            return Sync.ExecuteReadOperation(() =>
            {
                EnsureLinkIsAnyOrExists(source, "source");
                EnsureLinkIsAnyOrExists(target, "target");
                return EachCore(source, target, handler);
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool EachCore(ulong source, ulong target, Func<ulong, bool> handler)
        {
            return _memoryManager.Each(handler, LinksConstants.Null, source, target);
        }

        /// <summary>
        /// Создаёт связь (если она не существовала), либо возвращает индекс существующей связи с указанными Source (началом) и Target (концом).
        /// </summary>
        /// <param name="source">Индекс связи, которая является началом на создаваемой связи.</param>
        /// <param name="target">Индекс связи, которая является концом для создаваемой связи.</param>
        /// <returns>Индекс связи, с указанным Source (началом) и Target (концом)</returns>
        public ulong Create(ulong source, ulong target)
        {
            return Sync.ExecuteWriteOperation(() =>
            {
                EnsureLinkIsItselfOrExists(source, "source");
                EnsureLinkIsItselfOrExists(target, "target");
                return CreateCore(source, target);
            });
        }

        public ulong CreateCore(ulong source, ulong target)
        {
            ulong linkIndex;

            if (source != LinksConstants.Null && target != LinksConstants.Null)
            {
                linkIndex = SearchCore(source, target);

                if (linkIndex == LinksConstants.Null)
                {
                    linkIndex = _memoryManager.AllocateLink();
                    _memoryManager.SetLinkValue(linkIndex, source, target);
                }
            }
            else
            {
                linkIndex = _memoryManager.AllocateLink();
                _memoryManager.SetLinkValue(linkIndex, source == LinksConstants.Null ? linkIndex : source,
                                                       target == LinksConstants.Null ? linkIndex : target);
            }

            CommitCreation(GetLinkCore(linkIndex));
            return linkIndex;
        }

        public ulong Update(ulong prevLink, ulong newLink)
        {
            return Update(GetSource(prevLink), GetTarget(prevLink), GetSource(newLink), GetTarget(newLink));
        }

        public ulong Update(ulong prevLink, ulong newSource, ulong newTarget)
        {
            return Update(GetSource(prevLink), GetTarget(prevLink), newSource, newTarget);
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
            return Sync.ExecuteWriteOperation(() =>
            {
                EnsureLinkExists(source, "source");
                EnsureLinkExists(target, "target");
                EnsureLinkIsItselfOrExists(newSource, "newSource");
                EnsureLinkIsItselfOrExists(newTarget, "newTarget");

                var linkIndex = SearchCore(source, target);
                if (linkIndex == LinksConstants.Null) // Может лучше исключение?
                    return Create(newSource, newTarget);
                if (newSource == source && newTarget == target)
                    return linkIndex;

                return UpdateCore(linkIndex, newSource, newTarget);
            });
        }

        public ulong UpdateCore(ulong linkIndex, ulong newSource, ulong newTarget)
        {
            // TODO: Исправить баг с newSource и newTarget (случай когда они равны Itself)
            var newLink = SearchCore(newSource, newTarget);

            if (newLink == LinksConstants.Null) // Actual update
            {
                var before = GetLinkCore(linkIndex);

                _memoryManager.SetLinkValue(linkIndex, newSource == LinksConstants.Itself ? linkIndex : newSource,
                    newTarget == LinksConstants.Itself ? linkIndex : newTarget);

                CommitUpdate(before, GetLinkCore(linkIndex));

                return linkIndex;
            }

            // Replace one link with another (replaced link is deleted, children are updated or deleted), it is actually merge operation
            return ReplaceCore(linkIndex, newLink);
        }

        public ulong ReplaceCore(ulong linkIndex, ulong newLink)
        {
            var referencesAsSourceCount = _memoryManager.Count(LinksConstants.Null, linkIndex, LinksConstants.Any);
            var referencesAsTargetCount = _memoryManager.Count(LinksConstants.Null, LinksConstants.Any, linkIndex);

            var references = new ulong[referencesAsSourceCount + referencesAsTargetCount];

            var referencesFiller = new ArrayFiller<ulong>(references);

            _memoryManager.Each(referencesFiller.AddAndReturnTrue, LinksConstants.Null, linkIndex, LinksConstants.Any);
            _memoryManager.Each(referencesFiller.AddAndReturnTrue, LinksConstants.Null, LinksConstants.Any, linkIndex);

            for (ulong i = 0; i < referencesAsSourceCount; i++)
            {
                var reference = references[i];
                if (reference == linkIndex) continue;
                UpdateCore(reference, newLink, GetTargetCore(reference));
            }
            for (var i = (long)referencesAsSourceCount; i < references.Length; i++)
            {
                var reference = references[i];
                if (reference == linkIndex) continue;
                UpdateCore(reference, GetSourceCore(reference), newLink);
            }

            DeleteCore(linkIndex);

            return newLink;
        }

        /// <summary>Удаляет связь с указанными началом (Source) и концом (Target).</summary>
        /// <param name="source">Индекс связи, которая является началом удаляемой связи.</param>
        /// <param name="target">Индекс связи, которая является концом удаляемой связи.</param>
        public void Delete(ulong source, ulong target)
        {
            Sync.ExecuteWriteOperation(() =>
            {
                var link = SearchCore(source, target);
                EnsureLinkExists(link);
                DeleteCore(link);
            });
        }

        /// <summary>Удаляет связь с указанным индексом.</summary>
        /// <param name="link">Индекс удаляемой связи.</param>
        public void Delete(ulong link)
        {
            Sync.ExecuteWriteOperation(() =>
            {
                EnsureLinkExists(link);
                DeleteCore(link);
            });
        }

        /// <summary>Удаляет связь с указанным индексом.</summary>
        /// <param name="link">Индекс удаляемой связи.</param>
        /// <remarks>Версия функции без дополнительных проверок для ускорения работы рекурсии.</remarks>
        public void DeleteCore(ulong link)
        {
            if (_memoryManager.Count(link) > 0)
            {
                var before = GetLinkCore(link);

                _memoryManager.SetLinkValue(link, LinksConstants.Null, LinksConstants.Null);

                // TODO: Заменить на _memoryManager.Count(LinksConstants.Null, link)
                var referencesCount =
                    _memoryManager.Count(LinksConstants.Null, link, LinksConstants.Null) +
                    _memoryManager.Count(LinksConstants.Null, LinksConstants.Null, link);

                var references = new ulong[referencesCount];

                var referencesFiller = new ArrayFiller<ulong>(references);

                _memoryManager.Each(referencesFiller.AddAndReturnTrue, LinksConstants.Null, link, LinksConstants.Null);
                _memoryManager.Each(referencesFiller.AddAndReturnTrue, LinksConstants.Null, LinksConstants.Null, link);

                //references.Sort(); // TODO: Решить необходимо ли для корректного порядка отмены операций в транзакциях

                for (var i = (long)referencesCount - 1; i >= 0; i--)
                    DeleteCore(references[i]);

                _memoryManager.FreeLink(link);

                CommitDeletion(before);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureLinkExists(ulong link)
        {
            if (_memoryManager.Count(link) == 0)
                throw new ArgumentLinkDoesNotExistsException<ulong>(link);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureLinkExists(ulong link, string argumentName)
        {
            if (_memoryManager.Count(link) == 0)
                throw new ArgumentLinkDoesNotExistsException<ulong>(link, argumentName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureLinkIsAnyOrExists(ulong link, string argumentName)
        {
            if (link != LinksConstants.Any && _memoryManager.Count(link) == 0)
                throw new ArgumentLinkDoesNotExistsException<ulong>(link, argumentName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureLinkIsItselfOrExists(ulong link, string argumentName)
        {
            if (link != LinksConstants.Itself && _memoryManager.Count(link) == 0)
                throw new ArgumentLinkDoesNotExistsException<ulong>(link, argumentName);
        }

        #endregion

        #region DisposalBase

        protected override void DisposeCore(bool manual)
        {
            DisposeTransitions();
            var memoryManager = _memoryManager as IDisposable;
            if (memoryManager != null) memoryManager.Dispose();
        }

        #endregion
    }
}