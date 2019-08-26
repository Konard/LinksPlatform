using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Platform.Exceptions;
using Platform.Collections;
using Platform.IO;

namespace Platform.Examples
{
    /// <remarks>
    /// TODO: Can be renamed to XMLImporter
    /// TODO: Add support for XML arguments
    /// </remarks>
    class WikipediaImporter
    {
        private readonly IWikipediaStorage<ulong> _storage;

        public WikipediaImporter(IWikipediaStorage<ulong> storage) => _storage = storage;

        public Task Import(string file, CancellationToken token)
        {
            return Task.Factory.StartNew(() =>
            {
                //const int linesLength = 500;
                //var lines = new string[linesLength];
                //using (var rawReader = new StreamReader(file))
                //{
                //    for (var i = 0; i < 500; i++)
                //        lines[i] = rawReader.ReadLine();
                //}
                //using (var rawWriter = new StreamWriter(file+".500.first"))
                //{
                //    for (var i = 0; i < 500; i++)
                //        rawWriter.WriteLine(lines[i]);
                //}
                try
                {
                    var document = _storage.CreateDocument(file);

                    using (var reader = XmlReader.Create(file))
                    {
                        Read(reader, token, new ElementContext(document));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToStringWithAllInnerExceptions());
                }

            }, token);
        }

        private void Read(XmlReader reader, CancellationToken token, ElementContext context)
        {
            var parentContexts = new Stack<ElementContext>();
            var elements = new Stack<string>(); // Path
            // TODO: If path was loaded previously, skip it.
            while (reader.Read())
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        var elementName = reader.Name;
                        context.IncrementChildNameCount(elementName);
                        elementName = $"{elementName}[{context.ChildrenNamesCounts[elementName]}]";
                        if (!reader.IsEmptyElement)
                        {
                            elements.Push(elementName);
                            ConsoleHelpers.Debug("{0} starting...", elements.Count <= 20 ? ToXPath(elements) : elementName); // XPath
                            var element = _storage.CreateElement(name: elementName);
                            parentContexts.Push(context);
                            _storage.AttachElementToParent(elementToAttach: element, parent: context.Parent);
                            context = new ElementContext(element);
                        }
                        else
                        {
                            ConsoleHelpers.Debug("{0} finished.", elementName);
                        }
                        break;
                    case XmlNodeType.EndElement:
                        ConsoleHelpers.Debug("{0} finished.", elements.Count <= 20 ? ToXPath(elements) : elements.Peek()); // XPath
                        var topElement = elements.Pop();
                        // Restoring scope
                        context = parentContexts.Pop();
                        if (topElement.StartsWith("page"))
                        {
                            if (context.ChildrenNamesCounts["page"] % 1000 == 0)
                                Console.WriteLine(topElement);
                        }
                        break;

                    case XmlNodeType.Text:
                        ConsoleHelpers.Debug("Starting text element...");
                        var content = reader.Value;
                        ConsoleHelpers.Debug("Content: {0}{1}", content.Truncate(50), content.Length >= 50 ? "..." : "");
                        var textElement = _storage.CreateTextElement(content: content);
                        _storage.AttachElementToParent(textElement, context.Parent);
                        ConsoleHelpers.Debug("Text element finished.");
                        break;
                }
            }
        }

        private string ToXPath(Stack<string> path) => string.Join("/", path.Reverse());

        private struct ElementContext
        {
            public readonly ulong Parent;
            public readonly Dictionary<string, int> ChildrenNamesCounts;

            public ElementContext(ulong parent)
            {
                Parent = parent;
                ChildrenNamesCounts = new Dictionary<string, int>();
            }

            public void IncrementChildNameCount(string name)
            {
                if (ChildrenNamesCounts.TryGetValue(name, out int count))
                {
                    ChildrenNamesCounts[name] = count + 1;
                }
                else
                {
                    ChildrenNamesCounts[name] = 0;
                }
            }
        }
    }
}
