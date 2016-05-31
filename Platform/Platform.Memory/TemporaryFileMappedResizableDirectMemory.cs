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

        protected override void DisposeCore(bool manual)
        {
            base.DisposeCore(manual);

            File.Delete(Address);
        }
    }
}
