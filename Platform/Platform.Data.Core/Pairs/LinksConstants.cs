namespace Platform.Data.Core.Pairs
{
    /// <remarks>
    /// Возможно каждая константа должна иметь своё уникальное значение (которое можно отсчитывать от конца доступных значений),
    /// например (ulong.MaxValue - 1) и т.п.
    /// </remarks>
    public static class LinksConstants
    {
        /// <summary>Возвращает булевское значение, обозначающее продолжение прохода по связям.</summary>
        /// <remarks>Используется в функции обработчике, который передаётся в функцию Each.</remarks>
        public const bool Continue = true;

        /// <summary>Возвращает булевское значение, обозначающее остановку прохода по связям.</summary>
        /// <remarks>Используется в функции обработчике, который передаётся в функцию Each.</remarks>
        public const bool Break = false;

        /// <summary>Возвращает значение ulong, обозначающее отсутствие связи.</summary>
        public const ulong Null = 0;

        /// <summary>Возвращает значение ulong, обозначающее любую связь.</summary>
        /// <remarks>Возможно нужно зарезервировать отдельное значение, тогда можно будет создавать все варианты последовательностей в функции Create.</remarks>
        public const ulong Any = 0;

        /// <summary>Возвращает значение ulong, обозначающее связь-ссылку на саму связь.</summary>
        public const ulong Itself = 0;

        /// <summary>Возвращает индекс части, которая отвечает за ссылку на связь-начало.</summary>
        public const long SourcePart = 0;

        /// <summary>Возвращает индекс части, которая отвечает за ссылку на связь-конец.</summary>
        public const long TargetPart = 1;

        /// <summary>Возвращает минимально возможный индекс существующей связи.</summary>
        public const ulong MinPossibleIndex = 1;

        /// <summary>Возвращает максимально возможный индекс существующей связи.</summary>
        /// <remarks>Если за каждую константу будет отвечать отдельная связь, диапазон возможных связей будет уменьшен.</remarks>
        public const ulong MaxPossibleIndex = ulong.MaxValue;
    }
}
