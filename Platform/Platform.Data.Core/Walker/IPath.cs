namespace Platform.Data.Core.Walker
{
    internal interface IPath<TMethod, TMilestone>
    {
        void Step(TMethod method, TMilestone milestone);
    }
}