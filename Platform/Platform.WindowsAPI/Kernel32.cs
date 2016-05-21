using System;
using System.Runtime.InteropServices;

namespace Platform.WindowsAPI
{
    public static unsafe class Kernel32
    {
        private const string Kernel32LibraryName = "kernel32";
        private static readonly IntPtr CurrentProcessHeapHandle = GetProcessHeapOrFail();

        #region Virtual Memory

        #region Enumerations

        [Flags]
        public enum MemoryAllocationType : uint
        {
            Commit
                = 0x00001000,

            Reserve
                = 0x00002000,

            Reset
                = 0x00080000,

            ResetUndo
                = 0x01000000,

            LargePages
                = 0x20000000,

            Physical
                = 0x00400000,

            TopDown
                = 0x00100000,

            WriteWatch
                = 0x00200000
        }

        [Flags]
        public enum MemoryFreeType : uint
        {
            Decommit
                = 0x4000,

            Release
                = 0x8000
        }

        [Flags]
        public enum MemoryProtection : uint
        {
            NoAccess
                = 0x001,

            Readonly
                = 0x002,

            ReadWrite
                = 0x004,

            WriteCopy
                = 0x008,

            Execute
                = 0x010,

            ExecuteRead
                = 0x020,

            ExecuteReadWrite
                = 0x040,

            ExecuteWriteCopy
                = 0x080,

            GuardModifierFlag
                = 0x100,

            NoCacheModifierFlag
                = 0x200,

            WriteCombineModifierFlag
                = 0x400
        }

        #endregion

        #region Methods

        [DllImport(Kernel32LibraryName, SetLastError = true)]
        public static extern IntPtr VirtualAlloc(IntPtr blockAddress, UIntPtr sizeInBytes, MemoryAllocationType allocationType, MemoryProtection protection);

        /// <summary>
        /// Reserves, commits, or changes the state of a region of pages in the virtual address space of the calling process.
        /// Memory allocated by this function is automatically initialized to zero.
        /// </summary>
        /// <param name="sizeInBytes">The size of the region, in bytes</param>
        /// <param name="allocationType">The type of memory allocation.</param>
        /// <param name="protection">The memory protection for the region of pages to be allocated.</param>
        /// <returns>The base address of the allocated region of pages.</returns>
        public static void* VirtualAlloc(ulong sizeInBytes, MemoryAllocationType allocationType, MemoryProtection protection)
        {
            var pointer = VirtualAlloc(IntPtr.Zero, new UIntPtr(sizeInBytes), allocationType, protection).ToPointer();
            if (pointer == null)
                throw new InvalidOperationException(Marshal.GetLastWin32Error().ToString());
            return pointer;
        }

        [DllImport(Kernel32LibraryName, SetLastError = true)]
        public static extern bool VirtualFree(IntPtr blockAddress, UIntPtr sizeInBytes, MemoryFreeType freeType);

        /// <summary>
        /// Releases, decommits, or releases and decommits a region of pages within the virtual address space of the calling process.
        /// </summary>
        /// <param name="block">A pointer to the base address of the region of pages to be freed.</param>
        /// <param name="sizeInBytes">The size of the region of memory to be freed, in bytes.</param>
        /// <param name="freeType">The type of free operation.</param>
        public static void VirtualFree(void* block, ulong sizeInBytes, MemoryFreeType freeType)
        {
            if (!VirtualFree(new IntPtr(block), new UIntPtr(sizeInBytes), freeType))
                throw new InvalidOperationException(Marshal.GetLastWin32Error().ToString());
        }

        #endregion

        #endregion

        #region Heap Memory

        #region Enumerations

        [Flags]
        public enum HeapFlags : uint
        {
            NoFlags
                = 0x00000000,

            NoSerialize
                = 0x00000001,

            Growable
                = 0x00000002,

            GenerateExceptions
                = 0x00000004,

            ZeroMemory
                = 0x00000008,

            ReallocInPlaceOnly
                = 0x00000010,

            TailCheckingEnabled
                = 0x00000020,

            FreeCheckingEnabled
                = 0x00000040,

            DisableCoalesceOnFree
                = 0x00000080,

            CreateAlign16
                = 0x00010000,

            CreateEnableTracing
                = 0x00020000,

            CreateEnableExecute
                = 0x00040000
        }

        #endregion

        #region Methods

        [DllImport(Kernel32LibraryName, SetLastError = true)]
        public static extern IntPtr GetProcessHeap();

