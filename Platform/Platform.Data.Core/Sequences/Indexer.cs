﻿using Platform.Helpers;
using Platform.Data.Core.Doublets;

namespace Platform.Data.Core.Sequences
{
    public class SequencesIndexer<TLink>
    {
        private readonly ISynchronizedLinks<TLink> _links;
        private readonly TLink _null;

        public SequencesIndexer(ISynchronizedLinks<TLink> links)
        {
            _links = links;
            _null = _links.Constants.Null;
        }

        /// <summary>
        /// Индексирует последовательность глобально, и возвращает значение,
        /// определяющие была ли запрошенная последовательность проиндексирована ранее. 
        /// </summary>
        /// <param name="sequence">Последовательность для индексации.</param>
        /// <returns>
        /// True если последовательность уже была проиндексирована ранее и
        /// False если последовательность была проиндексирована только что.
        /// </returns>
        public bool Index(TLink[] sequence)
        {
            var indexed = true;

            var i = sequence.Length;
            while (--i >= 1 && (indexed = !MathHelpers<TLink>.IsEquals(_links.SearchOrDefault(sequence[i - 1], sequence[i]), _null))) { }

            for (; i >= 1; i--)
                _links.GetOrCreate(sequence[i - 1], sequence[i]);

            return indexed;
        }

        public bool BulkIndex(TLink[] sequence)
        {
            var indexed = true;

            var i = sequence.Length;

            var links = _links.Unsync;

            _links.SyncRoot.ExecuteReadOperation(() =>
            {
                while (--i >= 1 && (indexed = !MathHelpers<TLink>.IsEquals(links.SearchOrDefault(sequence[i - 1], sequence[i]), _null))) { }
            });

            if (indexed == false)
            {
                _links.SyncRoot.ExecuteWriteOperation(() =>
                {
                    for (; i >= 1; i--)
                        links.GetOrCreate(sequence[i - 1], sequence[i]);
                });
            }

            return indexed;
        }

        public bool BulkIndexUnsync(TLink[] sequence)
        {
            var indexed = true;

            var i = sequence.Length;

            var links = _links.Unsync;

            while (--i >= 1 && (indexed = !MathHelpers<TLink>.IsEquals(links.SearchOrDefault(sequence[i - 1], sequence[i]), _null))) { }

            for (; i >= 1; i--)
                links.GetOrCreate(sequence[i - 1], sequence[i]);

            return indexed;
        }

        public bool CheckIndex(TLink[] sequence)
        {
            var indexed = true;

            var i = sequence.Length;
            while (--i >= 1 && (indexed = !MathHelpers<TLink>.IsEquals(_links.SearchOrDefault(sequence[i - 1], sequence[i]), _null))) { }

            return indexed;
        }
    }
}
