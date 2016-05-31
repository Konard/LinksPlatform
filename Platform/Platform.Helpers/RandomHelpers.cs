using System;

namespace Platform.Helpers
{
    /// <remarks>
    /// TODO: Возможно нужна отдельная точка доступа к фабрикам по их типу.
    /// Например Factory[Random].Default чтобы обратиться к настраиваемой (переопределяемой) фабрике по умолчанию для этого типа.
    /// </remarks>
    public static class RandomHelpers
    {
        public static readonly Random DefaultFactory = Singleton.Get(() => new Random((int)DateTime.UtcNow.Ticks));
    }
}
