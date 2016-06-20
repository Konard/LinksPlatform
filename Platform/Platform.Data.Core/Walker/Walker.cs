using System.Threading.Tasks;

namespace Platform.Data.Core.Walker
{
    internal abstract class Walker<TMethod, TMilestone>
    {
        public IPath<TMethod, TMilestone> Path { get; }
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

            var task = new Task(() => { while (IsWalking) Step(); });
            task.Start();
            return task;
        }

        public void Step()
        {
            var method = Decide();
            var milestone = Move(method);
            Path.Step(method, milestone);
        }

        public void Stop() => IsWalking = false;

        protected abstract TMilestone Move(TMethod method);

        protected abstract TMethod Decide();
    }
}
