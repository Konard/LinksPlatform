using System;
using System.Runtime.InteropServices;

namespace Platform.WindowsAPI
{
    public static class Kernel32
    {
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

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualAlloc(IntPtr blockAddress, UIntPtr sizeInBytes,
            MemoryAllocationType allocationType, MemoryProtection protection);

        public static IntPtr VirtualAlloc(UIntPtr sizeInBytes, MemoryAllocationType allocationType,
            MemoryProtection protection)
        {
            return VirtualAlloc(IntPtr.Zero, sizeInBytes, allocationType, protection);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualFree(IntPtr blockAddress, UIntPtr sizeInBytes, MemoryFreeType freeType);

        #endregion

        #endregion

        #region Heap Memory

        #region Enumerations

        [Flags]
        public enum HeapFlags : uint
        {
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

        [DllImport("kernel32", SetLastError = true)]
        public static extern IntPtr GetProcessHeap();

        [DllImport("kernel32", SetLastError = false)]
        public static extern IntPtr HeapAlloc(IntPtr heapHandle, HeapFlags flags, UIntPtr sizeInBytes);

        [DllImport("kernel32", SetLastError = true)]
        public static extern bool HeapFree(IntPtr heapHandle, HeapFlags flags, IntPtr blockAddress);

        // TODO: Узнать, как проверять конкретное значение SetLastError
        [DllImport("kernel32")]
        public static extern IntPtr HeapReAlloc(IntPtr heapHandle, HeapFlags flags, IntPtr blockAddress,
            UIntPtr sizeInBytes);

        [DllImport("kernel32")]
        public static extern UIntPtr HeapSize(IntPtr heapHandle, HeapFlags flags, IntPtr blockAddress);

        #endregion

        #endregion

        #region Memory Tools

        // TODO: Возможно RtlCopyMemory
        // TODO: Узнать когда нужно явно определять EntryPoint, нужно ли это в других ситуациях?
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr destinationAddress, IntPtr sourceAddress, UIntPtr sizeInBytes);

        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
        public static extern void MoveMemory(IntPtr destinationAddress, IntPtr sourceAddress, UIntPtr sizeInBytes);

        #endregion
    }
}