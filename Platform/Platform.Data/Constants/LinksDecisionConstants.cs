using Platform.Numbers;

namespace Platform.Data.Constants
{
    /// <remarks>
    /// Возможно каждая константа должна иметь своё уникальное значение (которое можно отсчитывать от конца доступных значений),
    /// например (ulong.MaxValue - 1) и т.п.
    /// 
    /// TODO: Должна быть возможность переопределить константы, т.е. нужен интерфейс. ILinksConstants
    /// </remarks>
    public class LinksDecisionConstants<TDecision> : ILinksDecisionConstants<TDecision>
    {
        // Cannot be supported anymore because of MathHelpers.Subtract usage. Cannot operate IL subtract directly on Integer. If needed later special code should be emitter for this case.
        //public readonly LinksConstants<Integer, Integer, Integer> Auto = Default<LinksConstants<Integer, Integer, Integer>>.Instance;

        /// <summary>Возвращает булевское значение, обозначающее продолжение прохода по связям.</summary>
        /// <remarks>Используется в функции обработчике, который передаётся в функцию Each.</remarks>
        public TDecision Continue { get; } = (Integer<TDecision>)ulong.MaxValue;

        /// <summary>Возвращает булевское значение, обозначающее остановку прохода по связям.</summary>
        /// <remarks>Используется в функции обработчике, который передаётся в функцию Each.</remarks>
        public TDecision Break { get; } = default; // The same as Null
    }
}
