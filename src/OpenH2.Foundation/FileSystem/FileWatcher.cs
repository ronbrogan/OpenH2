using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Text;
using System.Threading;

namespace OpenH2.Foundation.FileSystem
{
    public sealed class FileWatcher : IDisposable
    {
        private string originalPath;
        private FileSystemWatcher watcher;
        private List<FileWatcherCallback> callbacks;
        private SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public FileWatcher(string path)
        {
            callbacks = new List<FileWatcherCallback>();
            originalPath = path;
            watcher = new FileSystemWatcher(Path.GetDirectoryName(path))
            {
                Filter = Path.GetFileName(path)
            };

            //watcher.Changed += this.W_Changed;
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(h => watcher.Changed += h, h => watcher.Changed -= h)
                .Throttle(TimeSpan.FromMilliseconds(300))
                .Subscribe(a => W_Changed(a.Sender, a.EventArgs));

            watcher.EnableRaisingEvents = true;

        }

        public IDisposable AddListener(Action<string> action)
        {
            return new FileWatcherCallback(action, callbacks);
        }

        public void Dispose()
        {
            this.watcher?.Dispose();

            foreach (var cb in callbacks)
                cb.Dispose();
        }

        private void W_Changed(object sender, FileSystemEventArgs e)
        {
            foreach (var cb in callbacks)
                cb.Invoke(originalPath);
        }

        private class FileWatcherCallback : IDisposable
        {
            private readonly Action<string> action;
            private readonly List<FileWatcherCallback> cbs;

            public FileWatcherCallback(Action<string> action, List<FileWatcherCallback> cbs)
            {
                this.action = action;
                this.cbs = cbs;

                cbs.Add(this);
            }

            public void Invoke(string path)
            {
                this.action(path);
            }

            public void Dispose()
            {
                try
                {
                    cbs.Remove(this);
                }
                catch (Exception) { }
            }
        }
    }
}
