using System.IO;

namespace Platform.Memory
{
    public class TemporaryFileMappedResizableDirectMemory : FileMappedResizableDirectMemory
    {
        #region DisposableBase Properties

        protected override string ObjectName => $"Temporary file stored memory block at '{Address}' path.";

        #endregion

        #region Constructors

        public TemporaryFileMappedResizableDirectMemory(long minimumReservedCapacity)
            : base(Path.GetTempFileName(), minimumReservedCapacity)
        {
        }

        public TemporaryFileMappedResizableDirectMemory()
            : this(MinimumCapacity)
        {
        }

        #endregion

        #region DisposableBase Methods

        protected override void DisposeCore(bool manual, bool wasDisposed)
        {
            base.DisposeCore(manual, wasDisposed);

            if (!wasDisposed)
                File.Delete(Address);
        }

        #endregion
    }
}
