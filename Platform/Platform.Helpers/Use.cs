using Platform.Disposables;

namespace Platform.Helpers
{
    public static class Use<T>
    {
        public static T Single => Global.Scope.Use<T>();

        public static Disposable<T> New
        {
            get
            {
                var scope = new Scope(autoInclude: true, autoExplore: true);
                return new Disposable<T, Scope>(scope.Use<T>(), scope);
            }
        }
    }
}

