using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace TroveTools.NET.Model
{
    static class TroveGameStatus
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static Timer _UpdateTroveGameStatusTimer = null;
        private static string _AccountLinkKey;
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
            _AccountLinkKey = accountLinkKey;
            _UpdateTroveGameStatusTimer.Start();
        }

        public static void StopTimer()
        {
            log.InfoFormat("Stopping Trove game status checking");
            _UpdateTroveGameStatusTimer.Stop();
        }

        private static void _UpdateTroveGameStatusTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            bool isTroveRunning = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(TroveLocation.TroveExecutableFileName)).Length > 0;
            if (isTroveRunning)
            {
                if (!_Online.HasValue || _Online.Value == false)
                {
                    _Online = true;
                    string status = TrovesaurusApi.UpdateTroveGameStatus(_AccountLinkKey, _Online.Value);
                    log.InfoFormat("Trove game detected running, updated Trovesaurus game status (return value: {0})", status);
                }
            }
            else
            {
                if (!_Online.HasValue || _Online.Value == true)
                {
                    _Online = false;
                    string status = TrovesaurusApi.UpdateTroveGameStatus(_AccountLinkKey, _Online.Value);
                    log.InfoFormat("Trove game detected not running, updated Trovesaurus game status (return value: {0})", status);
                }
            }
        }
    }
}
