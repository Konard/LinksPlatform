using System;
using Platform.Disposables;
using Platform.Helpers.Unsafe;

namespace Platform.Memory
{
    /// <summary>
    /// Represents adapter to a memory block with access via indexer.
    /// Представляет адаптер к блоку памяти с доступом через индексатор.
    /// </summary>
    /// <typeparam name="TElement">Element type. Тип элемента.</typeparam>
    public class DirectMemoryAsArrayMemoryAdapter<TElement> : DisposableBase, IArrayMemory<TElement>, IDirectMemory
        where TElement : struct
    {
        #region Constants

        public static readonly long ElementSize = StructureHelpers.SizeOf<TElement>();

        #endregion

        #region Fields

        private readonly IDirectMemory _memory;

        #endregion

        #region Properties

        public long Size => _memory.Size;

        public IntPtr Pointer => _memory.Pointer;

        public TElement this[long index]
        {
            get => Pointer.GetElement(ElementSize, index).GetValue<TElement>();
            set => Pointer.GetElement(ElementSize, index).SetValue(value);
        }

        #endregion

        #region DisposableBase Properties

        protected override string ObjectName => $"Array as memory block at '{Pointer}' address.";

        #endregion

        #region Constructors

        public DirectMemoryAsArrayMemoryAdapter(IDirectMemory memory)
        {
            _memory = memory;

            if (_memory.Size % ElementSize > 0)
                throw new ArgumentException("Memory is not aligned to element size.", nameof(memory));
        }

        #endregion

        #region DisposableBase Methods

        protected override void DisposeCore(bool manual, bool wasDisposed) => Disposable.TryDispose(_memory);

        #endregion
    }
}
