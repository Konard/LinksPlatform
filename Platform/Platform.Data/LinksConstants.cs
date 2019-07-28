using Platform.Numbers;

namespace Platform.Data
{
    public interface ILinksDecisionConstants<TDecision>
    {
        /// <summary>Возвращает булевское значение, обозначающее продолжение прохода по связям.</summary>
        /// <remarks>Используется в функции обработчике, который передаётся в функцию Each.</remarks>
        TDecision Continue { get; }

        /// <summary>Возвращает булевское значение, обозначающее остановку прохода по связям.</summary>
        /// <remarks>Используется в функции обработчике, который передаётся в функцию Each.</remarks>
        TDecision Break { get; }

        //TDecision Skip { get; }
    }

    /// <remarks>
    /// Возможно каждая константа должна иметь своё уникальное значение (которое можно отсчитывать от конца доступных значений),
    /// например (ulong.MaxValue - 1) и т.п.
    /// 
    /// TODO: Должна быть возможность переопределить константы, т.е. нужен интерфейс. ILinksConstants
    /// </remarks>
    public class LinksConstants<TDecision> : ILinksDecisionConstants<TDecision>
    {
        // Cannot be supported anymore because of MathHelpers.Subtract usage. Cannot operate IL subtract directly on Integer. If needed later special code should be emitter for this case.
        //public readonly LinksConstants<Integer, Integer, Integer> Auto = Default<LinksConstants<Integer, Integer, Integer>>.Instance;

        /// <summary>Возвращает булевское значение, обозначающее продолжение прохода по связям.</summary>
        /// <remarks>Используется в функции обработчике, который передаётся в функцию Each.</remarks>
        public TDecision Continue { get; } = (Integer<TDecision>)ulong.MaxValue;

        /// <summary>Возвращает булевское значение, обозначающее остановку прохода по связям.</summary>
        /// <remarks>Используется в функции обработчике, который передаётся в функцию Each.</remarks>
        public TDecision Break { get; } = default; // The same as Null

        //public TDecision Skip { get; } = Integer<TDecision>.One; // TODO: Подумать над корректной константой
    }

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

    public class LinksConstants<TDecision, TAddress> : LinksConstants<TDecision>, ILinksAddressConstants<TAddress>
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

        public LinksConstants()
        {
            Null = Integer<TAddress>.Zero;
            MinPossibleIndex = Integer<TAddress>.One;
            MaxPossibleIndex = ArithmeticHelpers.Subtract<TAddress>(ulong.MaxValue, 3);

            Itself = ArithmeticHelpers.Subtract<TAddress>(ulong.MaxValue, 2);
            Any = ArithmeticHelpers.Subtract<TAddress>(ulong.MaxValue, 1);
            // ulong.MaxValue is reserved for "Continue"
        }
    }

    public interface ILinksPartConstants<TPartIndex>
    {
        /// <summary>Возвращает индекс части, которая отвечает за индекс самой связи.</summary>
        TPartIndex IndexPart { get; }

        /// <summary>Возвращает индекс части, которая отвечает за ссылку на связь-начало.</summary>
        TPartIndex SourcePart { get; }

        /// <summary>Возвращает индекс части, которая отвечает за ссылку на связь-конец.</summary>
        TPartIndex TargetPart { get; }
    }

    public class LinksConstants<TDecision, TAddress, TPartIndex> : LinksConstants<TDecision, TAddress>, ILinksCombinedConstants<TDecision, TAddress, TPartIndex>
    {
        public TPartIndex IndexPart { get; } = Integer<TPartIndex>.Zero;

        public TPartIndex SourcePart { get; } = Integer<TPartIndex>.One;

        public TPartIndex TargetPart { get; } = Integer<TPartIndex>.Two;
    }

    public interface ILinksCombinedConstants<TDecision, TAddress, TPartIndex> :
        ILinksDecisionConstants<TDecision>,
        ILinksAddressConstants<TAddress>,
        ILinksPartConstants<TPartIndex>
    {
    }
}
