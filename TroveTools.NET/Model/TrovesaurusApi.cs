using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TroveTools.NET.DataAccess;
using TroveTools.NET.Framework;

namespace TroveTools.NET.Model
{
    static class TrovesaurusApi
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public const string TrovesaurusBaseUrl = "https://www.trovesaurus.com/";
        public const string ModListUrl = TrovesaurusBaseUrl + "modsapi.php?mode=list";
        public const string ModDownloadUrl = TrovesaurusBaseUrl + "mod.php?id={0}&download={1}";
        public const string ModViewUrl = TrovesaurusBaseUrl + "mod.php?id={0}";
        public const string CalendarUrl = TrovesaurusBaseUrl + "toolbox/calendar.php";
        public const string NewsUrl = TrovesaurusBaseUrl + "feeds/news.php";
        public const string NewsPageUrl = TrovesaurusBaseUrl + "toolbox/page.php?id={0}";
        public const string ServerStatusUrl = TrovesaurusBaseUrl + "statusjson.php";
        public const string CommandsUrl = TrovesaurusBaseUrl + "toolbox/page.php?id=262";
        public const string TwitchStreamsUrl = TrovesaurusBaseUrl + "feeds/onlinestreams.php";
        public const string TroveLaunchUrl = TrovesaurusBaseUrl + "toolbox/ping.php?id={0}&action=launch";
        public const string TroveCloseUrl = TrovesaurusBaseUrl + "toolbox/ping.php?id={0}&action=close";
        public const string MailCountUrl = TrovesaurusBaseUrl + "toolbox/mailcount.php?key={0}";
        public const string MailboxUrl = TrovesaurusBaseUrl + "mail.php";
        public const string ModInfoUrl = TrovesaurusBaseUrl + "toolbox/modinfo.php?id={0}";

        private static List<TroveMod> _modList = null;

        public static List<TroveMod> ModList
        {
            get
            {
                if (_modList == null) RefreshModList();
                return _modList;
            }
        }

        public static void RefreshModList()
        {
            using (WebClient client = new WebClient())
            {
                _modList = JsonConvert.DeserializeObject<List<TroveMod>>(client.DownloadString(ModListUrl));
            }
        }

        public static string DownloadMod(TroveMod mod)
        {
            if (mod?.Id == null) throw new ArgumentNullException("mod.Id");
            if (mod?.Downloads == null) throw new ArgumentNullException("mod.Downloads");

            return DownloadMod(mod, mod.LatestDownload.FileId);
        }

        public static string DownloadMod(TroveMod mod, string fileId)
        {
            if (mod?.Id == null) throw new ArgumentNullException("mod.Id");
            if (mod?.Downloads == null) throw new ArgumentNullException("mod.Downloads");

            string unixDate = mod.Downloads.First(m => m.FileId == fileId).Date;
            try { mod.UnixTimeSeconds = Convert.ToInt64(unixDate); } catch { }

            string fileName = string.Format("{0}+{1}.zip", TroveMod.FilterModFilename(mod.Name), unixDate);

            using (WebClient client = new WebClient())
            {
                string localPath = Path.Combine(SettingsDataProvider.ModsFolder, fileName);
                client.DownloadFile(string.Format(ModDownloadUrl, mod.Id, fileId), localPath);
                return localPath;
            }
        }

        public static void LaunchModSite(string id)
        {
            // Launch site in default browser
            Process.Start(string.Format(ModViewUrl, id));
        }

        public static void LaunchTrovesaurus()
        {
            // Launch site in default browser
            Process.Start(TrovesaurusBaseUrl);
        }

        public static string UpdateTroveGameStatus(string accountLinkKey, bool online)
        {
            using (WebClient client = new WebClient())
            {
                string url = online ? string.Format(TroveLaunchUrl, accountLinkKey) : string.Format(TroveCloseUrl, accountLinkKey);
                return client.DownloadString(url);
            }
        }
    }
}
