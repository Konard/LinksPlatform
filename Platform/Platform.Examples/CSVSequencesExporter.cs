using System.IO;
using System.Linq;
using System.Collections.Generic;
using Platform.Collections.Lists;
using Platform.Data.Sequences;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Sequences;

namespace Platform.Examples
{
    public class CSVSequencesExporter : CSVExporter
    {
        private readonly System.Collections.Generic.Stack<KeyValuePair<ulong, List<ulong>>> _stack = new System.Collections.Generic.Stack<KeyValuePair<ulong, List<ulong>>>();

        protected override void WriteLink(StreamWriter writer, IList<ulong> link)
        {
            var linkIndex = link[_links.Constants.IndexPart];

            if (_links.Count(_links.Constants.Any, linkIndex) > 1)
            {
                base.WriteLink(writer, link);
            }
            else
            {
                var elements = new List<ulong>();

                PushLeft(elements, link[_links.Constants.SourcePart]);
                PushRight(elements, link[_links.Constants.TargetPart]);

                CollectLinks(elements, ref linkIndex);

                if (elements.All(x => UnicodeMap.IsCharLink(x) || _visited.Contains(x)))
                {
                    WriteSequence(writer, elements);
                }
                else
                {
                    _stack.Push(new KeyValuePair<ulong, List<ulong>>(linkIndex, elements));
                }
            }

            if (_stack.Count > 0)
            {
                var last = _stack.Peek();
                while (last.Value.All(x => UnicodeMap.IsCharLink(x) || _visited.Contains(x)))
                {
                    last = _stack.Pop();

                    WriteSequence(writer, last.Value);

                    if (_stack.Count == 0)
                    {
                        break;
                    }
                    else
                    {
                        last = _stack.Peek();
                    }
                }
            }
        }

        private void WriteSequence(StreamWriter writer, List<ulong> elements)
        {
            var strings = elements.Select(x => FormatLink(x)).ToList();
            writer.WriteLine(string.Join(", ", strings));
            _linesCounter++;
        }

        private void CollectLinks(IList<ulong> elements, ref ulong linkIndex)
        {
            var linkIndexCopy = linkIndex;

            _links.Each(innerLink =>
            {
                var innerLinkIndex = innerLink[_links.Constants.IndexPart];
                var innerLinkSource = innerLink[_links.Constants.SourcePart];
                var innerLinkTarget = innerLink[_links.Constants.TargetPart];

                if (Visit(innerLinkIndex))
                {
                    if (innerLinkSource == linkIndexCopy)
                    {
                        PushRight(elements, innerLinkTarget);
                    }
                    else // if (innerLinkTarget == linkIndexCopy)
                    {
                        PushLeft(elements, innerLinkSource);
                    }

                    linkIndexCopy = innerLinkIndex;

                    if (_links.Count(_links.Constants.Any, innerLinkIndex) == 1)
                    {
                        CollectLinks(elements, ref linkIndexCopy);
                    }
                }

                return _links.Constants.Break;
            }, _links.Constants.Any, linkIndex);

            linkIndex = linkIndexCopy;
        }

        private void PushRight(IList<ulong> elements, ulong element)
        {
            if (!UnicodeMap.IsCharLink(element) && !_visited.Contains(element) && _links.Count(_links.Constants.Any, element) == 1)
            {
                StopableSequenceWalker.WalkRight(element, _links.GetSource, _links.GetTarget, x => _links.IsPartialPoint(x) || UnicodeMap.IsCharLink(x) || _links.Count(_links.Constants.Any, x) > 1, (x) => Visit(x), x => { }, x => _links.Count(_links.Constants.Any, x) == 1 && !_visited.Contains(x), elements.AddAndReturnTrue);
            }
            else
            {
                elements.Add(element);
            }
        }

        private void PushLeft(IList<ulong> elements, ulong element)
        {
            if (!UnicodeMap.IsCharLink(element) && !_visited.Contains(element) && _links.Count(_links.Constants.Any, element) == 1)
            {
                var temp = new List<ulong>();

                StopableSequenceWalker.WalkRight(element, _links.GetSource, _links.GetTarget, x => _links.IsPartialPoint(x) || UnicodeMap.IsCharLink(x) || _links.Count(_links.Constants.Any, x) > 1, (x) => Visit(x), x => { }, x => _links.Count(_links.Constants.Any, x) == 1 && !_visited.Contains(x), temp.AddAndReturnTrue);

                for (int i = temp.Count - 1; i >= 0; i--)
                {
                    elements.Insert(0, temp[i]);
                }
            }
            else
            {
                elements.Insert(0, element);
            }
        }
    }
}