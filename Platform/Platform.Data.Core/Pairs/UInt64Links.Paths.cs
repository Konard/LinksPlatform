using System;
using System.Collections;
using Platform.Helpers;

namespace Platform.Data.Core.Pairs
{
    public partial class UInt64Links
    {
        /// <remarks>
        /// TODO: Как так? Как то что ниже может быть корректно?
        /// Скорее всего практически не применимо
        /// Предполагалось, что можно было конвертировать формируемый в проходе через SequenceWalker 
        /// Stack в конкретный путь из Source, Target до связи, но это не всегда так.
        /// TODO: Возможно нужен метод, который именно выбрасывает исключения (EnsurePathExists)
        /// </remarks>
        public bool CheckPathExistance(params ulong[] path)
        {
            var current = path[0];

            //EnsureLinkExists(current, "path");
            if (_memoryManager.Count(current) == 0)
                return false;

            for (var i = 1; i < path.Length; i++)
            {
                var next = path[i];

                var values = _memoryManager.GetLinkValue(current);
                var source = values[Constants.SourcePart];
                var target = values[Constants.TargetPart];

                if (source == target && source == next)
                    //throw new Exception(string.Format("Невозможно выбрать путь, так как и Source и Target совпадают с элементом пути {0}.", next));
                    return false;
                if (next != source && next != target)
                    //throw new Exception(string.Format("Невозможно продолжить путь через элемент пути {0}", next));
                    return false;

                current = next;
            }

            return true;
        }

        /// <remarks>
        /// Может потребовать дополнительного стека для PathElement's при использовании SequenceWalker.
        /// </remarks>
        public ulong GetByKeys(ulong root, params int[] path)
        {
            this.EnsureLinkExists(root, "root");

            var currentLink = root;
            for (var i = 0; i < path.Length; i++)
                currentLink = _memoryManager.GetLinkValue(currentLink)[path[i]];
            return currentLink;
        }

        public ulong GetSquareMatrixSequenceElementByIndex(ulong root, ulong size, ulong index)
        {
            var source = Constants.SourcePart;
            var target = Constants.TargetPart;

            if (!MathHelpers.IsPowerOfTwo(size))
                throw new ArgumentOutOfRangeException(nameof(size), "Sequences with sizes other than powers of two are not supported.");

            var path = new BitArray(BitConverter.GetBytes(index));
            var length = MathHelpers.GetLowestBitPosition(size);

            this.EnsureLinkExists(root, "root");

            var currentLink = root;
            for (var i = length - 1; i >= 0; i--)
                currentLink = _memoryManager.GetLinkValue(currentLink)[path[i] ? target : source];
            return currentLink;
        }
    }
}
