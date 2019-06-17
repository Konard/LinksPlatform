using System.IO;

namespace Platform.Memory
{
    public class TemporaryFileMappedResizableDirectMemory : FileMappedResizableDirectMemory
    {
        public TemporaryFileMappedResizableDirectMemory(long minimumReservedCapacity)
            : base(Path.GetTempFileName(), minimumReservedCapacity)
        {
        }

        public TemporaryFileMappedResizableDirectMemory()
            : this(MinimumCapacity)
        {
        }

        protected override void DisposeCore(bool manual, bool wasDisposed)
        {
            base.DisposeCore(manual, wasDisposed);

            if (!wasDisposed)
                File.Delete(Address);
        }
    }
}
