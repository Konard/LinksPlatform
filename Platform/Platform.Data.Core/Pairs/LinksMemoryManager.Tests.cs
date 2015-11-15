using System;
using System.Globalization;
using System.Xml;

namespace Platform.Data.Core.Pairs
{
    unsafe partial class LinksMemoryManager
    {
        public void ExportSourcesTree(string outputFilename)
        {
                using (var writer = XmlWriter.Create(outputFilename))
                {
                    // <?xml version="1.0" encoding="UTF-8"?>
                    writer.WriteStartDocument();

                    // <gexf xmlns="http://www.gexf.net/1.2draft" version="1.2">
                    writer.WriteStartElement("gexf", "http://www.gexf.net/1.2draft");
                    writer.WriteAttributeString("version", "1.2");

                    // <graph mode="static" defaultedgetype="directed">
                    writer.WriteStartElement("graph");
                    writer.WriteAttributeString("mode", "static");
                    writer.WriteAttributeString("defaultedgetype", "directed");

                    // <nodes>
                    writer.WriteStartElement("nodes");
                    for (ulong link = 1; link <= _header->AllocatedLinks; link++)
                    {
                        if (Exists(link))
                        {
                            // <node id="0" label="0" />
                            writer.WriteStartElement("node");
                            writer.WriteAttributeString("id", link.ToString(CultureInfo.InvariantCulture));
                            writer.WriteAttributeString("label", FormatLink(link));
                            writer.WriteEndElement();
                        }
                    }
                    // </nodes>
                    writer.WriteEndElement();

                    ulong edges = 0;

                    // <edges>
                    writer.WriteStartElement("edges");
                    for (ulong link = 1; link <= _header->AllocatedLinks; link++)
                    {
                        if (Exists(link))
                        {
                            var linkIndex = link.ToString(CultureInfo.InvariantCulture);

                            if (_links[link].LeftAsSource != 0)
                            {
                                // <edge id="0" source="0" target="1" />
                                writer.WriteStartElement("edge");
                                writer.WriteAttributeString("id", edges.ToString(CultureInfo.InvariantCulture));
                                writer.WriteAttributeString("source", linkIndex);
                                writer.WriteAttributeString("target",
                                    _links[link].LeftAsSource.ToString(CultureInfo.InvariantCulture));
                                writer.WriteEndElement();

                                edges++;
                            }

                            if (_links[link].RightAsSource != 0)
                            {
                                // <edge id="0" source="0" target="1" />
                                writer.WriteStartElement("edge");
                                writer.WriteAttributeString("id", edges.ToString(CultureInfo.InvariantCulture));
                                writer.WriteAttributeString("source", linkIndex);
                                writer.WriteAttributeString("target",
                                    _links[link].RightAsSource.ToString(CultureInfo.InvariantCulture));
                                writer.WriteEndElement();

                                edges++;
                            }
                        }
                    }
                    // </edges>
                    writer.WriteEndElement();

                    // </graph>
                    writer.WriteEndElement();

                    // </gexf>
                    writer.WriteEndElement();

                    writer.WriteEndDocument();

                    Console.WriteLine("Head of Sources: {0}", _header->FirstAsSource);
                }
        }

        public void ExportTargetsTree()
        {
            throw new NotImplementedException();
        }

        public void Export(string outputFilename)
        {
                using (var writer = XmlWriter.Create(outputFilename))
                {
                    // <?xml version="1.0" encoding="UTF-8"?>
                    writer.WriteStartDocument();

                    // <gexf xmlns="http://www.gexf.net/1.2draft" version="1.2">
                    writer.WriteStartElement("gexf", "http://www.gexf.net/1.2draft");
                    writer.WriteAttributeString("version", "1.2");

                    // <graph mode="static" defaultedgetype="directed">
                    writer.WriteStartElement("graph");
                    writer.WriteAttributeString("mode", "static");
                    writer.WriteAttributeString("defaultedgetype", "directed");

                    // <nodes>
                    writer.WriteStartElement("nodes");
                    for (ulong link = 1; link <= _header->AllocatedLinks; link++)
                    {
                        if (Exists(link))
                        {
                            // <node id="0" label="0" />
                            writer.WriteStartElement("node");
                            writer.WriteAttributeString("id", link.ToString(CultureInfo.InvariantCulture));
                            writer.WriteAttributeString("label", FormatLink(link));
                            writer.WriteEndElement();
                        }
                    }
                    // </nodes>
                    writer.WriteEndElement();

                    ulong edges = 0;

                    // <edges>
                    writer.WriteStartElement("edges");
                    for (ulong link = 1; link <= _header->AllocatedLinks; link++)
                    {
                        if (Exists(link))
                        {
                            // <edge id="0" source="0" target="1" />
                            writer.WriteStartElement("edge");
                            writer.WriteAttributeString("id", edges.ToString(CultureInfo.InvariantCulture));
                            writer.WriteAttributeString("source",
                                _links[link].Source.ToString(CultureInfo.InvariantCulture));
                            writer.WriteAttributeString("target",
                                _links[link].Target.ToString(CultureInfo.InvariantCulture));
                            writer.WriteEndElement();

                            edges++;
                        }
                    }
                    // </edges>
                    writer.WriteEndElement();

                    // </graph>
                    writer.WriteEndElement();

                    // </gexf>
                    writer.WriteEndElement();

                    writer.WriteEndDocument();

                    Console.WriteLine("Head of Sources: {0}", _header->FirstAsSource);
                }
        }

        public string FormatLink(ulong link)
        {
            if (!Exists(link))
                    throw new Exception(string.Format("Source link {0} is not exists.", link));

                //return string.Format("{0}: {1} -> {2}", link, this.links[link].Source, this.links[link].Target);

                //if (_links[link].Target == 0)
                //    return string.Format("0 {0} 0", link);
                //else

                return string.Format("{1} {0} {2}", link, _links[link].Source, _links[link].Target);
        }
    }
}