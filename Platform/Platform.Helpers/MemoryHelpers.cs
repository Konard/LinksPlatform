using System;

namespace Platform.Helpers
{
    public class MemoryHelpers
    {
        /// <summary>
        /// Возвращает размер страницы виртуальной памяти в байтах, установленный в операционной системе.
        /// </summary>
        public static readonly long SystemPageSize = Environment.SystemPageSize;

        public static void AlignSizeToSystemPageSize(ref long size)
        {
            if (size == 0)
                size = SystemPageSize;
            if (size%SystemPageSize != 0)
                size = (size/SystemPageSize + 1)*SystemPageSize;
        }
    }
}