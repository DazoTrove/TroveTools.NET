using log4net;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TroveTools.NET.Framework;

namespace TroveTools.NET.DataAccess
{
    class RegistrySettings
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const string TroveKeyName = "trove";
        private const string TroveUrlProtocol = @"HKEY_CLASSES_ROOT\" + TroveKeyName;
        private const string TroveUrlProtocolOpenCommand = TroveUrlProtocol + @"\shell\open\command";
        private const string TroveUrlProtocolValue = "URL: Trove Protocol";
        private const string UrlProtocolValueName = "URL Protocol";
        private const string OpenCommandFormat = "\"{0}\" \"%1\"";

        private const string WinUninstall = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
        private const string Win64Uninstall = @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
        private const string WinUninstallTroveLive = WinUninstall + @"\Glyph Trove";
        private const string Win64UninstallTroveLive = Win64Uninstall + @"\Glyph Trove";
        private const string WinUninstallTrovePTS = WinUninstall + @"\Glyph Trove PTS-US";
        private const string Win64UninstallTrovePTS = Win64Uninstall + @"\Glyph Trove PTS-US";
        private const string LocationValue = "InstallLocation";

        public static bool IsTroveUrlProtocolRegistered
        {
            get
            {
                bool registered = false;
                try
                {
                    string value = Registry.GetValue(TroveUrlProtocol, "", string.Empty) as string;
                    string urlProtocol = Registry.GetValue(TroveUrlProtocol, UrlProtocolValueName, string.Empty) as string;
                    string openCommand = Registry.GetValue(TroveUrlProtocolOpenCommand, "", string.Empty) as string;
                    string troveToolsOpenCommand = string.Format(OpenCommandFormat, Assembly.GetEntryAssembly().Location);

                    if (value == TroveUrlProtocolValue && urlProtocol != null && openCommand == troveToolsOpenCommand) registered = true;
                }
                catch (Exception ex)
                {
                    log.Error("Error getting Trove URL Protocol settings from Windows Registry", ex);
                }
                return registered;
            }
        }

        public static void RegisterTroveUrlProtocol()
        {
            try
            {
                log.Info("Registering Trove URL Protocol in Windows Registry");
                string troveToolsOpenCommand = string.Format(OpenCommandFormat, Assembly.GetEntryAssembly().Location);

                Registry.SetValue(TroveUrlProtocol, "", TroveUrlProtocolValue);
                Registry.SetValue(TroveUrlProtocol, UrlProtocolValueName, "");
                Registry.SetValue(TroveUrlProtocolOpenCommand, "", troveToolsOpenCommand);

                log.Info("Registered Trove URL Protocol in Windows Registry");
            }
            catch (Exception ex)
            {
                log.Error("Error registering Trove URL Protocol settings in Windows Registry", ex);
            }
        }

        public static void UnregisterTroveUrlProtocol()
        {
            try
            {
                log.Info("Removing Trove URL Protocol registration from Windows Registry");

                Registry.ClassesRoot.DeleteSubKeyTree(TroveKeyName, false);

                log.Info("Removed Trove URL Protocol registration from Windows Registry");
            }
            catch (Exception ex)
            {
                log.Error("Error removing Trove URL Protocol registration from Windows Registry", ex);
            }
        }

        public static void GetTroveLocations(Dictionary<string, string> potentialLocs)
        {
            try
            {
                string path = Registry.GetValue(Win64UninstallTroveLive, LocationValue, null) as string;
                if (path != null) potentialLocs.AddIfMissing(path, "Trove Live (Registry)");

                path = Registry.GetValue(WinUninstallTroveLive, LocationValue, null) as string;
                if (path != null) potentialLocs.AddIfMissing(path, "Trove Live (Registry)");

                path = Registry.GetValue(Win64UninstallTrovePTS, LocationValue, null) as string;
                if (path != null) potentialLocs.AddIfMissing(path, "Trove PTS (Registry)");

                path = Registry.GetValue(WinUninstallTrovePTS, LocationValue, null) as string;
                if (path != null) potentialLocs.AddIfMissing(path, "Trove PTS (Registry)");
            }
            catch (Exception ex)
            {
                log.Error("Error getting Trove locations from Windows Registry", ex);
            }
        }
    }
}
