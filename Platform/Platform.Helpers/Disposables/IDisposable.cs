namespace Platform.Helpers.Disposables
{
    /// <summary>
    /// Представляет расширенный интерфейс IDisposable.
    /// Represents an extended IDisposable interface.
    /// </summary>
    public interface IDisposable : System.IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether the object was disposed.
        /// Возвращает значение определяющее был ли высвобожден объект.
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// Выполняет определенные пользователем задачи, связанные с освобождением, высвобождением или сбросом неуправляемых ресурсов.
        /// </summary>
        /// <remarks>
        /// Should be called only from classes destructors.
        /// Должен вызываться только из деструкторов классов.
        /// </remarks>
        void Destruct();
    }
}