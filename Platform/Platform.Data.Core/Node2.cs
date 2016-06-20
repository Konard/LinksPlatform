using System;
using System.Collections.Generic;
using System.Linq;

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
    public class Node2
    {
        private Dictionary<object, Node2> _childNodes;

        public Node2(Node2 parent, object key, bool createNullChildren)
        {
            Parent = parent;
            Key = key;
            CreateNullChildren = createNullChildren;
        }

        public Node2(object key, bool createNullChildren)
            : this(null, key, createNullChildren)
        {
        }

        public Node2(object key)
            : this(null, key, false)
        {
        }

        public object Key { get; set; }
        public bool CreateNullChildren { get; }
        public Node2 Parent { get; private set; }

        public Dictionary<object, Node2> ChildNodes
        {
            get { return _childNodes ?? (_childNodes = new Dictionary<object, Node2>()); }
            private set { _childNodes = value; }
        }

        public Node2 this[object key]
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

        public Node2 AddChild(object key) => AddChild(key, null);

        public Node2 AddChild(object key, object value)
        {
            //ValidateKeyAlreadyExists(key);

            var child = new Node2(this, key, CreateNullChildren);
            var node = AddChild(child);

            if (value != null)
            {
                var nodeValue = new Node2(node, key, CreateNullChildren);
                AddChild(nodeValue);
            }

            return node;
        }

        private void ValidateKeyAlreadyExists(object key)
        {
            if (ChildNodes.ContainsKey(key))
                throw new InvalidOperationException("Child collection already contains node with the same key.");
        }

        public Node2 AddChild(Node2 child)
        {
            //ValidateKeyAlreadyExists(child.Key);

            ChildNodes.Add(child.Key, child);

            return child;
        }

        public Node2 GetChild(params object[] keys)
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

            if (childNode == null)
                return null;

            if (childNode.ChildNodes.Count == 1)
                return childNode.ChildNodes.Keys.Single();
            else
                return childNode.ChildNodes;
        }

        public bool ContainsChild(params object[] keys) => GetChild(keys) != null;

        public Node2 SetChild(params object[] keys) => SetChildValue(null, keys);

        public Node2 SetChild(object key) => SetChildValue(null, key);

        public Node2 SetChildValue(object value, params object[] keys)
        {
            var node = this;
            for (var i = 0; i < keys.Length; i++)
            {
                Node2 child;
                if (!node.ChildNodes.TryGetValue(keys[i], out child))
                    child = node.AddChild(keys[i], value);
                node = child;
            }

            if (node.ChildNodes == null || node.ChildNodes.Count != 0)
                node.ChildNodes = new Dictionary<object, Node2>();

            node.ChildNodes.Add(value, new Node2(node, value, CreateNullChildren));

            return node;
        }

        public Node2 SetChildValue(object value, object key)
        {
            Node2 child;
            if (!ChildNodes.TryGetValue(key, out child))
                child = AddChild(key, value);

            if (ChildNodes == null || ChildNodes.Count != 0)
                ChildNodes = new Dictionary<object, Node2>();

            ChildNodes.Add(value, new Node2(this, value, CreateNullChildren));

            return child;
        }
    }
}