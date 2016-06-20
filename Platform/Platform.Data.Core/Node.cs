using System;
using System.Collections.Generic;

// ReSharper disable ForCanBeConvertedToForeach

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
    public class Node
    {
        private Dictionary<object, Node> _childNodes;

        public Node(Node parent, object key, object value, bool createNullChildren)
        {
            Parent = parent;
            Key = key;
            Value = value;
            CreateNullChildren = createNullChildren;
        }

        public Node(object key, object value, bool createNullChildren)
            : this(null, key, value, createNullChildren)
        {
        }

        public Node(object key, object value)
            : this(null, key, value, false)
        {
        }

        public Node(object key)
            : this(null, key, null, false)
        {
        }

        public object Key { get; set; }
        public object Value { get; set; }
        public bool CreateNullChildren { get; }
        public Node Parent { get; private set; }

        public Dictionary<object, Node> ChildNodes => _childNodes ?? (_childNodes = new Dictionary<object, Node>());

        public Node this[object key]
        {
            get
            {
                var child = GetChild(key);
                if (child == null && CreateNullChildren)
                    child = AddChild(key);
                return child;
            }
            set
            {
                SetChildValue(value, key);
            }
        }

        public Node AddChild(object key) => AddChild(key, null);

        public Node AddChild(object key, object value)
        {
            //ValidateKeyAlreadyExists(key);

            var child = new Node(this, key, value, CreateNullChildren);
            return AddChild(child);
        }

        private void ValidateKeyAlreadyExists(object key)
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

        public Node GetChild(params object[] keys)
        {
            var node = this;
            for (var i = 0; i < keys.Length; i++)
            {
                node.ChildNodes.TryGetValue(keys[i], out node);
                if (node == null)
                    return null;
            }

            return node;
        }

        public object GetChildValue(params object[] keys)
        {
            var childNode = GetChild(keys);

            return childNode?.Value;
        }

        public bool ContainsChild(params object[] keys) => GetChild(keys) != null;

        public Node SetChild(params object[] keys) => SetChildValue(null, keys);

        public Node SetChild(object key) => SetChildValue(null, key);

        public Node SetChildValue(object value, params object[] keys)
        {
            var node = this;
            for (var i = 0; i < keys.Length; i++)
            {
                Node child;
                if (!node.ChildNodes.TryGetValue(keys[i], out child))
                    child = node.AddChild(keys[i], value);
                node = child;
            }
            node.Value = value;

            return node;
        }

        public Node SetChildValue(object value, object key)
        {
            Node child;
            if (!ChildNodes.TryGetValue(key, out child))
                child = AddChild(key, value);
            child.Value = value;

            return child;
        }
    }
}