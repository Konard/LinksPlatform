namespace Platform.Data.Core.Pairs
{
    /// <remarks>
    /// Связь точка - это связь, у которой начало (Source) и конец (Target) есть сама эта связь.
    /// </remarks>
    partial class Links
    {
        /// <summary>
        /// Возвращает значение, определяющее является ли связь с указанным индексом точкой (связью замкнутой на себе).
        /// </summary>
        /// <param name="link">Индекс проверяемой на существование связи.</param>
        /// <returns>Значение, определяющее является ли связь точкой.</returns>
        public bool IsPoint(ulong link)
        {
            return _sync.ExecuteReadOperation(() => IsPointCore(link));
        }

        /// <remarks>
        /// Вероятно следует изменить логику проверки, так чтобы достаточно было одной любой ссылки на себя.
        /// Также в будущем можно будет проверять и всех родителей, чтобы проверить есть ли ссылки на себя (на эту связь).
        /// </remarks>
        private bool IsPointCore(ulong link)
        {
            if (!_memoryManager.Exists(link)) return false;

            var values = _memoryManager.GetLinkValue(link);
            return values[LinksConstants.SourcePart] == link && values[LinksConstants.TargetPart] == link;
        }
    }
}