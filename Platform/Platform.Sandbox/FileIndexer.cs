using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Platform.Data.Core.Pairs;
using Platform.Data.Core.Sequences;

namespace Platform.Sandbox
{
    public class FileIndexer
    {
        private readonly Sequences _sequences;
        private readonly Links _links;

        public FileIndexer(Links links, Sequences sequences)
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

                while (!cancellationToken.IsCancellationRequested && (readChars = reader.Read(buffer, 0, stepSize)) > 0) // localSteps * stepSize
                {
                    if (lastCharOfPreviousChunk != '\0')
                        _links.Create(UnicodeMap.FromCharToLink(lastCharOfPreviousChunk),
                            UnicodeMap.FromCharToLink(buffer[0]));

                    lastCharOfPreviousChunk = buffer[readChars - 1];

                    var bufferCopy = buffer;
                    var readCharsCopy = readChars;
                    buffer = new char[stepSize];

                    tasks.Enqueue(Task.Run(() =>
                    {
                        var linkArray = UnicodeMap.FromCharsToLinkArray(bufferCopy, readCharsCopy);
                        _sequences.BulkIndex(linkArray);
                    }));

                    Task task;
                    while (tasks.Count > 3 && tasks.TryDequeue(out task))
                        await task;

                    Console.WriteLine("chars: {0}, links: {1}", steps * stepSize + readChars, _links.Count() - UnicodeMap.MapSize);

                    steps++;
                }

                {
                    Task task;
                    while (tasks.TryDequeue(out task))
                        await task;
                }
            }
        }
    }
}
