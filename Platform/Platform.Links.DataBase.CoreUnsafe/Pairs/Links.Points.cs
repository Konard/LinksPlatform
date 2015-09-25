namespace Platform.Links.DataBase.CoreUnsafe.Pairs
{
    /// <remarks>
    /// Связь точка - это связь, у которой начало (Source) и конец (Target) есть сама эта связь.
    /// </remarks>
    unsafe partial class Links
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

        private bool IsPointCore(ulong link)
        {
            return Exists(link) && _links[link].Source == link && _links[link].Target == link;
        }
    }
}