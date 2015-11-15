using System;
using Platform.Data.Core.Structures;

namespace Platform.Data.Core.Pairs
{
    /// <summary>
    /// Представляет интерфейс для работы с базой данных в формате Links (хранилища взаимосвязей).
    /// </summary>
    /// <remarks>
    /// Регионы Read & Write можно выделить в отдельные интерфейсы.
    /// </remarks>
    public interface ILinks<TLink>
    {
        #region Read

        /// <summary>
        /// Возвращает общее число связей находящихся в хранилище.
        /// </summary>
        ulong Total { get; }

        /// <summary>
        /// Возвращает индекс начальной (Source) связи для указанной связи.
        /// </summary>
        /// <param name="link">Индекс связи.</param>
        /// <returns>Индекс начальной связи для указанной связи.</returns>
        TLink GetSource(TLink link);

        /// <summary>
        /// Возвращает индекс конечной (Target) связи для указанной связи.
        /// </summary>
        /// <param name="link">Индекс связи.</param>
        /// <returns>Индекс конечной связи для указанной связи.</returns>
        TLink GetTarget(TLink link);

        /// <summary>
        /// Возвращает уникальную связь для указанного индекса.
        /// </summary>
        /// <param name="link">Индекс связи.</param>
        /// <returns>Уникальную связь.</returns>
        Link GetLink(TLink link);

        /// <summary>
        /// Возвращает значение, определяющее существует ли связь с указанным индексом в базе данных.
        /// </summary>
        /// <param name="link">Индекс проверяемой на существование связи.</param>
        /// <returns>Значение, определяющее существует ли связь.</returns>
        bool Exists(TLink link);

        /// <summary>
        /// Выполняет проход по всем связям, соответствующим шаблону, вызывая обработчик (handler) для каждой подходящей связи.
        /// </summary>
        /// <param name="source">Значение, определяющее соответствующие шаблону связи. (0 - любое начало, 1..∞ конкретное начало)</param>
        /// <param name="target">Значение, определяющее соответствующие шаблону связи. (0 - любое конец, 1..∞ конкретный конец)</param>
        /// <param name="handler">Обработчик каждой подходящей связи.</param>
        /// <returns>True, в случае если проход по связям не был прерван и False в обратном случае.</returns>
        bool Each(TLink source, TLink target, Func<TLink, bool> handler);

        /// <summary>
        /// Выполняет поиск связи с указанными Source (началом) и Target (концом)
        /// </summary>
        /// <param name="source">Индекс связи, которая является началом для искомой связи.</param>
        /// <param name="target">Индекс связи, которая является концом для искомой связи.</param>
        /// <returns>Индекс искомой связи с указанными Source (началом) и Target (концом)</returns>
        TLink Search(TLink source, TLink target);

        #endregion

        #region Write

        /// <summary>
        /// Создаёт связь (если она не существовала), либо возвращает индекс существующей связи с указанными Source (началом) и Target (концом).
        /// </summary>
        /// <param name="source">Индекс связи, которая является началом на создаваемой связи.</param>
        /// <param name="target">Индекс связи, которая является концом для создаваемой связи.</param>
        /// <returns>Индекс связи, с указанным Source (началом) и Target (концом)</returns>
        TLink Create(TLink source, TLink target);

        /// <summary>
        /// Обновляет связь с указанными началом (Source) и концом (Target)
        /// на связь с указанными началом (NewSource) и концом (NewTarget).
        /// </summary>
        /// <param name="source">Индекс связи, которая является началом обновляемой связи.</param>
        /// <param name="target">Индекс связи, которая является концом обновляемой связи.</param>
        /// <param name="newSource">Индекс связи, которая является началом связи, на которую выполняется обновление.</param>
        /// <param name="newTarget">Индекс связи, которая является концом связи, на которую выполняется обновление.</param>
        /// <returns>Индекс обновлённой связи.</returns>
        TLink Update(TLink source, TLink target, TLink newSource, TLink newTarget);

        /// <summary>Удаляет связь с указанным индексом.</summary>
        /// <param name="link">Индекс удаляемой связи.</param>
        void Delete(TLink link);

        #endregion
    }
}