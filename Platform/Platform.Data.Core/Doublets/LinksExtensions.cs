using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Platform.Collections.Arrays;
using Platform.Helpers;
using Platform.Helpers.Numbers;
using Platform.Helpers.Random;
using Platform.Data.Core.Common;
using Platform.Data.Core.Exceptions;
using Platform.Data.Core.Sequences;

namespace Platform.Data.Core.Doublets
{
    public static class LinksExtensions
    {
        public static readonly LinksConstants<bool, ulong, int> Constants = Default<LinksConstants<bool, ulong, int>>.Instance;

        public static void UseUnicode(this ILinks<ulong> links) => UnicodeMap.InitNew(links);

        public static void RunRandomCreations<TLink>(this ILinks<TLink> links, long amountOfCreations)
        {
            for (long i = 0; i < amountOfCreations; i++)
            {
                Integer<TLink> source = RandomHelpers.DefaultFactory.NextUInt64((Integer<TLink>)links.Count());
                Integer<TLink> target = RandomHelpers.DefaultFactory.NextUInt64((Integer<TLink>)links.Count());

                links.CreateAndUpdate(source, target);
            }
        }

        public static void RunRandomSearches<TLink>(this ILinks<TLink> links, long amountOfSearches)
        {
            for (long i = 0; i < amountOfSearches; i++)
            {
                Integer<TLink> source = RandomHelpers.DefaultFactory.NextUInt64(1, (Integer<TLink>)links.Count());
                Integer<TLink> target = RandomHelpers.DefaultFactory.NextUInt64(1, (Integer<TLink>)links.Count());

                links.SearchOrDefault(source, target);
            }
        }

        public static void RunRandomDeletions<TLink>(this ILinks<TLink> links, long amountOfDeletions)
        {
            var min = (ulong)amountOfDeletions > (Integer<TLink>)links.Count() ? 1 : (Integer<TLink>)links.Count() - (ulong)amountOfDeletions;

            for (long i = 0; i < amountOfDeletions; i++)
            {
                Integer<TLink> link = RandomHelpers.DefaultFactory.NextUInt64(min, (Integer<TLink>)links.Count());
                links.Delete(link);
                if ((Integer<TLink>)links.Count() < min)
                    break;
            }
        }

        /// <remarks>
        /// TODO: Возможно есть очень простой способ это сделать.
        /// (Например просто удалить файл, или изменить его размер таким образом,
        /// чтобы удалился весь контент)
        /// Например через _header->AllocatedLinks в ResizableDirectMemoryLinks
        /// </remarks>
        public static void DeleteAll<TLink>(this ILinks<TLink> links)
        {
            var equalityComparer = EqualityComparer<TLink>.Default;
            var comparer = Comparer<TLink>.Default;

            for (var i = links.Count(); comparer.Compare(i, default) > 0; i = MathHelpers.Decrement(i))
            {
                links.Delete(i);
                if (!equalityComparer.Equals(links.Count(), MathHelpers.Decrement(i)))
                    i = links.Count();
            }
        }

        public static TLink First<TLink>(this ILinks<TLink> links)
        {
            TLink firstLink = default;

            var equalityComparer = EqualityComparer<TLink>.Default;

            if (equalityComparer.Equals(links.Count(), default))
                throw new Exception("В хранилище нет связей.");

            links.Each(links.Constants.Any, links.Constants.Any, link =>
            {
                firstLink = link[links.Constants.IndexPart];
                return links.Constants.Break;
            });

            if (equalityComparer.Equals(firstLink, default))
                throw new Exception("В процессе поиска по хранилищу не было найдено связей.");

            return firstLink;
        }

        public static void EnsureEachLinkExists(this ILinks<ulong> links, IList<ulong> sequence)
        {
            if (sequence == null)
                return;

            for (var i = 0; i < sequence.Count; i++)
                if (!links.Exists(sequence[i]))
                    throw new ArgumentLinkDoesNotExistsException<ulong>(sequence[i],
                                                                        $"sequence[{i}]");
        }

        public static void EnsureEachLinkIsAnyOrExists(this ILinks<ulong> links, IList<ulong> sequence)
        {
            if (sequence == null)
                return;

            for (var i = 0; i < sequence.Count; i++)
                if (sequence[i] != Constants.Any && !links.Exists(sequence[i]))
                    throw new ArgumentLinkDoesNotExistsException<ulong>(sequence[i],
                                                                        $"sequence[{i}]");
        }

