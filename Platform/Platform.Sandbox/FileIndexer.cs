using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Platform.Data.Core.Pairs;
using Platform.Data.Core.Sequences;
using Platform.Helpers.Collections;

namespace Platform.Sandbox
{
    public class FileIndexer
    {
        private readonly Sequences _sequences;
        private readonly SynchronizedLinks<ulong> _links;

        public FileIndexer(SynchronizedLinks<ulong> links, Sequences sequences)
        {
            _links = links;
            _sequences = sequences;
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
                        _links.CreateAndUpdate(UnicodeMap.FromCharToLink(lastCharOfPreviousChunk),
                            UnicodeMap.FromCharToLink(buffer[0]));

                    lastCharOfPreviousChunk = buffer[readChars - 1];

                    var bufferCopy = buffer;
                    var readCharsCopy = readChars;
                    buffer = new char[stepSize];

                    tasks.EnqueueTask(() =>
                    {
                        var linkArray = UnicodeMap.FromCharsToLinkArray(bufferCopy, readCharsCopy);
                        _sequences.BulkIndex(linkArray);
                    });

                    if (tasks.Count > 3) await tasks.AwaitAll();

                    Console.WriteLine($"chars: {(ulong) steps*stepSize + (ulong) readChars}, links: {_links.Count() - UnicodeMap.MapSize}");

                    steps++;
                }

                await tasks.AwaitAll();
            }
        }

        public async Task IndexParallelAsync(string path, CancellationToken cancellationToken)
        {
            var partitioner = Partitioner.Create(File.ReadLines(path), EnumerablePartitionerOptions.NoBuffering);

            // TODO: Looks like it should safely wait for each operation to finish on cancel
            Parallel.ForEach(partitioner, (line, state) =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    state.Stop();
                    return;
                }

                if (string.IsNullOrWhiteSpace(line))
                    return;

                // NewLine -> First Character
                _links.CreateAndUpdate(UnicodeMap.FromCharToLink('\n'), UnicodeMap.FromCharToLink(line[0]));

                var linkArray = UnicodeMap.FromStringToLinkArray(line);
                _sequences.BulkIndex(linkArray);

                // Last Character -> NewLine
                _links.CreateAndUpdate(UnicodeMap.FromCharToLink(line[line.Length - 1]), UnicodeMap.FromCharToLink('\n'));

                Console.WriteLine($"parsed line of {line.Length} chars, links: {_links.Count() - UnicodeMap.MapSize}");
            });
        }
    }
}
