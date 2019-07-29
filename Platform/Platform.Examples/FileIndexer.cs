using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Platform.Threading;
using Platform.IO;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Sequences;
using Platform.Data;

namespace Platform.Examples
{
    public class FileIndexer
    {
        private readonly SequencesIndexer<ulong> _indexer;
        private readonly SynchronizedLinks<ulong> _links;

        public FileIndexer(SynchronizedLinks<ulong> links, SequencesIndexer<ulong> indexer)
        {
            _links = links;
            _indexer = indexer;
        }

        public void IndexSync(string path, CancellationToken cancellationToken)
        {
            const int stepSize = 1024 * 1024;

            using (var reader = File.OpenText(path))
            {
                var steps = 0;
                char[] buffer = new char[stepSize];
                int readChars = 0;
                char lastCharOfPreviousChunk = '\0';
                // TODO: Try use IDirectMemory + Partitioner
                while (!cancellationToken.IsCancellationRequested && (readChars = reader.Read(buffer, 0, stepSize)) > 0) // localSteps * stepSize
                {
                    if (lastCharOfPreviousChunk != '\0')
                    {
                        _links.GetOrCreate(UnicodeMap.FromCharToLink(lastCharOfPreviousChunk),
                            UnicodeMap.FromCharToLink(buffer[0]));
                    }
                    lastCharOfPreviousChunk = buffer[readChars - 1];
                    var linkArray = UnicodeMap.FromCharsToLinkArray(buffer, readChars);
                    _indexer.BulkIndexUnsync(linkArray);
                    Console.WriteLine($"chars: {(ulong)steps * stepSize + (ulong)readChars}, links: {_links.Count() - UnicodeMap.MapSize}");
                    steps++;
                }
            }
        }

        public async Task IndexAsync(string path, CancellationToken cancellationToken)
        {
            const int stepSize = 1024 * 1024;
            using (var reader = File.OpenText(path))
            {
                var steps = 0;
                char[] buffer = new char[stepSize];
                int readChars = 0;
                char lastCharOfPreviousChunk = '\0';
                ConcurrentQueue<Task> tasks = new ConcurrentQueue<Task>();
                // TODO: Try use IDirectMemory + Partitioner
                while (!cancellationToken.IsCancellationRequested && (readChars = reader.Read(buffer, 0, stepSize)) > 0) // localSteps * stepSize
                {
                    if (lastCharOfPreviousChunk != '\0')
                    {
                        _links.GetOrCreate(UnicodeMap.FromCharToLink(lastCharOfPreviousChunk),
                            UnicodeMap.FromCharToLink(buffer[0]));
                    }
                    lastCharOfPreviousChunk = buffer[readChars - 1];
                    var bufferCopy = buffer;
                    var readCharsCopy = readChars;
                    buffer = new char[stepSize];
                    tasks.EnqueueAsRunnedTask(() =>
                    {
                        var linkArray = UnicodeMap.FromCharsToLinkArray(bufferCopy, readCharsCopy);
                        _indexer.BulkIndex(linkArray);
                    });
                    if (tasks.Count > 3)
                    {
                        await tasks.AwaitOne();
                    }
                    Console.WriteLine($"chars: {(ulong)steps * stepSize + (ulong)readChars}, links: {_links.Count() - UnicodeMap.MapSize}");
                    steps++;
                }
                await tasks.AwaitAll();
            }
        }

        public void IndexParallel(string path, CancellationToken cancellationToken)
        {
            const int printStepSize = 1024;
            var partitioner = Partitioner.Create(File.ReadLines(path), EnumerablePartitionerOptions.NoBuffering);
            var totalSize = FileHelpers.GetSize(path);
            var totalChars = 0L;
            var linesToPrint = 0L;
            // TODO: Looks like it should safely wait for each operation to finish on cancel
            Parallel.ForEach(partitioner, (line, state) =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    state.Stop();
                    return;
                }
                if (string.IsNullOrWhiteSpace(line))
                {
                    return;
                }
                // NewLine -> First Character
                _links.GetOrCreate(UnicodeMap.FromCharToLink('\n'), UnicodeMap.FromCharToLink(line[0]));
                var linkArray = UnicodeMap.FromStringToLinkArray(line);
                _indexer.BulkIndex(linkArray);
                // Last Character -> NewLine
                _links.GetOrCreate(UnicodeMap.FromCharToLink(line[line.Length - 1]), UnicodeMap.FromCharToLink('\n'));
                var totalLinks = _links.Count() - UnicodeMap.MapSize;
                Interlocked.Add(ref totalChars, line.Length);
                Interlocked.Increment(ref linesToPrint);
                if (totalChars % printStepSize == 0)
                {
                    Console.WriteLine($"Parsed {totalChars}/{totalSize} chars, links: {totalLinks}");
                }
            });
        }
    }
}
