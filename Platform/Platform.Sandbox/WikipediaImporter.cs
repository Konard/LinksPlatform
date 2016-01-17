using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

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
                //using (var rawWriter = new StreamWriter(file.Trim('"')+".500.first"))
                //{
                //    for (var i = 0; i < 500; i++)
                //        rawWriter.WriteLine(lines[i]);
                //}

                var name = file.Trim('"');

                var document = _storage.CreateDocument(name);

                using (var reader = XmlReader.Create(name))
                    Read(reader, token, document);
            }, token);
        }

        private void Read(XmlReader reader, CancellationToken token, ulong parent)
        {
            var parents = new Stack<Tuple<ulong, string, int>>();
            var elements = new Stack<string>();

            string lastElementName = null;
            var elementRepeatCount = 0;

            // TODO: If path was loaded previously, skip it.

            while (reader.Read())
            {
                if (token.IsCancellationRequested)
                    return;

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        var elementName = reader.Name;

                        if (lastElementName != elementName)
                        {
                            lastElementName = elementName;
                            elementRepeatCount = 0;
                        }
                        else
                            elementRepeatCount++;

                        elementName = string.Format("{0}[{1}]", elementName, elementRepeatCount);

                        if (!reader.IsEmptyElement)
                        {
                            elements.Push(elementName);

#if DEBUG
                            Console.WriteLine("{0} starting...",
                                elements.Count <= 20 ? string.Join("/", elements.Reverse()) : elementName);
#endif

                            var element = _storage.CreateElement(name: elementName);

                            parents.Push(new Tuple<ulong, string, int>(parent, lastElementName, elementRepeatCount));
                            _storage.AttachElementToParent(elementToAttach: element, parent: parent);

                            parent = element;
                            lastElementName = null;
                            elementRepeatCount = 0;
                        }
                        else
                        {
#if DEBUG
                            Console.WriteLine("{0} finished.", elementName);
#endif
                        }

                        break;

                    case XmlNodeType.EndElement:

#if DEBUG
                        Console.WriteLine("{0} finished.",
                            elements.Count <= 20 ? string.Join("/", elements.Reverse()) : elements.Peek());
#else
                        var topElement = elements.Peek();
                        if (topElement.StartsWith("page"))
                            Console.WriteLine(topElement);
#endif

                        elements.Pop();

                        // Restoring scope
                        var tuple = parents.Pop();

                        parent = tuple.Item1;
                        lastElementName = tuple.Item2;
                        elementRepeatCount = tuple.Item3;

                        break;

                    case XmlNodeType.Text:
#if DEBUG
                        Console.WriteLine("Starting text element...");
#endif

                        var content = reader.Value;
#if DEBUG
                        Console.WriteLine("Content: {0}{1}", content.Truncate(50), content.Length >= 50 ? "..." : "");
#endif
                        var textElement = _storage.CreateTextElement(content: content);

                        _storage.AttachElementToParent(textElement, parent);
#if DEBUG
                        Console.WriteLine("Text element finished.");
#endif
                        break;
                }
            }
        }
    }
}
