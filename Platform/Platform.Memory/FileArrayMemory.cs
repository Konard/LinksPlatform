using System.IO;
using Platform.Helpers.Disposables;
using Platform.Helpers.IO;
using Platform.Helpers.Unsafe;

namespace Platform.Memory
{
    /// <summary>
    /// Represents a memory block with access via indexer and stored as file on disk.
    /// Представляет блок памяти с доступом через индексатор и хранящийся в виде файла на диске.
    /// </summary>
    /// <typeparam name="TElement">Element type. Тип элемента.</typeparam>
    public class FileArrayMemory<TElement> : DisposableBase, IArrayMemory<TElement> //-V3073
        where TElement : struct
    {
        #region Constants

        public static readonly long ElementSize = UnsafeHelpers.SizeOf<TElement>();

        #endregion

        #region Fields

        private readonly string _address;
        private readonly FileStream _file;

        #endregion

        #region Properties

        public long Size => _file.Length;

        public TElement this[long index]
        {
            get
            {
                _file.Seek(ElementSize * index, SeekOrigin.Begin);
                return _file.ReadOrDefault<TElement>();
            }
            set
            {
                _file.Seek(ElementSize * index, SeekOrigin.Begin);
                _file.Write(value);
            }
        }

        #endregion

        #region DisposableBase Properties

        protected override string ObjectName => $"File stored memory block at '{_address}' path.";

        #endregion

        #region Contructors

        public FileArrayMemory(string address)
        {
            _address = address;
            _file = File.Open(address, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }

        #endregion

        #region DisposableBase Methods

        protected override void DisposeCore(bool manual, bool wasDisposed) => Disposable.TryDispose(_file);

        #endregion
    }
}