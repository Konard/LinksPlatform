using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using Platform.Data.Core.Doublets;

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
        public static IEnumerable<T> Execute<T>(this SynchronizedLinks<ulong> links,
            Expression<Func<SynchronizedLinks<ulong>, IEnumerable<T>>> query)
        {
            var queryId = query.ToString();

            var compiledQuery = CompiledQueriesCache<T>.CompiledQueries.GetOrAdd(queryId, key => query.Compile());

            return compiledQuery(links);
        }

        private static class CompiledQueriesCache<T>
        {
            public static readonly ConcurrentDictionary<string, Func<SynchronizedLinks<ulong>, IEnumerable<T>>>
                CompiledQueries =
                    new ConcurrentDictionary<string, Func<SynchronizedLinks<ulong>, IEnumerable<T>>>();
        }
    }
}