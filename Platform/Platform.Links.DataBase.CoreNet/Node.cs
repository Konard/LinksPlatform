using System;
using System.Collections.Generic;

namespace Platform.Links.DataBase.CoreNet
{
    // Concept:
    // var value = Node[43][12]["32423"].Value;
    // Node[12][42]["324234"].Value = 12;
    //
    // Новая идея
    // "324234" -> [51][50][52][50][51][52]
    //
    //49 = '1'
    //50 = '2'
    //51 = '3'
    //52 = '4'

    // Узнать почему нужен IComparable
    public class Node
    {
        private Dictionary<IComparable, Node> m_ChildNodes;

        public IComparable Key { get; set; }
        public object Value { get; set; }
        public bool CreateNullChildren { get; private set; }
        public Node Parent { get; private set; }

        public Dictionary<IComparable, Node> ChildNodes
        {
            get
            {
                if (m_ChildNodes == null)
                {
                    m_ChildNodes = new Dictionary<IComparable, Node>();
                }
                return m_ChildNodes;
            }
            private set
            {
                m_ChildNodes = value;
            }
        }

        public Node this[IComparable key]
        {
            get
            {
                Node child = this.GetChild(key);
                if (child == null && this.CreateNullChildren)
                {
                    child = this.AddChild(key);
                }
                return child;
            }
            set
            {
                this.SetChild(key);
            }
        }

        public Node(Node parent, IComparable key, object value, bool createNullChildren)
        {
            this.Parent = parent;
            this.Key = key;
            this.Value = value;
            this.CreateNullChildren = createNullChildren;
        }

        public Node(IComparable key, object value, bool createNullChildren)
            : this(null, key, value, createNullChildren)
        {
        }

        public Node(IComparable key, object value)
            : this(null, key, value, false)
        {
        }

        public Node(IComparable key)
            : this(null, key, null, false)
        {
        }

        public Node AddChild(IComparable key)
        {
            return AddChild(key, null);
        }

        public Node AddChild(IComparable key, object value)
        {
            //ValidateKeyAlreadyExists(key);

            Node child = new Node(this, key, value, this.CreateNullChildren);
            return AddChild(child);
        }

        private void ValidateKeyAlreadyExists(IComparable key)
        {
            if (this.ChildNodes.ContainsKey(key))
            {
                throw new InvalidOperationException("Child collection already contains node with the same key.");
            }
        }

        public Node AddChild(Node child)
        {
            //ValidateKeyAlreadyExists(child.Key);

            this.ChildNodes.Add(child.Key, child);

            return child;
        }

        public Node GetChild(params IComparable[] keys)
        {
            Node node = this;
            for (int i = 0; i < keys.Length; i++)
            {
                node.ChildNodes.TryGetValue(keys[i], out node);
                if (node == null)
                {
                    return null;
                }
            }

            return node;
        }

        public object GetChildValue(params IComparable[] keys)
        {
            Node childNode = GetChild(keys);

            if (childNode == null)
            {
                return null;
            }

            return childNode.Value;
        }

        public bool ContainsChild(params IComparable[] keys)
        {
            return GetChild(keys) != null;
        }

        public Node SetChild(params IComparable[] keys)
        {
            return SetChildValue(null, keys);
        }

        public Node SetChild(IComparable key)
        {
            return SetChildValue(null, key);
        }

        public Node SetChildValue(object value, params IComparable[] keys)
        {
            Node node = this;
            for (int i = 0; i < keys.Length; i++)
            {
                Node child;
                if (!node.ChildNodes.TryGetValue(keys[i], out child))
                {
                    child = node.AddChild(keys[i], value);
                }
                node = child;
            }
            node.Value = value;

            return node;
        }

        public Node SetChildValue(object value, IComparable key)
        {
            Node child;
            if (!this.ChildNodes.TryGetValue(key, out child))
            {
                child = this.AddChild(key, value);
            }
            child.Value = value;

            return child;
        }
    }
}