        public static bool AnyLinkIsAny(this ILinks<ulong> links, params ulong[] sequence)
        {
            if (sequence == null)
                return false;

            for (var i = 0; i < sequence.Length; i++)
                if (sequence[i] == Constants.Any) return true;

            return false;
        }

        public static string FormatStructure(this ILinks<ulong> links, ulong linkIndex, Func<UInt64Link, bool> isElement, bool renderIndex = false, bool renderDebug = false)
        {
            var sb = new StringBuilder();
            var visited = new HashSet<ulong>();

            links.AppendStructure(sb, visited, linkIndex, isElement, (innerSb, link) => innerSb.Append(link.Index), renderIndex, renderDebug);

            return sb.ToString();
        }

        public static string FormatStructure(this ILinks<ulong> links, ulong linkIndex, Func<UInt64Link, bool> isElement, Action<StringBuilder, UInt64Link> appendElement, bool renderIndex = false, bool renderDebug = false)
        {
            var sb = new StringBuilder();
            var visited = new HashSet<ulong>();

            links.AppendStructure(sb, visited, linkIndex, isElement, appendElement, renderIndex, renderDebug);

            return sb.ToString();
        }

        public static void AppendStructure(this ILinks<ulong> links, StringBuilder sb, HashSet<ulong> visited, ulong linkIndex, Func<UInt64Link, bool> isElement, Action<StringBuilder, UInt64Link> appendElement, bool renderIndex = false, bool renderDebug = false)
        {
            if (sb == null)
                throw new ArgumentNullException(nameof(sb));

            if (linkIndex == Constants.Null || linkIndex == Constants.Any || linkIndex == Constants.Itself)
                return;

            if (links.Exists(linkIndex))
            {
                if (visited.Add(linkIndex))
                {
                    sb.Append('(');

                    var link = new UInt64Link(links.GetLink(linkIndex));

                    if (renderIndex)
                    {
                        sb.Append(link.Index);
                        sb.Append(':');
                    }

                    if (link.Source == link.Index)
                        sb.Append(link.Index);
                    else
                    {
                        var source = new UInt64Link(links.GetLink(link.Source));
                        if (isElement(source))
                            appendElement(sb, source);
                        else
                            links.AppendStructure(sb, visited, source.Index, isElement, appendElement, renderIndex);
                    }

                    sb.Append(' ');

                    if (link.Target == link.Index)
                        sb.Append(link.Index);
                    else
                    {
                        var target = new UInt64Link(links.GetLink(link.Target));
                        if (isElement(target))
                            appendElement(sb, target);
                        else
                            links.AppendStructure(sb, visited, target.Index, isElement, appendElement, renderIndex);
                    }

                    sb.Append(')');
                }
                else
                {
                    if (renderDebug)
                        sb.Append('*');
                    sb.Append(linkIndex);
                }
            }
            else
            {
                if (renderDebug)
                    sb.Append('~');
                sb.Append(linkIndex);
            }
        }

