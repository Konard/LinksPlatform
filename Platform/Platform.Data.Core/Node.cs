using System;
using System.Collections.Generic;

namespace Platform.Data.Core
{
    // Concept:
    // var value = Node[43][12]["32423"].Value;
    // Node[12][42]["324234"].Value = 12;
    //
    // Новая идея
    // "324234" -> [51][50][52][50][51][52]
    //
    //48 = '0'
    //49 = '1'
    //50 = '2'
    //51 = '3'
    //52 = '4'
    //
    //
    // Node представляет собой элемент свободного дерева, с неограниченным количеством элементов. 
    // Однако каждая ветка/лист может содержаться в элементе только один раз.
    // Такого рода структура может быть полезна для формирования индексов-группировки со свободной структурой.
    // 
    // Эта структура совместима с Links, и может быть представлена через связи. Но ближе всего к последоватеностям.
    // Наивный подход, а именно связи: (51 50) (50 52) (52 50) (50 51) (51 52) не всегда будет функционировать корректно.
    // ((((51 50) 52) 50) 51) 52) будет точнее.

    // Узнать почему нужен IComparable
    public class Node
    {
        private Dictionary<IComparable, Node> _childNodes;

        public Node(Node parent, IComparable key, object value, bool createNullChildren)
        {
            Parent = parent;
            Key = key;
            Value = value;
            CreateNullChildren = createNullChildren;
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

        public IComparable Key { get; set; }
        public object Value { get; set; }
        public bool CreateNullChildren { get; private set; }
        public Node Parent { get; private set; }

        public Dictionary<IComparable, Node> ChildNodes
        {
            get
            {
                if (_childNodes == null)
                    _childNodes = new Dictionary<IComparable, Node>();
                return _childNodes;
            }
            private set { _childNodes = value; }
        }

        public Node this[IComparable key]
        {
            get
            {
                Node child = GetChild(key);
                if (child == null && CreateNullChildren)
                    child = AddChild(key);
                return child;
            }
            set { SetChild(key); }
        }

        public Node AddChild(IComparable key)
        {
            return AddChild(key, null);
        }

        public Node AddChild(IComparable key, object value)
        {
            //ValidateKeyAlreadyExists(key);

            var child = new Node(this, key, value, CreateNullChildren);
            return AddChild(child);
        }

        private void ValidateKeyAlreadyExists(IComparable key)
        {
            if (ChildNodes.ContainsKey(key))
                throw new InvalidOperationException("Child collection already contains node with the same key.");
        }

        public Node AddChild(Node child)
        {
            //ValidateKeyAlreadyExists(child.Key);

            ChildNodes.Add(child.Key, child);

            return child;
        }

        public Node GetChild(params IComparable[] keys)
        {
            Node node = this;
            for (int i = 0; i < keys.Length; i++)
            {
                node.ChildNodes.TryGetValue(keys[i], out node);
                if (node == null)
                    return null;
            }

            return node;
        }

        public object GetChildValue(params IComparable[] keys)
        {
            Node childNode = GetChild(keys);

            if (childNode == null)
                return null;

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
                    child = node.AddChild(keys[i], value);
                node = child;
            }
            node.Value = value;

            return node;
        }

        public Node SetChildValue(object value, IComparable key)
        {
            Node child;
            if (!ChildNodes.TryGetValue(key, out child))
                child = AddChild(key, value);
            child.Value = value;

            return child;
        }
    }
}