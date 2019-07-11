using System.IO;

namespace Platform.Memory
{
    /// <summary>
    /// Represents a memory block stored as a temporary file on disk.
    /// Представляет блок памяти, хранящийся в виде временного файла на диске.
    /// </summary>
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
