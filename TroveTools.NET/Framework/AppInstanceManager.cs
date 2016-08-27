using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TroveTools.NET.Framework
{
    public static class AppInstanceManager
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static bool CreateSingleInstance(EventHandler<DataReceivedEventArgs> handler, string data)
        {
            bool isFirstInstance = true;
            try
            {
                var eventHandle = SharedMemoryCommon.CreateOrOpenEventHandle(out isFirstInstance);
                if (isFirstInstance)
                {
                    IIpcServer server = new SharedMemoryServer(eventHandle);
                    server.Received += handler;
                    server.Start();

                    // Register process exit handler to stop the server
                    Process process = Process.GetCurrentProcess();
                    process.Exited += (s, e) =>
                    {
                        log.Info("Closing interprocess communication server");
                        try { server.Dispose(); }
                        catch (Exception ex) { log.Error("Error closing interprocess communication server", ex); }
                    };
                }
                else
                {
                    using (IIpcClient client = new SharedMemoryClient(eventHandle)) client.Send(data);
                }
            }
            catch (Exception ex) { log.Error("Error creating single instance of application", ex); }
            return isFirstInstance;
        }
    }
}
