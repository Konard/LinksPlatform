using System.Collections.Generic;

// ReSharper disable ForCanBeConvertedToForeach

namespace Platform.Helpers
{
    public class Node
    {
        private Dictionary<object, Node> _childNodes;

        public object Value { get; set; }

        public Dictionary<object, Node> ChildNodes => _childNodes ?? (_childNodes = new Dictionary<object, Node>());

        public Node this[object key]
        {
            get
            {
                var child = GetChild(key);
                if (child == null)
                    child = AddChild(key);
                return child;
            }
            set => SetChildValue(value, key);
        }

        public Node(object value) => Value = value;

        public Node()
            : this(null)
        {
        }

        public bool ContainsChild(params object[] keys) => GetChild(keys) != null;

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

        public object GetChildValue(params object[] keys) => GetChild(keys)?.Value;

        public Node AddChild(object key) => AddChild(key, new Node(null));

        public Node AddChild(object key, object value) => AddChild(key, new Node(value));

        public Node AddChild(object key, Node child)
        {
            ChildNodes.Add(key, child);
            return child;
        }

        public Node SetChild(params object[] keys) => SetChildValue(null, keys);

        public Node SetChild(object key) => SetChildValue(null, key);

        public Node SetChildValue(object value, params object[] keys)
        {
            var node = this;
            for (var i = 0; i < keys.Length; i++)
                node = SetChildValue(keys[i], value);
            node.Value = value;
            return node;
        }

        public Node SetChildValue(object key, object value)
        {
            if (!ChildNodes.TryGetValue(key, out Node child))
                child = AddChild(key, value);
            child.Value = value;
            return child;
        }
    }
}