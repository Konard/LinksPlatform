using System;
using Platform.Data.Core.Exceptions;

namespace Platform.Data.Core.Pairs
{
    public partial class Links
    {
        public enum PathElement
        {
            Source,
            Target
        }

        /// <remarks>
        /// Скорее всего практически не применимо
        /// Предполагалось, что можно было конвертировать формируемый в проходе через SequenceWalker 
        /// Stack в конкретный путь из Source, Target до связи, но это не всегда так.
        /// </remarks>
        public ulong Get(params ulong[] path)
        {
            return _sync.ExecuteReadOperation(() =>
            {
                for (var i = 0; i < path.Length; i++)
                {
                    var current = path[i];

                    if (!ExistsCore(current))
                        throw new ArgumentLinkDoesNotExistsException<ulong>(current, "path");

                    if ((i + 1) < path.Length)
                    {
                        var next = path[i + 1];
                        var source = GetSourceCore(current);
                        var target = GetTargetCore(current);
                        if (source == target && source == next)
                            throw new Exception(string.Format("Невозможно выбрать путь, так как и Source и Target совпадают с элементом пути {0}.", next));
                        if (next != source && next != target)
                            throw new Exception(string.Format("Невозможно продолжить путь через элемент пути {0}", next));
                    }
                    else
                    {
                        return current;
                    }
                }

                return Null;
            });
        }

        /// <remarks>
        /// Может потребовать дополнительного стека для PathElement's при использовании SequenceWalker.
        /// </remarks>
        public ulong Get(ulong root, params PathElement[] path)
        {
            return _sync.ExecuteReadOperation(() =>
            {
                if (!ExistsCore(root))
                    throw new ArgumentLinkDoesNotExistsException<ulong>(root, "root");

                var currentLink = root;
                for (var i = 0; i < path.Length; i++)
                    currentLink = GetNextCore(currentLink, path[i]);
                return currentLink;
            });
        }

        private ulong GetNextCore(ulong root, PathElement element)
        {
            if (element == PathElement.Source)
                return GetSourceCore(root);
            if (element == PathElement.Target)
                return GetTargetCore(root);

            throw new NotSupportedException();
        }
    }
}
