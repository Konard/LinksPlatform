﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Platform.Data.Exceptions;
using Platform.Helpers.Setters;

namespace Platform.Data
{
    public static class ILinksExtensions
    {
        public static TLink Count<TLink>(this ILinks<TLink> links, params TLink[] restrictions) => links.Count(restrictions);

        /// <summary>
        /// Возвращает значение, определяющее существует ли связь с указанным индексом в хранилище связей.
        /// </summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="link">Индекс проверяемой на существование связи.</param>
        /// <returns>Значение, определяющее существует ли связь.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Exists<TLink>(this ILinks<TLink> links, TLink link) => Comparer<TLink>.Default.Compare(links.Count(link), default) > 0;

        /// <param name="links">Хранилище связей.</param>
        /// <remarks>
        /// TODO: May be move to EnsureExtensions or make it both there and here
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureLinkExists<TLink>(this ILinks<TLink> links, TLink link)
        {
            if (!links.Exists(link))
            {
                throw new ArgumentLinkDoesNotExistsException<TLink>(link);
            }
        }

        /// <param name="links">Хранилище связей.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureLinkExists<TLink>(this ILinks<TLink> links, TLink link, string argumentName)
        {
            if (!links.Exists(link))
            {
                throw new ArgumentLinkDoesNotExistsException<TLink>(link, argumentName);
            }
        }

        /// <summary>
        /// Выполняет проход по всем связям, соответствующим шаблону, вызывая обработчик (handler) для каждой подходящей связи.
        /// </summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="handler">Обработчик каждой подходящей связи.</param>
        /// <param name="restrictions">Ограничения на содержимое связей. Каждое ограничение может иметь значения: Constants.Null - 0-я связь, обозначающая ссылку на пустоту, Any - отсутствие ограничения, 1..∞ конкретный адрес связи.</param>
        /// <returns>True, в случае если проход по связям не был прерван и False в обратном случае.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Each<TLink>(this ILinks<TLink> links, Func<IList<TLink>, TLink> handler, params TLink[] restrictions) => !EqualityComparer<TLink>.Default.Equals(links.Each(handler, restrictions), links.Constants.Break);

        /// <summary>
        /// Возвращает части-значения для связи с указанным индексом.
        /// </summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="link">Индекс связи.</param>
        /// <returns>Уникальную связь.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IList<TLink> GetLink<TLink>(this ILinks<TLink> links, TLink link)
        {
            var linkPartsSetter = new Setter<IList<TLink>, TLink>(links.Constants.Continue, links.Constants.Break, default);
            links.Each(linkPartsSetter.SetAndReturnTrue, link);
            return linkPartsSetter.Result;
        }

        #region Points

        /// <summary>Возвращает значение, определяющее является ли связь с указанным индексом точкой полностью (связью замкнутой на себе дважды).</summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="link">Индекс проверяемой связи.</param>
        /// <returns>Значение, определяющее является ли связь точкой полностью.</returns>
        /// <remarks>
        /// Связь точка - это связь, у которой начало (Source) и конец (Target) есть сама эта связь.
        /// Но что, если точка уже есть, а нужно создать пару с таким же значением? Должны ли точка и пара существовать одновременно?
        /// Или в качестве решения для точек нужно использовать 0 в качестве начала и конца, а сортировать по индексу в массиве связей?
        /// Какое тогда будет значение Source и Target у точки? 0 или её индекс?
        /// Или точка должна быть одновременно точкой и парой, а также последовательностями из самой себя любого размера?
        /// Как только есть ссылка на себя, появляется этот парадокс, причём достаточно даже одной ссылки на себя (частичной точки).
        /// А что если не выбирать что является точкой, пара нулей (цикл через пустоту) или 
        /// самостоятельный цикл через себя? Что если предоставить все варианты использования связей?
        /// Что если разрешить и нули, а так же частичные варианты?
        /// 
        /// Что если точка, это только в том случае когда link.Source == link && link.Target == link , т.е. дважды ссылка на себя.
        /// А пара это тогда, когда link.Source == link.Target && link.Source != link , т.е. ссылка не на себя а во вне.
        /// 
        /// Тогда если у нас уже создана пара, но нам нужна точка, мы можем используя промежуточную связь,
        /// например "DoubletOf" обозначить что является точно парой, а что точно точкой.
        /// И наоборот этот же метод поможет, если уже существует точка, но нам нужна пара.
        /// </remarks>
        public static bool IsFullPoint<T>(this ILinks<T> links, T link)
        {
            links.EnsureLinkExists(link);
            return Point<T>.IsFullPoint(links.GetLink(link));
        }

        /// <summary>Возвращает значение, определяющее является ли связь с указанным индексом точкой частично (связью замкнутой на себе как минимум один раз).</summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="link">Индекс проверяемой связи.</param>
        /// <returns>Значение, определяющее является ли связь точкой частично.</returns>
        /// <remarks>
        /// Достаточно любой одной ссылки на себя.
        /// Также в будущем можно будет проверять и всех родителей, чтобы проверить есть ли ссылки на себя (на эту связь).
        /// </remarks>
        public static bool IsPartialPoint<T>(this ILinks<T> links, T link)
        {
            links.EnsureLinkExists(link);
            return Point<T>.IsPartialPoint(links.GetLink(link));
        }

        #endregion
    }
}
