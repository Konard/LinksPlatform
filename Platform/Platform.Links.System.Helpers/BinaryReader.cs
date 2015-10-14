using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Platform.Links.System.Helpers.Disposal;

namespace Platform.Links.System.Helpers
{
    public class BinaryReader : DisposalBase
    {
        private readonly FileStream _file;

        public BinaryReader(string filename)
        {
            _file = File.Open(filename, FileMode.Open, FileAccess.Read);
        }

        public T Pull<T>()
            where T : struct
        {
            var size = Marshal.SizeOf(typeof(T));
            var buffer = new byte[size];
            var read = _file.Read(buffer, 0, size);
            return read < size ? default(T) : BitConverterExtensions.ToStructure<T>(buffer);
        }

        public List<T> ReadAll<T>()
            where T : struct
        {
            var elements = new List<T>();

            var size = Marshal.SizeOf(typeof(T));
            var buffer = new byte[size];

            while(true)
            {
                var read = _file.Read(buffer, 0, size);

                if (read < size)
                    break;

                var element = BitConverterExtensions.ToStructure<T>(buffer);
                elements.Add(element);
            }

            return elements;
        }

        public static List<T> ReadAll<T>(string filename)
            where T : struct
        {
            using (var reader = new BinaryReader(filename))
                return reader.ReadAll<T>();
        }

        protected override void DisposeCore(bool manual)
        {
            _file.Dispose();
        }
    }
}
