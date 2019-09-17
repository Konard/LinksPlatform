using System.Collections.Generic;

namespace Platform.Examples
{
    internal class XmlElementContext
    {
        public readonly Dictionary<string, int> ChildrenNamesCounts;
        public int TotalChildren;

        public XmlElementContext() => ChildrenNamesCounts = new Dictionary<string, int>();

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
            TotalChildren++;
        }
    }
}
