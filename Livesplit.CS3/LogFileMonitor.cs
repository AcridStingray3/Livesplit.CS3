using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Livesplit.CS3
{
    
/*
 * PollingLogFileMonitor.cs: Aggressively poll a log file to read lines from it
 * in approximately real time.
 *
 * Written in 2020 by xorhash.
 *
 * To the extent possible under law,
 * the author(s) have dedicated all copyright and related and
 * neighboring rights to this software to the public domain worldwide.
 * This software is distributed without any warranty. 
 *
 * You should have received a copy of the CC0 Public Domain Dedication
 * along with this software.
 * If not, see <https://creativecommons.org/publicdomain/zero/1.0/>.
 */
    public sealed class LogFileMonitor : IDisposable
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
                while (!_wantStop)
                {
                    string line = _reader.ReadLine();
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
                            
                            Thread.Sleep(1);
                            /*
                             * Sleep 1ms in hopes the file will grow.
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
            _wantStop = true;
        }

        #region IDisposable Support
        private bool disposedValue; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!disposedValue) {
                if (disposing) {
                    _reader?.Dispose();
                    _stream?.Dispose();
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