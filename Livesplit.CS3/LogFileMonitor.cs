using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Livesplit.CS3
{
    public class LogFileMonitor : IDisposable
    {
        private readonly FileStream _stream;
        private readonly StreamReader _reader;
        private bool _wantStop;

        public delegate void NewLineHandler(string line);
        public NewLineHandler Handlers;

        public LogFileMonitor(string path)
        {
            try {
                _stream = new FileStream(path, FileMode.Open, FileAccess.Read,
                    FileShare.ReadWrite /* allow other processes to write */);
                _stream.Seek(0, SeekOrigin.End);
                _reader = new StreamReader(_stream, Encoding.UTF8);
                _wantStop = false;
            } catch {
                Dispose();
                throw;
            }
        }

        public void Start()
        {
            new Thread(() => {
                string line;

                while (!_wantStop) {
                    line = _reader.ReadLine();
                    if (line == null) {
                        /*
                         * Check if truncation occurred and rebase the reader
                         * if so.
                         * *Somehow* this doesn't cause CPU spikes even though
                         * this should be either super slow or fail to work?
                         */
                        if (_stream.Length < _stream.Position) {
                            _stream.Seek(0, SeekOrigin.End);
                            _reader.DiscardBufferedData();
                        } else {
                            /*
                             * Sleep 10ms in hopes the file will grow.
                             * You may want to adjust this threshold to
                             * something even lower, depending on how little of
                             * an error margin you can allow,
                             * but CPU usage may spike if you do.
                             * 
                             * Ideally, you'd have Durante add the game's idea
                             * of the current time to each log entry so that
                             * this doesn't really matter.
                             */
                           
                        }
                    } else {
                        Handlers?.Invoke(line);
                    }
                }
            }).Start();
        }

        public void Stop()
        {
            this._wantStop = true;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue) {
                if (disposing) {
                    if (_reader != null)
                        _reader.Dispose();
                    if (_stream != null)
                        _stream.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}