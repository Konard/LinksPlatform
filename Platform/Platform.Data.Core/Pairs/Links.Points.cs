namespace Platform.Data.Core.Pairs
{
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
    /// например "PairOf" обозначить что является точно парой, а что точно точкой.
    /// И наоборот этот же метод поможет, если уже существует точка, но нам нужна пара.
    /// </remarks>
    partial class Links
    {
        /// <summary>Возвращает значение, определяющее является ли связь с указанным индексом точкой полностью (связью замкнутой на себе дважды).</summary>
        /// <param name="link">Индекс проверяемой связи.</param>
        /// <returns>Значение, определяющее является ли связь точкой полностью.</returns>
        public bool IsFullPoint(ulong link)
        {
            return _sync.ExecuteReadOperation(() =>
            {
                EnsureLinkExists(link);
                return IsFullPointCore(link);
            });
        }

        public bool IsFullPointCore(ulong link)
        {
            var values = _memoryManager.GetLinkValue(link);
            return values[LinksConstants.SourcePart] == link && values[LinksConstants.TargetPart] == link;
        }

        /// <summary>Возвращает значение, определяющее является ли связь с указанным индексом точкой частично (связью замкнутой на себе как минимум один раз).</summary>
        /// <param name="link">Индекс проверяемой связи.</param>
        /// <returns>Значение, определяющее является ли связь точкой частично.</returns>
        public bool IsPartialPoint(ulong link)
        {
            return _sync.ExecuteReadOperation(() =>
            {
                EnsureLinkExists(link);
                return IsFullPointCore(link);
            });
        }

        /// <remarks>
        /// Достаточно любой одной ссылки на себя.
        /// Также в будущем можно будет проверять и всех родителей, чтобы проверить есть ли ссылки на себя (на эту связь).
        /// </remarks>
        public bool IsPartialPointCore(ulong link)
        {
            var values = _memoryManager.GetLinkValue(link);
            return values[LinksConstants.SourcePart] == link || values[LinksConstants.TargetPart] == link;
        }
    }
}