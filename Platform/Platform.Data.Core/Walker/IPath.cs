namespace Platform.Data.Core.Walker
{
    public interface IPath<TMethod, TMilestone>
    {
        void Step(TMethod method, TMilestone milestone);
    }
}
