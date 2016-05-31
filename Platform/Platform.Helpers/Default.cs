namespace Platform.Helpers
{
    /// <summary>
    /// Представляет собой точку доступа к экземплярям типов по умолчанию (созданных с помощью конструктора без аргументов).
    /// </summary>
    /// <typeparam name="T">Тип экземпляра объекта.</typeparam>
    public class Default<T>
        where T : new()
    {
        public static readonly T Instance = new T();
    }
}
