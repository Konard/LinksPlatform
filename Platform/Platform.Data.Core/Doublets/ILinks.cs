using System;
using System.Collections.Generic;

namespace Platform.Data.Core.Doublets
{
    /// <summary>
    /// Представляет интерфейс для работы с данными в формате Links (хранилища взаимосвязей).
    /// </summary>
    /// <remarks>
    /// Этот интерфейс в данный момент не зависит от размера содержимого связи, а значит подходит как для дублетов, так и для триплетов и т.п.
    /// Возможно этот интерфейс подходит даже для Sequences.
    /// TODO: Этот интерфейс можно перенести в пространство имён Platform.Data.Core или Platform.Data.Core.Common
    /// </remarks>
    public interface ILinks<TLink>
    {
        #region Constants

        /// <summary>
        /// Возвращает набор констант, который необходим для эффективной коммуникации с методами этого интерфейса.
        /// Эти константы не меняются с момента создания точки доступа к хранилищу.
        /// </summary>
        ILinksCombinedConstants<TLink, TLink, int> Constants { get; }

        #endregion

        #region Read

        /// <summary>
        /// Подсчитывает и возвращает общее число связей находящихся в хранилище, соответствующих указанным ограничениям.
        /// </summary>
        /// <param name="restriction">Ограничения на содержимое связей.</param>
        /// <returns>Общее число связей находящихся в хранилище, соответствующих указанным ограничениям.</returns>
        TLink Count(IList<TLink> restriction);

        /// <summary>
        /// Выполняет проход по всем связям, соответствующим шаблону, вызывая обработчик (handler) для каждой подходящей связи.
        /// </summary>
        /// <param name="handler">Обработчик каждой подходящей связи.</param>
        /// <param name="restrictions">Ограничения на содержимое связей. Каждое ограничение может иметь значения: Constants.Null - 0-я связь, обозначающая ссылку на пустоту, Any - отсутствие ограничения, 1..∞ конкретный адрес связи.</param>
        /// <returns>True, в случае если проход по связям не был прерван и False в обратном случае.</returns>
        TLink Each(Func<IList<TLink>, TLink> handler, IList<TLink> restrictions);

        // TODO: Move to UniLinksExtensions
        //return Trigger(restrictions, (before, after) => handler(before), null, null);
        //// Trigger(restrictions, null, restrictions, null); - Должно быть синонимом

        #endregion

        #region Write

        /// <summary>
        /// Создаёт связь.
        /// </summary>
        /// <returns>Индекс созданной связи.</returns>
        TLink Create(); // TODO: Возможно всегда нужно принимать restrictions, возможно и возвращать связь нужно целиком.

        // TODO: Move to UniLinksExtensions
        //// { 0, 0, 0 } => { ifself, 0, 0 }
        //// { 0 } => { ifself, 0, 0 } *

        //T result = default(T);

        //Func<IList<T>, IList<T>, T> substitutedHandler = (before, after) =>
        //{
        //    result = after[Constants.IndexPart];
        //    return Constants.Continue;
        //};

        //// Сейчас будет полагать что соответствие шаблону (ограничению) произойдёт только один раз
        //Trigger(new[] { Constants.Null }, null,
        //        new[] { Constants.Itself, Constants.Null, Constants.Null }, substitutedHandler);

        //// TODO: Возможна реализация опционального поведения (один ноль-пустота, бесконечность нолей-пустот)
        //// 0 => 1

        //// 0 => 1
        //// 0 => 2
        //// 0 => 3
        //// ...

        /// <summary>
        /// Обновляет связь с указанными restrictions[Constants.IndexPart] в адресом связи
        /// на связь с указанным новым содержимым.
        /// </summary>
        /// <param name="restriction">
        /// Ограничения на содержимое связей.
        /// Предполагается, что будет указан адрес связи (в restrictions[Constants.IndexPart]) и далее за ним будет следовать содержимое связи.
        /// Каждое ограничение может иметь значения: Constants.Null - 0-я связь, обозначающая ссылку на пустоту,
        /// Constants.Itself - требование установить ссылку на себя, 1..∞ конкретный адрес другой связи.
        /// </param>
        /// <returns>Индекс обновлённой связи.</returns>
        TLink Update(IList<TLink> restrictions); // TODO: Возможно и возвращать связь нужно целиком.

        // TODO: Move to UniLinksExtensions
        //// { 1, any, any } => { 1, x, y }
        //// { 1 } => { 1, x, y } *
        //// { 1, 3, 4 }

        //Trigger(new[] { restrictions[Constants.IndexPart] }, null, 
        //        new[] { restrictions[Constants.IndexPart], restrictions[Constants.SourcePart], restrictions[Constants.TargetPart] }, null);

        //return restrictions[Constants.IndexPart];

        /// <summary>Удаляет связь с указанным индексом.</summary>
        /// <param name="link">Индекс удаляемой связи.</param>
        void Delete(TLink link); // TODO: Возможно всегда нужно принимать restrictions, a так же возвращать удалённую связь, если удаление было реально выполнено, и Null, если нет.

        // TODO: Move to UniLinksExtensions
        //// { 1 } => { 0, 0, 0 }
        //// { 1 } => { 0 } *
        //Trigger(new[] { link }, null,
        //        new[] { Constants.Null }, null);

        // TODO: Если учесть последние TODO, тогда все функции Create, Update, Delete будут иметь один и тот же интерфейс - IList<TLink> Method(IList<TLink> restrictions);, что может быть удобно для "Create|Update|Delete" транзакционности, !! но нужна ли такая транзакционность? Ведь всё что нужно записывать в транзакцию это изменение с чего в во что. Создание это index, 0, 0 -> index, X, Y (и начало отслеживания связи). Удаление это всегда index, X, Y -> index, 0, 0 (и прекращение отслеживания связи). Обновление - аналогично, но состояние отслеживания не меняется.
        // TODO: Хотя пожалуй, выдавать дополнительное значение в виде True/False вряд ли допустимо для Delete. Тогда создание это 0,0,0 -> I,S,T и т.п.
        // TODO: Если все методы, Create, Update, Delete будут и принимать и возвращать IList<TLink>, то можно всё заменить одним единым Update, у которого для удаления нужно указать исходный адрес связи и Constans.Null в качестве его нового значения (возможно будет указано 2 ограничения из 3-х)

        #endregion
    }
}
