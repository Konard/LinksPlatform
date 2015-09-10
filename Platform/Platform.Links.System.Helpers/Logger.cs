using System.IO;
using Platform.Links.System.Helpers.Disposal;

namespace Platform.Links.System.Helpers
{
    /// <summary>
    /// Представляет логгер (объект, который занимается записью в лог).
    /// </summary>
    public class Logger : DisposalBase
    {
        private readonly FileStream _file;

        public Logger(string filename)
        {
            _file = File.Open(filename, FileMode.Append, FileAccess.Write);
        }

        public void Push<T>(T value)
            where T : struct
        {
            var bytes = BitConverterExtensions.GetBytes(value);
            _file.Write(bytes, 0, bytes.Length);
        }

        protected override void DisposeCore(bool manual)
        {
            _file.Dispose();
        }
    }
}