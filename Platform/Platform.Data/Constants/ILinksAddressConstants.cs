namespace Platform.Data.Constants
{
    public interface ILinksAddressConstants<TAddress>
    {
        /// <summary>Возвращает значение, обозначающее отсутствие связи.</summary>
        TAddress Null { get; }

        /// <summary>Возвращает значение, обозначающее любую связь.</summary>
        /// <remarks>Возможно нужно зарезервировать отдельное значение, тогда можно будет создавать все варианты последовательностей в функции Create.</remarks>
        TAddress Any { get; }

        /// <summary>Возвращает значение, обозначающее связь-ссылку на саму связь.</summary>
        TAddress Itself { get; }

        /// <summary>Возвращает минимально возможный индекс существующей связи.</summary>
        TAddress MinPossibleIndex { get; }

        /// <summary>Возвращает максимально возможный индекс существующей связи.</summary>
        /// <remarks>
        /// Если за каждую константу будет отвечать отдельная связь, диапазон возможных связей будет уменьшен.
        /// Благодаря логике конвертации Integer для каждого типа здесь будет максимальное значение этого типа.
        /// </remarks>
        TAddress MaxPossibleIndex { get; }
    }
}
