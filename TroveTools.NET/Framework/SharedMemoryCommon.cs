using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TroveTools.NET.Framework
{
    /// <summary>
    /// Common components used by the SharedMemoryClient and SharedMemoryServer
    /// </summary>
    public static class SharedMemoryCommon
    {
        public const int Capacity = 4096;
        public const int DataLengthPosition = 0;
        public const int DataPosition = DataLengthPosition + sizeof(int);

        public static EventWaitHandle CreateOrOpenEventHandle(out bool createdNew)
        {
            return new EventWaitHandle(false, EventResetMode.AutoReset, typeof(SharedMemoryCommon).FullName, out createdNew);
        }

        public static MemoryMappedFile CreateOrOpenMemoryMappedFile()
        {
            string mapName = typeof(SharedMemoryCommon).FullName + "File";
            return MemoryMappedFile.CreateOrOpen(mapName, Capacity);
        }
    }

    /// <summary>
    /// An interprocess communication client implemented with a shared memory mapped file and a thread synchronization event
    /// </summary>
    public class SharedMemoryClient : IIpcClient
    {
        private readonly EventWaitHandle _dataAvailable;

        public SharedMemoryClient(EventWaitHandle dataAvailable)
        {
            _dataAvailable = dataAvailable;
        }

        public void Send(string data)
        {
            using (var memFile = SharedMemoryCommon.CreateOrOpenMemoryMappedFile())
            using (var memView = memFile.CreateViewAccessor())
            {
                var bytes = Encoding.Default.GetBytes(data);
                memView.Write(SharedMemoryCommon.DataLengthPosition, bytes.Length);
                memView.WriteArray(SharedMemoryCommon.DataPosition, bytes, 0, bytes.Length);
                _dataAvailable.Set();
            }
        }

        public void Dispose()
        {
            _dataAvailable.Dispose();
        }
    }

    /// <summary>
    /// An interprocess communication server implemented with a shared memory mapped file and a thread synchronization event
    /// </summary>
    public class SharedMemoryServer : IIpcServer
    {
        private readonly EventWaitHandle _dataAvailable;
        private readonly ManualResetEvent _killer = new ManualResetEvent(false);
        
        public SharedMemoryServer(EventWaitHandle dataAvailable)
        {
            _dataAvailable = dataAvailable;
        }

        private void OnReceived(DataReceivedEventArgs e)
        {
            Received?.Invoke(this, e);
        }

        public event EventHandler<DataReceivedEventArgs> Received;

        public void Start()
        {
            Task.Factory.StartNew(() =>
            {
                using (var memFile = SharedMemoryCommon.CreateOrOpenMemoryMappedFile())
                using (var memView = memFile.CreateViewAccessor())
                {
                    // Continue waiting and looping while data available satisfied the wait; stop looping when killer event is set
                    while (WaitHandle.WaitAny(new WaitHandle[] { _killer, _dataAvailable }) == 1)
                    {
                        int length = memView.ReadInt32(SharedMemoryCommon.DataLengthPosition);
                        var data = new byte[length];

                        memView.ReadArray(SharedMemoryCommon.DataPosition, data, 0, length);
                        OnReceived(new DataReceivedEventArgs(Encoding.Default.GetString(data)));
                    }
                }
            });
        }

        public void Stop()
        {
            _killer.Set();
        }

        public void Dispose()
        {
            Stop();
            _killer.Dispose();
            _dataAvailable.Dispose();
        }
    }
}
