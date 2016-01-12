using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Platform.Data.Core.Pairs;

namespace Platform.Sandbox
{
    class WikipediaImporter
    {
        private readonly IWikipediaStorage<ulong> _storage;

        public WikipediaImporter(IWikipediaStorage<ulong> storage)
        {
            _storage = storage;
        }

        public Task Import(string file, CancellationToken token)
        {
            return Task.Factory.StartNew(() =>
            {
                //const int linesLength = 500;
                //var lines = new string[linesLength];
                //using (var rawReader = new StreamReader(file.Trim('"')))
                //{
                //    for (var i = 0; i < 500; i++)
                //        lines[i] = rawReader.ReadLine();
                //}

                using (var reader = XmlReader.Create(file.Trim('"')))
                    ReadElement(reader, token);
            }, token);
        }

        private void ReadElement(XmlReader reader, CancellationToken token, ulong parent = LinksConstants.Null)
        {
            // TODO: Add element path to storage (http://stackoverflow.com/questions/31338885/getting-xpath-for-node-with-xmlreader)
            // TODO: Namespaces/Namespace[1]/Text, Namespaces/Namespace[2]/Text (keep track of different elements with same name).
            // TODO: Write to console current path.
            // TODO: If path was loaded previously, skip it.

            if (token.IsCancellationRequested)
                return;

            if (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (token.IsCancellationRequested)
                            return;

                        var elementName = reader.Name;

                        Console.WriteLine("Starting element {0}...", elementName);

                        var element = _storage.CreateElement(name: elementName);
                        if (parent != LinksConstants.Null)
                            _storage.AttachElementToParent(elementToAttach: element, parent: parent);

                        do
                        {
                            ReadElement(reader, token, element);

                            if (token.IsCancellationRequested)
                                return;

                        } while (reader.Name != elementName);

                        Console.WriteLine("Element {0} finished.", elementName);
                        break;

                    case XmlNodeType.Text:
                        if (token.IsCancellationRequested)
                            return;

                        Console.WriteLine("Starting text element...");

                        var textElement = _storage.CreateTextElement(content: reader.Value);

                        _storage.AttachElementToParent(textElement, parent);

                        Console.WriteLine("Text element finished.");

                        break;

                    case XmlNodeType.EndElement:

                        Console.WriteLine("Current depth: {0}.", reader.Depth);

                        return;
                }
            }
        }
    }
}