        public static IntPtr GetProcessHeapOrFail()
        {
            var pointer = GetProcessHeap();
            if (pointer == IntPtr.Zero)
                throw new InvalidOperationException(Marshal.GetLastWin32Error().ToString());
            return pointer;
        }

        [DllImport(Kernel32LibraryName)]
        public static extern IntPtr HeapAlloc(IntPtr heapHandle, HeapFlags flags, UIntPtr sizeInBytes);

        /// <summary>
        /// Allocates a memory block of given size. 
        /// The allocated memory is automatically initialized to zero.
        /// </summary>
        /// <param name="size">The size of a memory block.</param>
        /// <returns>The pointer to a memory block.</returns>
        public static void* HeapAlloc(ulong size)
        {
            var result = HeapAlloc(CurrentProcessHeapHandle, HeapFlags.ZeroMemory, new UIntPtr(size)).ToPointer();
            if (result == null) throw new OutOfMemoryException();
            return result;
        }

        [DllImport(Kernel32LibraryName, SetLastError = true)]
        public static extern bool HeapFree(IntPtr heapHandle, HeapFlags flags, IntPtr blockAddress);

        /// <summary>
        /// Frees a memory block.
        /// </summary>
        /// <param name="block">The pointer to a memory block to be freed.</param>
        public static void HeapFree(void* block)
        {
            if (!HeapFree(CurrentProcessHeapHandle, HeapFlags.NoFlags, new IntPtr(block)))
                throw new InvalidOperationException(Marshal.GetLastWin32Error().ToString());
        }

        [DllImport(Kernel32LibraryName)]
        public static extern IntPtr HeapReAlloc(IntPtr heapHandle, HeapFlags flags, IntPtr blockAddress, UIntPtr sizeInBytes);

        /// <summary>
        /// Re-allocates a memory block. If the reallocation request is for a
        /// larger size, the additional region of memory is automatically initialized to zero.
        /// </summary>
        /// <param name="block">The pointer to a memory block.</param>
        /// <param name="size">The size (bytes) of a memory block.</param>
        /// <returns>The pointer to a new memory block location.</returns>
        public static void* HeapReAlloc(void* block, ulong size)
        {
            var pointer = HeapReAlloc(CurrentProcessHeapHandle, HeapFlags.ZeroMemory, new IntPtr(block), new UIntPtr(size)).ToPointer();
            if (pointer == null) throw new OutOfMemoryException();
            return pointer;
        }

        [DllImport(Kernel32LibraryName)]
        public static extern UIntPtr HeapSize(IntPtr heapHandle, HeapFlags flags, IntPtr blockAddress);

        /// <summary>
        /// Returns the size of a memory block.
        /// </summary>
        /// <param name="block">The pointer to a memory block.</param>
        /// <returns>The size of a memory block.</returns>
        public static ulong HeapSize(void* block)
        {
            return HeapSize(CurrentProcessHeapHandle, HeapFlags.NoFlags, new IntPtr(block)).ToUInt64();
        }

        #endregion

        #endregion

        #region Memory Tools

        [DllImport(Kernel32LibraryName, EntryPoint = "RtlCopyMemory")]
        public static extern void CopyMemory(IntPtr destinationAddress, IntPtr sourceAddress, UIntPtr sizeInBytes);

        /// <summary>
        /// Copies data of a memory block with a given size from source address to destination address.
        /// The source and destination blocks are permitted to overlap.
        /// </summary>
        /// <param name="destination">The pointer to destination where a memory block will be copied.</param>
        /// <param name="source">The pointer to a source memory block.</param>
        /// <param name="size">The size of a memory block.</param>
        public static void CopyMemory(void* destination, void* source, ulong size)
        {
            CopyMemory(new IntPtr(destination), new IntPtr(source), new UIntPtr(size));
        }

        [DllImport(Kernel32LibraryName, EntryPoint = "RtlMoveMemory")]
        public static extern void MoveMemory(IntPtr destinationAddress, IntPtr sourceAddress, UIntPtr sizeInBytes);

        /// <summary>
        /// Moves a block of memory from one location to another.
        /// </summary>
        /// <param name="destination">The pointer to destination where a memory block will be moved.</param>
        /// <param name="source">The pointer to a source memory block.</param>
        /// <param name="size">The size of a memory block.</param>
        public static void MoveMemory(void* destination, void* source, ulong size)
        {
            MoveMemory(new IntPtr(destination), new IntPtr(source), new UIntPtr(size));
        }

        #endregion
    }
}