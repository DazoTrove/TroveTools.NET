using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TroveTools.NET.Framework
{
    /// <summary>
    /// Interface for an interprocess communication client
    /// </summary>
    public interface IIpcClient : IDisposable
    {
        void Send(string data);
    }

    /// <summary>
    /// Interface for an interprocess communication server
    /// </summary>
    public interface IIpcServer : IDisposable
    {
        void Start();
        void Stop();

        event EventHandler<DataReceivedEventArgs> Received;
    }

    /// <summary>
    /// Event arguments class that contains the data transferred via interprocess communication
    /// </summary>
    [Serializable]
    public class DataReceivedEventArgs : EventArgs
    {
        public DataReceivedEventArgs(string data) { Data = data; }
        public string Data { get; private set; }
    }
}
