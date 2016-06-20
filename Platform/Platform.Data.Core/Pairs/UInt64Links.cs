using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Platform.Helpers;
using Platform.Helpers.Collections;
using Platform.Helpers.Disposables;

namespace Platform.Data.Core.Pairs
{
    /// <summary>
    /// Представляет объект для работы с базой данных (файлом) в формате Links (массива взаимосвязей).
    /// </summary>
    /// <remarks>
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
    public sealed partial class UInt64Links : DisposableBase, ILinks<ulong>
    {
        private readonly ILinksMemoryManager<ulong> _memoryManager;

        public ILinksCombinedConstants<bool, ulong, int> Constants { get; } = Default<LinksConstants<bool, ulong, int>>.Instance;

        #region Links Logic

        public UInt64Links(string address)
            : this(new UInt64LinksMemoryManager(address))
        {
        }

        public UInt64Links(ILinksMemoryManager<ulong> memoryManager)
        {
            _memoryManager = memoryManager;
        }

        public UInt64Links(ILinksOptions<ulong> linksOptions)
            : this(linksOptions.MemoryManager)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe UInt64Link GetLinkStruct(ulong link) => *((UInt64LinksMemoryManager)_memoryManager).GetLinkStruct(link);

        public ulong Count(params ulong[] restrictions) => _memoryManager.Count(restrictions);

        public bool Each(Func<IList<ulong>, bool> handler, IList<ulong> restrictions)
        {
            this.EnsureLinkIsAnyOrExists(restrictions);
            return _memoryManager.Each(link => handler(GetLinkStruct(link)), restrictions);
        }

        public ulong Create()
        {
            var link = _memoryManager.CreatePoint();
            CommitCreation(GetLinkStruct(link));
            return link;
        }

        public ulong Update(IList<ulong> restrictions)
        {
            if (restrictions.IsNullOrEmpty())
                return Constants.Null;

            // TODO: Remove usages of these hacks (these should not be backwards compatible)
            if (restrictions.Count == 2)
                return this.Merge(restrictions[0], restrictions[1]);
            if (restrictions.Count == 4)
                return this.UpdateOrCreateOrGet(restrictions[0], restrictions[1], restrictions[2], restrictions[3]);

            // TODO: Looks like this is a common type of exceptions linked with restrictions support
            if (restrictions.Count != 3)
                throw new NotSupportedException();

            var updatedLink = restrictions[Constants.IndexPart];
            this.EnsureLinkExists(updatedLink, nameof(Constants.IndexPart));
            var newSource = restrictions[Constants.SourcePart];
            this.EnsureLinkIsItselfOrExists(newSource, nameof(Constants.SourcePart));
            var newTarget = restrictions[Constants.TargetPart];
            this.EnsureLinkIsItselfOrExists(newTarget, nameof(Constants.TargetPart));

            var existedLink = Constants.Null;

            if (newSource != Constants.Itself && newTarget != Constants.Itself)
                existedLink = this.SearchOrDefault(newSource, newTarget);

            if (existedLink == Constants.Null)
            {
                var before = GetLinkStruct(updatedLink);

                if (before.Source != newSource || before.Target != newSource)
                {
                    _memoryManager.SetLinkValue(updatedLink, newSource == Constants.Itself ? updatedLink : newSource,
                                                             newTarget == Constants.Itself ? updatedLink : newTarget);

                    CommitUpdate(before, GetLinkStruct(updatedLink));
                }

                return updatedLink;
            }
            else
            {
                // Replace one link with another (replaced link is deleted, children are updated or deleted), it is actually merge operation
                return this.Merge(updatedLink, existedLink);
            }
        }

        /// <summary>Удаляет связь с указанным индексом.</summary>
        /// <param name="link">Индекс удаляемой связи.</param>
        public void Delete(ulong link)
        {
            this.EnsureLinkExists(link);
            var before = GetLinkStruct(link);

            _memoryManager.SetLinkValue(link, Constants.Null, Constants.Null);

            var referencesCount = _memoryManager.Count(Constants.Any, link);
            var references = new ulong[referencesCount];

            var referencesFiller = new ArrayFiller<ulong>(references);

            _memoryManager.Each(referencesFiller.AddAndReturnTrue, Constants.Any, link);

            //references.Sort(); // TODO: Решить необходимо ли для корректного порядка отмены операций в транзакциях

            for (var i = (long)referencesCount - 1; i >= 0; i--)
                if (this.Exists(references[i]))
                    Delete(references[i]);
            //else
            // TODO: Определить почему здесь есть связи, которых не существует  

            _memoryManager.FreeLink(link);

            CommitDeletion(before);
        }

        #endregion

        #region DisposalBase

        protected override void DisposeCore(bool manual)
        {
            if (manual)
            {
                DisposeTransitions();
                Disposable.TryDispose(_memoryManager);
            }
        }

        #endregion
    }
}