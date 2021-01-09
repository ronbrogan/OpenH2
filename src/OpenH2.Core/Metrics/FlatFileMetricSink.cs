using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace OpenH2.Core.Metrics
{
    public class FlatFileMetricSink : IMetricSink
    {
        private ConcurrentQueue<string> messages = new();
        private Thread? worker = null;
        private FileStream outFile;
        private TextWriter outWriter;

        public FlatFileMetricSink(string path)
        {
            this.outFile = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            this.outWriter = new StreamWriter(this.outFile);
        }

        public void Start()
        {
            worker = new Thread(new ThreadStart(Work));
            worker.Start();
        }

        public void Stop()
        {
            if(worker != null)
            {
                worker.Abort();
                worker.Join();
            }
        }

        public void Write(string sourceIdentifier, string plainText)
        {
            messages.Enqueue($"[{sourceIdentifier}] {plainText}");
        }

        private void Work()
        {
            while(true)
            {
                var batchSize = 100;
                while(batchSize > 0 && messages.TryDequeue(out var msg))
                {
                    this.outWriter.WriteLine(msg);

                    batchSize--;
                }

                Thread.Sleep(500);
            }
        }
    }
}