        public static bool IsInnerReference<TLink>(this ILinks<TLink> links, TLink reference)
        {
            var constants = links.Constants;
            var comparer = Comparer<TLink>.Default;
            return (comparer.Compare(constants.MinPossibleIndex, reference) >= 0) && (comparer.Compare(reference, constants.MaxPossibleIndex) <= 0);
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

        #region Paths

        /// <remarks>
        /// TODO: Как так? Как то что ниже может быть корректно?
        /// Скорее всего практически не применимо
        /// Предполагалось, что можно было конвертировать формируемый в проходе через SequenceWalker 
        /// Stack в конкретный путь из Source, Target до связи, но это не всегда так.
        /// TODO: Возможно нужен метод, который именно выбрасывает исключения (EnsurePathExists)
        /// </remarks>
        public static bool CheckPathExistance<TLink>(this ILinks<TLink> links, params TLink[] path)
        {
            var current = path[0];

            //EnsureLinkExists(current, "path");
            if (!links.Exists(current))
                return false;

            var equalityComparer = EqualityComparer<TLink>.Default;

            for (var i = 1; i < path.Length; i++)
            {
                var next = path[i];

                var values = links.GetLink(current);
                var source = values[Constants.SourcePart];
                var target = values[Constants.TargetPart];

                if (equalityComparer.Equals(source, target) && equalityComparer.Equals(source, next))
                    //throw new Exception(string.Format("Невозможно выбрать путь, так как и Source и Target совпадают с элементом пути {0}.", next));
                    return false;
                if (!equalityComparer.Equals(next, source) && !equalityComparer.Equals(next, target))
                    //throw new Exception(string.Format("Невозможно продолжить путь через элемент пути {0}", next));
                    return false;

                current = next;
            }

            return true;
        }

        /// <remarks>
        /// Может потребовать дополнительного стека для PathElement's при использовании SequenceWalker.
        /// </remarks>
        public static TLink GetByKeys<TLink>(this ILinks<TLink> links, TLink root, params int[] path)
        {
            links.EnsureLinkExists(root, "root");

            var currentLink = root;
            for (var i = 0; i < path.Length; i++)
                currentLink = links.GetLink(currentLink)[path[i]];
            return currentLink;
        }

        public static TLink GetSquareMatrixSequenceElementByIndex<TLink>(this ILinks<TLink> links, TLink root, ulong size, ulong index)
        {
            var source = Constants.SourcePart;
            var target = Constants.TargetPart;

            if (!MathHelpers.IsPowerOfTwo(size))
                throw new ArgumentOutOfRangeException(nameof(size), "Sequences with sizes other than powers of two are not supported.");

            var path = new BitArray(BitConverter.GetBytes(index));
            var length = MathHelpers.GetLowestBitPosition(size);

            links.EnsureLinkExists(root, "root");

            var currentLink = root;
            for (var i = length - 1; i >= 0; i--)
                currentLink = links.GetLink(currentLink)[path[i] ? target : source];
            return currentLink;
        }

        #endregion

        /// <summary>
        /// Возвращает индекс указанной связи.
        /// </summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="link">Связь представленная списком, состоящим из её адреса и содержимого.</param>
        /// <returns>Индекс начальной связи для указанной связи.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink GetIndex<TLink>(this ILinks<TLink> links, IList<TLink> link) => link[links.Constants.IndexPart];

        /// <summary>
        /// Возвращает индекс начальной (Source) связи для указанной связи.
        /// </summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="link">Индекс связи.</param>
        /// <returns>Индекс начальной связи для указанной связи.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink GetSource<TLink>(this ILinks<TLink> links, TLink link) => links.GetLink(link)[links.Constants.SourcePart];

        /// <summary>
        /// Возвращает индекс начальной (Source) связи для указанной связи.
        /// </summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="link">Связь представленная списком, состоящим из её адреса и содержимого.</param>
        /// <returns>Индекс начальной связи для указанной связи.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink GetSource<TLink>(this ILinks<TLink> links, IList<TLink> link) => link[links.Constants.SourcePart];

        /// <summary>
        /// Возвращает индекс конечной (Target) связи для указанной связи.
        /// </summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="link">Индекс связи.</param>
        /// <returns>Индекс конечной связи для указанной связи.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink GetTarget<TLink>(this ILinks<TLink> links, TLink link) => links.GetLink(link)[links.Constants.TargetPart];

        /// <summary>
        /// Возвращает индекс конечной (Target) связи для указанной связи.
        /// </summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="link">Связь представленная списком, состоящим из её адреса и содержимого.</param>
        /// <returns>Индекс конечной связи для указанной связи.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink GetTarget<TLink>(this ILinks<TLink> links, IList<TLink> link) => link[links.Constants.TargetPart];

        /// <summary>
        /// Возвращает части-значения для связи с указанным индексом.
        /// </summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="link">Индекс связи.</param>
        /// <returns>Уникальную связь.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IList<TLink> GetLink<TLink>(this ILinks<TLink> links, TLink link)
        {
            var linkPartsSetter = new Setter<IList<TLink>, TLink>(links.Constants.Continue, links.Constants.Break);
            links.Each(linkPartsSetter.SetAndReturnTrue, link);
            return linkPartsSetter.Result;
        }

        public static TLink Count<TLink>(this ILinks<TLink> links, params TLink[] restrictions) => links.Count(restrictions);

        /// <summary>
        /// Выполняет проход по всем связям, соответствующим шаблону, вызывая обработчик (handler) для каждой подходящей связи.
        /// </summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="source">Значение, определяющее соответствующие шаблону связи. (Constants.Null - 0-я связь, обозначающая ссылку на пустоту в качестве начала, Constants.Any - любое начало, 1..∞ конкретное начало)</param>
        /// <param name="target">Значение, определяющее соответствующие шаблону связи. (Constants.Null - 0-я связь, обозначающая ссылку на пустоту в качестве конца, Constants.Any - любой конец, 1..∞ конкретный конец)</param>
        /// <param name="handler">Обработчик каждой подходящей связи.</param>
        /// <returns>True, в случае если проход по связям не был прерван и False в обратном случае.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Each<TLink>(this ILinks<TLink> links, TLink source, TLink target, Func<TLink, bool> handler)
        {
            var constants = links.Constants;
            return links.Each(link => handler(link[constants.IndexPart]) ? constants.Continue : constants.Break, constants.Any, source, target);
        }

        /// <summary>
        /// Выполняет проход по всем связям, соответствующим шаблону, вызывая обработчик (handler) для каждой подходящей связи.
        /// </summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="source">Значение, определяющее соответствующие шаблону связи. (Constants.Null - 0-я связь, обозначающая ссылку на пустоту в качестве начала, Constants.Any - любое начало, 1..∞ конкретное начало)</param>
        /// <param name="target">Значение, определяющее соответствующие шаблону связи. (Constants.Null - 0-я связь, обозначающая ссылку на пустоту в качестве конца, Constants.Any - любой конец, 1..∞ конкретный конец)</param>
        /// <param name="handler">Обработчик каждой подходящей связи.</param>
        /// <returns>True, в случае если проход по связям не был прерван и False в обратном случае.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Each<TLink>(this ILinks<TLink> links, TLink source, TLink target, Func<IList<TLink>, TLink> handler) => links.Each(handler, links.Constants.Any, source, target);

        /// <summary>
        /// Выполняет проход по всем связям, соответствующим шаблону, вызывая обработчик (handler) для каждой подходящей связи.
        /// </summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="handler">Обработчик каждой подходящей связи.</param>
        /// <param name="restrictions">Ограничения на содержимое связей. Каждое ограничение может иметь значения: Constants.Null - 0-я связь, обозначающая ссылку на пустоту, Any - отсутствие ограничения, 1..∞ конкретный адрес связи.</param>
        /// <returns>True, в случае если проход по связям не был прерван и False в обратном случае.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Each<TLink>(this ILinks<TLink> links, Func<IList<TLink>, TLink> handler, params TLink[] restrictions) => !EqualityComparer<TLink>.Default.Equals(links.Each(handler, restrictions), links.Constants.Break);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IList<IList<TLink>> All<TLink>(this ILinks<TLink> links, params TLink[] restrictions)
        {
            var constants = links.Constants;

            int listSize = (Integer<TLink>)links.Count(restrictions);
            var list = new IList<TLink>[listSize];

            if (listSize > 0)
            {
                var filler = new ArrayFiller<IList<TLink>, TLink>(list, links.Constants.Continue);
                links.Each(filler.AddAndReturnConstant, restrictions);
            }

            return list;
        }

        /// <summary>
        /// Возвращает значение, определяющее существует ли связь с указанным индексом в хранилище связей.
        /// </summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="link">Индекс проверяемой на существование связи.</param>
        /// <returns>Значение, определяющее существует ли связь.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Exists<TLink>(this ILinks<TLink> links, TLink link) => Comparer<TLink>.Default.Compare(links.Count(link), default) > 0;

        /// <summary>
        /// Возвращает значение, определяющее существует ли связь с указанными началом и концом в хранилище связей.
        /// </summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="source">Начало связи.</param>
        /// <param name="target">Конец связи.</param>
        /// <returns>Значение, определяющее существует ли связь.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Exists<TLink>(this ILinks<TLink> links, TLink source, TLink target) => Comparer<TLink>.Default.Compare(links.Count(links.Constants.Any, source, target), default) > 0;

        /// <param name="links">Хранилище связей.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureLinkExists<TLink>(this ILinks<TLink> links, TLink link)
        {
            if (!links.Exists(link))
                throw new ArgumentLinkDoesNotExistsException<TLink>(link);
        }

        /// <param name="links">Хранилище связей.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureLinkExists<TLink>(this ILinks<TLink> links, TLink link, string argumentName)
        {
            if (!links.Exists(link))
                throw new ArgumentLinkDoesNotExistsException<TLink>(link, argumentName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureInnerReferenceExists<TLink>(this ILinks<TLink> links, TLink reference, string argumentName)
        {
            if (links.IsInnerReference(reference) && !links.Exists(reference))
                throw new ArgumentLinkDoesNotExistsException<TLink>(reference, argumentName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureInnerReferenceExists<TLink>(this ILinks<TLink> links, IList<TLink> restrictions, string argumentName)
        {
            for (int i = 0; i < restrictions.Count; i++)
                links.EnsureInnerReferenceExists(restrictions[i], argumentName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureLinkIsAnyOrExists<TLink>(this ILinks<TLink> links, IList<TLink> restrictions)
        {
            for (int i = 0; i < restrictions.Count; i++)
                links.EnsureLinkIsAnyOrExists(restrictions[i], nameof(restrictions));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureLinkIsAnyOrExists<TLink>(this ILinks<TLink> links, TLink link, string argumentName)
        {
            var equalityComparer = EqualityComparer<TLink>.Default;
            if (!equalityComparer.Equals(link, links.Constants.Any) && !links.Exists(link))
                throw new ArgumentLinkDoesNotExistsException<TLink>(link, argumentName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureLinkIsItselfOrExists<TLink>(this ILinks<TLink> links, TLink link, string argumentName)
        {
            var equalityComparer = EqualityComparer<TLink>.Default;
            if (!equalityComparer.Equals(link, links.Constants.Itself) && !links.Exists(link))
                throw new ArgumentLinkDoesNotExistsException<TLink>(link, argumentName);
        }

        /// <param name="links">Хранилище связей.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureDoesNotExists<TLink>(this ILinks<TLink> links, TLink source, TLink target)
        {
            if (links.Exists(source, target))
                throw new LinkWithSameValueAlreadyExistsException();
        }

        /// <param name="links">Хранилище связей.</param>
        public static ulong DependenciesCount<TLink>(this ILinks<TLink> links, TLink link)
        {
            var constants = links.Constants;

            var values = links.GetLink(link);

            ulong referencesAsSource = (Integer<TLink>)links.Count(constants.Any, link, constants.Any);

            var equalityComparer = EqualityComparer<TLink>.Default;

            if (equalityComparer.Equals(values[Constants.SourcePart], link))
                referencesAsSource--;

            ulong referencesAsTarget = (Integer<TLink>)links.Count(constants.Any, constants.Any, link);

            if (equalityComparer.Equals(values[Constants.TargetPart], link))
                referencesAsTarget--;

            return referencesAsSource + referencesAsTarget;
        }

        /// <param name="links">Хранилище связей.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool DependenciesExist<TLink>(this ILinks<TLink> links, TLink link) => links.DependenciesCount(link) > 0;

        /// <param name="links">Хранилище связей.</param>
        public static void EnsureNoDependencies<TLink>(this ILinks<TLink> links, TLink link)
        {
            if (links.DependenciesExist(link))
                throw new ArgumentLinkHasDependenciesException<TLink>(link);
        }

        /// <param name="links">Хранилище связей.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals<TLink>(this ILinks<TLink> links, TLink link, TLink source, TLink target)
        {
            var values = links.GetLink(link);
            var equalityComparer = EqualityComparer<TLink>.Default;
            return equalityComparer.Equals(values[Constants.SourcePart], source) && equalityComparer.Equals(values[Constants.TargetPart], target);
        }

        /// <summary>
        /// Выполняет поиск связи с указанными Source (началом) и Target (концом).
        /// </summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="source">Индекс связи, которая является началом для искомой связи.</param>
        /// <param name="target">Индекс связи, которая является концом для искомой связи.</param>
        /// <returns>Индекс искомой связи с указанными Source (началом) и Target (концом).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink SearchOrDefault<TLink>(this ILinks<TLink> links, TLink source, TLink target)
        {
            var setter = new Setter<TLink, TLink>(falseValue: links.Constants.Break);
            links.Each(setter.SetFirstAndReturnFalse, links.Constants.Any, source, target);
            return setter.Result;
        }

        /// <param name="links">Хранилище связей.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink CreatePoint<TLink>(this ILinks<TLink> links)
        {
            var link = links.Create();
            return links.Update(link, link, link);
        }

        /// <param name="links">Хранилище связей.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink CreateAndUpdate<TLink>(this ILinks<TLink> links, TLink source, TLink target) => links.Update(links.Create(), source, target);

        /// <summary>
        /// Обновляет связь с указанными началом (Source) и концом (Target)
        /// на связь с указанными началом (NewSource) и концом (NewTarget).
        /// </summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="link">Индекс обновляемой связи.</param>
        /// <param name="newSource">Индекс связи, которая является началом связи, на которую выполняется обновление.</param>
        /// <param name="newTarget">Индекс связи, которая является концом связи, на которую выполняется обновление.</param>
        /// <returns>Индекс обновлённой связи.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink Update<TLink>(this ILinks<TLink> links, TLink link, TLink newSource, TLink newTarget) => links.Update(new[] { link, newSource, newTarget });

        /// <summary>
        /// Обновляет связь с указанными началом (Source) и концом (Target)
        /// на связь с указанными началом (NewSource) и концом (NewTarget).
        /// </summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="restrictions">Ограничения на содержимое связей. Каждое ограничение может иметь значения: Constants.Null - 0-я связь, обозначающая ссылку на пустоту, Itself - требование установить ссылку на себя, 1..∞ конкретный адрес другой связи.</param>
        /// <returns>Индекс обновлённой связи.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink Update<TLink>(this ILinks<TLink> links, params TLink[] restrictions)
        {
            if (restrictions.Length == 2)
                return links.Merge(restrictions[0], restrictions[1]);
            if (restrictions.Length == 4)
                return links.UpdateOrCreateOrGet(restrictions[0], restrictions[1], restrictions[2], restrictions[3]);
            else
                return links.Update(restrictions);
        }

        /// <param name="links">Хранилище связей.</param>
        public static void EnsureCreated<TLink>(this ILinks<TLink> links, params TLink[] addresses) => links.EnsureCreated(links.Create, addresses);

        /// <param name="links">Хранилище связей.</param>
        public static void EnsurePointsCreated<TLink>(this ILinks<TLink> links, params TLink[] addresses) => links.EnsureCreated(links.CreatePoint, addresses);

        /// <param name="links">Хранилище связей.</param>
        public static void EnsureCreated<TLink>(this ILinks<TLink> links, Func<TLink> creator, params TLink[] addresses)
        {
            var constants = links.Constants;

            var nonExistentAddresses = new HashSet<ulong>(addresses.Where(x => !links.Exists(x)).Select(x => (ulong)(Integer<TLink>)x));
            if (nonExistentAddresses.Count > 0)
            {
                var max = nonExistentAddresses.Max();

                // TODO: Эту верхнюю границу нужно разрешить переопределять (проверить применяется ли эта логика)
                max = Math.Min(max, (Integer<TLink>)constants.MaxPossibleIndex);

                var createdLinks = new List<TLink>();

                var equalityComparer = EqualityComparer<TLink>.Default;

                TLink createdLink;
                while (!equalityComparer.Equals(createdLink = creator(), (TLink)(Integer<TLink>)max))
                    createdLinks.Add(createdLink);

                for (var i = 0; i < createdLinks.Count; i++)
                {
                    if (!nonExistentAddresses.Contains((Integer<TLink>)createdLinks[i]))
                        links.Delete(createdLinks[i]);
                }
            }
        }

        /// <summary>
        /// Создаёт связь (если она не существовала), либо возвращает индекс существующей связи с указанными Source (началом) и Target (концом).
        /// </summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="source">Индекс связи, которая является началом на создаваемой связи.</param>
        /// <param name="target">Индекс связи, которая является концом для создаваемой связи.</param>
        /// <returns>Индекс связи, с указанным Source (началом) и Target (концом)</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink GetOrCreate<TLink>(this ILinks<TLink> links, TLink source, TLink target)
        {
            var link = links.SearchOrDefault(source, target);
            if (EqualityComparer<TLink>.Default.Equals(link, default))
                link = links.CreateAndUpdate(source, target);
            return link;
        }

        /// <summary>
        /// Обновляет связь с указанными началом (Source) и концом (Target)
        /// на связь с указанными началом (NewSource) и концом (NewTarget).
        /// </summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="source">Индекс связи, которая является началом обновляемой связи.</param>
        /// <param name="target">Индекс связи, которая является концом обновляемой связи.</param>
        /// <param name="newSource">Индекс связи, которая является началом связи, на которую выполняется обновление.</param>
        /// <param name="newTarget">Индекс связи, которая является концом связи, на которую выполняется обновление.</param>
        /// <returns>Индекс обновлённой связи.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink UpdateOrCreateOrGet<TLink>(this ILinks<TLink> links, TLink source, TLink target, TLink newSource, TLink newTarget)
        {
            var equalityComparer = EqualityComparer<TLink>.Default;
            var link = links.SearchOrDefault(source, target);
            if (equalityComparer.Equals(link, default))
                return links.CreateAndUpdate(newSource, newTarget);
            if (equalityComparer.Equals(newSource, source) && equalityComparer.Equals(newTarget, target))
                return link;
            return links.Update(link, newSource, newTarget);
        }

        /// <summary>Удаляет связь с указанными началом (Source) и концом (Target).</summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="source">Индекс связи, которая является началом удаляемой связи.</param>
        /// <param name="target">Индекс связи, которая является концом удаляемой связи.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLink DeleteIfExists<TLink>(this ILinks<TLink> links, TLink source, TLink target)
        {
            var link = links.SearchOrDefault(source, target);
            if (!EqualityComparer<TLink>.Default.Equals(link, default))
            {
                links.Delete(link);
                return link;
            }
            return default;
        }

        /// <summary>Удаляет несколько связей.</summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="deletedLinks">Список адресов связей к удалению.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeleteMany<TLink>(this ILinks<TLink> links, IList<TLink> deletedLinks)
        {
            for (int i = 0; i < deletedLinks.Count; i++)
                links.Delete(deletedLinks[i]);
        }

        // Replace one link with another (replaced link is deleted, children are updated or deleted)
        public static TLink Merge<TLink>(this ILinks<TLink> links, TLink linkIndex, TLink newLink)
        {
            var equalityComparer = EqualityComparer<TLink>.Default;
            if (equalityComparer.Equals(linkIndex, newLink))
                return newLink;

            var constants = links.Constants;
            ulong referencesAsSourceCount = (Integer<TLink>)links.Count(constants.Any, linkIndex, constants.Any);
            ulong referencesAsTargetCount = (Integer<TLink>)links.Count(constants.Any, constants.Any, linkIndex);

            var isStandalonePoint = Point<TLink>.IsFullPoint(links.GetLink(linkIndex)) && referencesAsSourceCount == 1 && referencesAsTargetCount == 1;
            if (!isStandalonePoint)
            {
                var totalReferences = referencesAsSourceCount + referencesAsTargetCount;
                if (totalReferences > 0)
                {
                    var references = ArrayPool.Allocate<TLink>((long)totalReferences);
                    var referencesFiller = new ArrayFiller<TLink, TLink>(references, links.Constants.Continue);

                    links.Each(referencesFiller.AddFirstAndReturnConstant, constants.Any, linkIndex, constants.Any);
                    links.Each(referencesFiller.AddFirstAndReturnConstant, constants.Any, constants.Any, linkIndex);

                    for (ulong i = 0; i < referencesAsSourceCount; i++)
                    {
                        var reference = references[i];
                        if (equalityComparer.Equals(reference, linkIndex)) continue;
                        links.Update(reference, newLink, links.GetTarget(reference));
                    }
                    for (var i = (long)referencesAsSourceCount; i < references.Length; i++)
                    {
                        var reference = references[i];
                        if (equalityComparer.Equals(reference, linkIndex)) continue;
                        links.Update(reference, links.GetSource(reference), newLink);
                    }
                    ArrayPool.Free(references);
                }
            }

            links.Delete(linkIndex);

            return newLink;
        }
    }
}