using Platform.Numbers;

namespace Platform.Data.Constants
{
    public class LinksCombinedConstants<TDecision, TAddress> : LinksDecisionConstants<TDecision>, ILinksAddressConstants<TAddress>
    {
        /// <summary>Возвращает значение, обозначающее отсутствие связи.</summary>
        public TAddress Null { get; }

        /// <summary>Возвращает значение, обозначающее любую связь.</summary>
        /// <remarks>Возможно нужно зарезервировать отдельное значение, тогда можно будет создавать все варианты последовательностей в функции Create.</remarks>
        public TAddress Any { get; }

        /// <summary>Возвращает значение, обозначающее связь-ссылку на саму связь.</summary>
        public TAddress Itself { get; }

        /// <summary>Возвращает минимально возможный индекс существующей связи.</summary>
        public TAddress MinPossibleIndex { get; }

        /// <summary>Возвращает максимально возможный индекс существующей связи.</summary>
        /// <remarks>
        /// Если за каждую константу будет отвечать отдельная связь, диапазон возможных связей будет уменьшен.
        /// Благодаря логике конвертации Integer для каждого типа здесь будет максимальное значение этого типа.
        /// </remarks>
        public TAddress MaxPossibleIndex { get; }

        public LinksCombinedConstants()
        {
            Null = Integer<TAddress>.Zero;
            MinPossibleIndex = Integer<TAddress>.One;
            MaxPossibleIndex = ArithmeticHelpers.Subtract<TAddress>(ulong.MaxValue, 3);

            Itself = ArithmeticHelpers.Subtract<TAddress>(ulong.MaxValue, 2);
            Any = ArithmeticHelpers.Subtract<TAddress>(ulong.MaxValue, 1);
            // ulong.MaxValue is reserved for "Continue"
        }
    }
}
