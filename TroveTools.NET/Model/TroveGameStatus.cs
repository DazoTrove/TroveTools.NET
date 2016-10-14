using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using SharpConfig;

namespace TroveTools.NET.Model
{
    static class TroveGameStatus
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static Timer _UpdateTroveGameStatusTimer = null;
        private static bool? _Online = null;

        public static void StartTimer(string accountLinkKey)
        {
            if (string.IsNullOrEmpty(accountLinkKey))
            {
                log.Error("Trove game status updating requires that a valid account link key is entered");
                return;
            }
            log.InfoFormat("Starting Trove game status checking");
            if (_UpdateTroveGameStatusTimer == null)
            {
                _UpdateTroveGameStatusTimer = new Timer();
                _UpdateTroveGameStatusTimer.Interval = new TimeSpan(0, 0, 1).TotalMilliseconds; // every one second
                _UpdateTroveGameStatusTimer.AutoReset = true;
                _UpdateTroveGameStatusTimer.Elapsed += _UpdateTroveGameStatusTimer_Elapsed;
            }
            _UpdateTroveGameStatusTimer.Start();
        }

        public static void StopTimer()
        {
            log.InfoFormat("Stopping Trove game status checking");
            try
            {
                _UpdateTroveGameStatusTimer?.Stop();
                TrovesaurusApi.UpdateTroveGameStatus(false);
            }
            catch (Exception ex) { log.Error("Error stopping Trove game status detection", ex); }
        }

        private static void _UpdateTroveGameStatusTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                bool isTroveRunning = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(TroveLocation.TroveExecutableFileName)).Length > 0;
                if (isTroveRunning)
                {
                    if (!_Online.HasValue || _Online.Value == false)
                    {
                        _Online = true;
                        string status = TrovesaurusApi.UpdateTroveGameStatus(_Online.Value);
                        log.InfoFormat("Trove game detected running, updated Trovesaurus game status (return value: {0})", status);
                    }
                }
                else
                {
                    if (!_Online.HasValue || _Online.Value == true)
                    {
                        _Online = false;
                        string status = TrovesaurusApi.UpdateTroveGameStatus(_Online.Value);
                        log.InfoFormat("Trove game detected not running, updated Trovesaurus game status (return value: {0})", status);
                    }
                }
            }
            catch (Exception ex) { log.Error("Error in Trove game status detection", ex); }
        }

        public static string TroveConfigPath
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Trove", "Trove.cfg"); }
        }

        public static Configuration TroveConfig
        {
            get { return Configuration.LoadFromFile(TroveConfigPath); }
        }

        public static bool? TroveUseOverrides
        {
            get { return TroveConfig["System"]["UseOverrides"].GetValue<bool?>(); }
            set
            {
                var config = TroveConfig;
                config["System"]["UseOverrides"].SetValue(value);
                config.SaveToFile(TroveConfigPath);
            }
        }

        public static bool? TroveDisableAllMods
        {
            get { return TroveConfig["Mods"]["DisableAllMods"].GetValue<bool?>(); }
            set
            {
                var config = TroveConfig;
                config["Mods"]["DisableAllMods"].SetValue(value);
                config.SaveToFile(TroveConfigPath);
            }
        }

        public static bool? TroveMultithreaded
        {
            get { return TroveConfig["User"]["Multithreaded"].GetValue<bool?>(); }
            set
            {
                var config = TroveConfig;
                config["User"]["Multithreaded"].SetValue(value);
                config.SaveToFile(TroveConfigPath);
            }
        }
    }
}
