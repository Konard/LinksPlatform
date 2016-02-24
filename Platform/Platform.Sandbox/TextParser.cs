using System;
using System.IO;
using System.Text;
using Platform.Data.Core.Triplets;

namespace Platform.Sandbox
{
    public class TextParser : IDisposable
    {
        const int DefaultBufferSize = 1024 * 80;

        protected class State
        {
            public Link ResultSequence;
            public string ParsedFromPath;
        }

        private readonly StreamReader _streamReader;
        private readonly State _state;

        public TextParser()
        {
            _state = new State();
        }

        public TextParser(Stream stream, int bufferSize = DefaultBufferSize)
            : this()
        {
            _streamReader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: bufferSize);
        }

        public TextParser(string path, int bufferSize = DefaultBufferSize)
            : this(File.OpenRead(path), bufferSize)
        {
            _state.ParsedFromPath = path;
        }

        //public Link Parse()
        //{
        //    Link text = Link.Create(Link.Itself, Net.IsA, Net.Text);
        //    if (_state.ResultSequence != null)
        //    {


        //    }
        //    else
        //    {

        //    }
        //}

        public void Dispose()
        {
            _streamReader.Dispose();
        }
    }
}
