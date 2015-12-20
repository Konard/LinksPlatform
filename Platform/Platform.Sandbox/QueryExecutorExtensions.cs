using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using Platform.Data.Core.Pairs;

namespace Platform.Sandbox
{
    /// <summary>
    ///     Представляет класс-контейнер расширений для выполнения произвольных запросов над Links
    /// </summary>
    public static class QueryExecutorExtensions
    {
        /// <summary>
        ///     Выполняет запрос query над links и возвращает результат запроса в виде перечисляемого объекта с элементами типа T.
        /// </summary>
        /// <typeparam name="T">Тип элемента запроса.</typeparam>
        /// <param name="links">База данных связей, над которой будет выполняться запрос.</param>
        /// <param name="query">Запрос в виде Linq-выражения.</param>
        /// <returns>Результат запроса в виде перечисляемого объекта с элементами типа T.</returns>
        public static IEnumerable<T> Execute<T>(this Links links,
            Expression<Func<Links, IEnumerable<T>>> query)
        {
            var queryId = query.ToString();
            Func<Links, IEnumerable<T>> compiledQuery;
            if (!CompiledQueriesCache<T>.compiledQueries.TryGetValue(queryId, out compiledQuery))
            {
                compiledQuery = query.Compile(); // Подумать как повлиять на процесс компиляции запроса
                CompiledQueriesCache<T>.compiledQueries.TryAdd(queryId, compiledQuery);
            }

            return compiledQuery(links);

            //yield return default(T);

            //return Enumerable.Empty<T>();
        }

        private static class CompiledQueriesCache<T>
        {
            public static readonly ConcurrentDictionary<string, Func<Links, IEnumerable<T>>>
                compiledQueries =
                    new ConcurrentDictionary<string, Func<Links, IEnumerable<T>>>();
        }
    }
}