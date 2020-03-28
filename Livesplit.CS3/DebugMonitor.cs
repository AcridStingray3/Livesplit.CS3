using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Security.AccessControl;
using System.Threading;
using System.Text;


namespace Livesplit.CS3
{
    
// Taken from https://stackoverflow.com/questions/1520119/replicate-functionality-of-debugview-in-net-global-win32-debug-hooks
//Changes include trying to Open the events if they can't be created, and opening in ReadOnly so as to not interrupt the game's prints
//Also a bunch of Rider suggestions and warnings
//Sometimes becomes unable to MapViewOfFile. This seems to be fixed by launching LiveSplit as administrator

    
    public class DebugMonitor : IDisposable
    {
        public delegate void OnOutputDebugStringHandler(int pid, string text);
        public OnOutputDebugStringHandler OnOutputDebugString;

        private readonly MemoryMappedFile _mmf;
        private readonly MemoryMappedViewAccessor _mmfAccessor;
        private readonly EventWaitHandle _bufferReadyEvent;
        private readonly EventWaitHandle _dataReadyEvent;
        private readonly int _monitoredPid;
        private bool _wantStop; // bools are inherently atomic, no lock needed

        public DebugMonitor(int pid)
        {
            try {
                _mmf = MemoryMappedFile.CreateOrOpen("DBWIN_BUFFER", 4096, MemoryMappedFileAccess.Read);
                
                
                _mmfAccessor = _mmf.CreateViewAccessor(0, 4096, MemoryMappedFileAccess.Read);
                try
                {
                    _bufferReadyEvent = new EventWaitHandle(false, EventResetMode.AutoReset, "DBWIN_BUFFER_READY");

                }
                catch
                {
                    _bufferReadyEvent = EventWaitHandle.OpenExisting("DBWIN_BUFFER_READY", EventWaitHandleRights.ReadPermissions | EventWaitHandleRights.Modify );    
                    Debug.WriteLine("Opened buffer");
                }

                try
                {
                    _dataReadyEvent = new EventWaitHandle(false, EventResetMode.AutoReset, "DBWIN_DATA_READY");
                }
                catch
                {
                    _dataReadyEvent = EventWaitHandle.OpenExisting("DBWIN_DATA_READY", EventWaitHandleRights.ReadPermissions);
                    Debug.WriteLine("Opened data");
                }
               
                _monitoredPid = pid;
                _wantStop = false;
            } catch (Exception e) {
                Dispose();
                throw e;
            }
        }

        public void Run()
        {
            new Thread(() => {
                byte[] messageBuffer = new byte[4096 - sizeof(int)];

                _bufferReadyEvent.Set();
                while (true) {
                    
                    if (_wantStop) // check if we want to exit
                        break;

                    _mmfAccessor.Read(0, out int pid);
                    if (pid != _monitoredPid) // uninteresting process
                        continue;
                    _mmfAccessor.ReadArray(sizeof(int), messageBuffer, 0, messageBuffer.Length);
                    Debug.WriteLine(Encoding.ASCII.GetString(messageBuffer));
                    OnOutputDebugString?.Invoke(pid, Encoding.ASCII.GetString(messageBuffer));

                    _bufferReadyEvent.Set();
                }
            }).Start();
        }

        public void Stop()
        {
            _wantStop = true;
            _dataReadyEvent.Set();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue) {
                if (disposing) {
                    Debug.WriteLine("yeet 3");
                    _mmfAccessor?.Dispose();
                    _mmf?.Dispose();
                    _bufferReadyEvent?.Dispose();
                    _dataReadyEvent?.Dispose();
                }

                disposedValue = true;
            }
        }

        // This code was added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}