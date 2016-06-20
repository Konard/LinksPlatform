using System;
using System.Collections.Generic;

namespace Platform.Data.Core.Pairs
{
    public interface ILinksMemoryManager<TLink>
    {
        /// <summary>
        /// Возвращает набор констант, который необходим для эффективной коммуникации с методами этого интерфейса.
        /// Эти константы не меняются с момента создания точки доступа к хранилищу.
        /// </summary>
        ILinksCombinedConstants<bool, TLink, int> Constants { get; }

        TLink Count(IList<TLink> restrictions);
        bool Each(Func<TLink, bool> handler, IList<TLink> restrictions);
        void SetLinkValue(IList<TLink> parts);
        IList<TLink> GetLinkValue(TLink link);

        /// <summary>
        /// Выделяет следующую свободную связь и возвращает её индекс.
        /// </summary>
        /// <returns>Индекс свободной связи.</returns>
        TLink AllocateLink();

        /// <summary>
        /// Высвобождает используемое связью пространство.
        /// </summary>
        /// <param name="link">Индекс высвобождаемой связи.</param>
        void FreeLink(TLink link);
    }
}
