using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Linq;
using Platform.Exceptions;

namespace Platform.Examples
{
    public class XmlElementCounter
    {
        public XmlElementCounter() { }

        public Task Count(string file, CancellationToken token)
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
                    var context = new RootElementContext();
                    using (var reader = XmlReader.Create(file))
                    {
                        Read(reader, token, context);
                    }
                    Console.WriteLine($"Total pages: {context.TotalPages}, total content length: {context.TotalContentsLength}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToStringWithAllInnerExceptions());
                }
            }, token);
        }

        private void Read(XmlReader reader, CancellationToken token, ElementContext context)
        {
            var rootContext = (RootElementContext)context;
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
                            //ConsoleHelpers.Debug("{0} starting...",
                            //    elements.Count <= 20 ? ToXPath(elements) : elementName); // XPath
                            parentContexts.Push(context);
                            context = new ElementContext();
                        }
                        else
                        {
                            //ConsoleHelpers.Debug("{0} finished.", elementName);
                        }
                        break;

                    case XmlNodeType.EndElement:
                        //ConsoleHelpers.Debug("{0} finished.",
                        //    elements.Count <= 20 ? ToXPath(elements) : elements.Peek()); // XPath
                        var topElement = elements.Pop();
                        // Restoring scope
                        context = parentContexts.Pop();
                        if (topElement.StartsWith("page"))
                        {
                            rootContext.TotalPages++;
                            //if (rootContext.TotalPages > 10163)
                            //    selfCancel = true;

                            // TODO: Check for 0x00 part/symbol at 198102797 line and 13 position.
                            //if (rootContext.TotalPages > 3490000)
                            //    selfCancel = true;
                            if (context.ChildrenNamesCounts["page"] % 10000 == 0)
                            {
                                Console.WriteLine(topElement);
                            }
                        }
                        break;

                    case XmlNodeType.Text:
                        //ConsoleHelpers.Debug("Starting text element...");
                        var content = reader.Value;
                        rootContext.TotalContentsLength += (ulong)content.Length;
                        //ConsoleHelpers.Debug($"Content length is: {content.Length}");
                        //ConsoleHelpers.Debug("Text element finished.");
                        break;
                }
            }
        }

        private string ToXPath(Stack<string> path) => string.Join("/", path.Reverse());

        private class ElementContext
        {
            public readonly Dictionary<string, int> ChildrenNamesCounts;

            public ElementContext()
            {
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

        private class RootElementContext : ElementContext
        {
            public ulong TotalPages;
            public ulong TotalContentsLength;
        }
    }
}
