namespace Platform.Data.Doublets
{
    public struct Doublet<T>
    {
        public T Source;
        public T Target;

        public Doublet(T source, T target)
        {
            Source = source;
            Target = target;
        }

        public override string ToString() => $"{Source}->{Target}";
    }
}
