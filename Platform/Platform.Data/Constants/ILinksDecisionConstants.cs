namespace Platform.Data.Constants
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
}
