using System;

namespace Platform.Helpers
{
    public static class Global
    {
        /// <summary>
        /// Представляет поле-помойку, куда можно сбрасывать ненужные значения.
        /// В некоторых случаях это может помочь избежать нежелательной оптимизации
        /// (исключения записи в переменную в конце функции)
        /// и сделать вид, что значение действительно используется. Фактически
        /// таким образом мы обманываем компилятор. Такое может быть полезно при
        /// реализации тестов на производительность.
        /// </summary>
        public static object Trash = Default<object>.Instance;

        public static Scope Scope = new Scope(autoInclude: true, autoExplore: true);

        public static event EventHandler<Exception> IgnoredException = (sender, exception) => { }; // TODO: Change default handler later

        public static void OnIgnoredException(Exception exception) => IgnoredException.Invoke(null, exception);
    }
}