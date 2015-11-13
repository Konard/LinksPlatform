using System.Threading.Tasks;

namespace Platform.Data.Core.Walker
{
    public abstract class Walker<TMethod, TMilestone>
    {
        public IPath<TMethod, TMilestone> Path { get; private set; }
        public bool IsWalking { get; private set; }

        protected Walker(IPath<TMethod, TMilestone> path)
        {
            IsWalking = false;
            Path = path;
        }

        public void Start()
        {
            IsWalking = true;

            while (IsWalking) Step();
        }

        public Task StartAsync()
        {
            IsWalking = true;

            return new Task(() => { while (IsWalking) Step(); });
        }

        public void Step()
        {
            var method = Handle();
            var milestone = Move(method);
            Path.Step(method, milestone);
        }

        public void Stop()
        {
            IsWalking = false;
        }

        protected abstract TMilestone Move(TMethod method);

        protected abstract TMethod Handle();
    }
}
