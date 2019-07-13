namespace Platform.Helpers.Random
{
    /// <remarks>
    /// TODO: Возможно нужна отдельная точка доступа к фабрикам по их типу.
    /// Например Factory[Random].Default чтобы обратиться к настраиваемой (переопределяемой) фабрике по умолчанию для этого типа.
    /// </remarks>
    public static class RandomHelpers
    {
        public static readonly System.Random DefaultFactory = Singleton.Get(() => new System.Random((int)System.DateTime.UtcNow.Ticks));
    }
}
