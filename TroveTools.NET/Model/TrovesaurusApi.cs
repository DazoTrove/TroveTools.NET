using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
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
        public const string CalendarPageUrl = TrovesaurusBaseUrl + "calendar";
        public const string NewsUrl = TrovesaurusBaseUrl + "feeds/news.php";
        public const string NewsPageUrl = TrovesaurusBaseUrl + "news";
        public const string NewsTagUrl = TrovesaurusBaseUrl + "news.php?t={0}";
        public const string ServerStatusUrl = TrovesaurusBaseUrl + "statusjson.php";
        public const string ServerStatusHtmlUrl = TrovesaurusBaseUrl + "_status.php";
        public const string ServerStatusPage = TrovesaurusBaseUrl + "status.php";
        public const string OnlineStreamsUrl = TrovesaurusBaseUrl + "feeds/onlinestreams.php";
        public const string OnlineStreamsPageUrl = TrovesaurusBaseUrl + "livestreams.php";
        public const string TroveLaunchUrl = TrovesaurusBaseUrl + "toolbox/ping.php?id={0}&action=launch";
        public const string TroveCloseUrl = TrovesaurusBaseUrl + "toolbox/ping.php?id={0}&action=close";
        public const string MailCountUrl = TrovesaurusBaseUrl + "toolbox/mailcount.php?key={0}";
        public const string MailboxUrl = TrovesaurusBaseUrl + "mail";

        private static List<TroveMod> _ModList = null;
        private static List<TrovesaurusNewsItem> _NewsList = null;
        private static List<TrovesaurusCalendarItem> _CalendarList = null;
        private static List<TrovesaurusOnlineStream> _StreamList = null;

        #region Public Static Properties
        public static List<TroveMod> ModList
        {
            get
            {
                if (_ModList == null) RefreshModList();
                return _ModList;
            }
        }

        public static List<TrovesaurusNewsItem> NewsList
        {
            get
            {
                if (_NewsList == null) RefreshNewsList();
                return _NewsList;
            }
        }

        public static List<TrovesaurusCalendarItem> CalendarList
        {
            get
            {
                if (_CalendarList == null) RefreshCalendarList();
                return _CalendarList;
            }
        }

        public static List<TrovesaurusOnlineStream> StreamList
        {
            get
            {
                if (_StreamList == null) RefreshStreamList();
                return _StreamList;
            }
        }
        #endregion

        #region Public Static Methods
        public static void RefreshModList()
        {
            using (var client = OpenWebClient()) _ModList = JsonConvert.DeserializeObject<List<TroveMod>>(client.DownloadString(ModListUrl));
        }

        public static void RefreshNewsList()
        {
            using (var client = OpenWebClient()) _NewsList = JsonConvert.DeserializeObject<List<TrovesaurusNewsItem>>(client.DownloadString(NewsUrl));
        }

        public static void RefreshCalendarList()
        {
            using (var client = OpenWebClient()) _CalendarList = JsonConvert.DeserializeObject<List<TrovesaurusCalendarItem>>(client.DownloadString(AddTicksQuerystring(CalendarUrl)));
        }

        public static void RefreshStreamList()
        {
            using (var client = OpenWebClient()) _StreamList = JsonConvert.DeserializeObject<List<TrovesaurusOnlineStream>>(client.DownloadString(OnlineStreamsUrl));
        }

        public static int GetMailCount(string trovesaurusAccountLinkKey)
        {
            using (var client = OpenWebClient()) return Convert.ToInt32(client.DownloadString(string.Format(MailCountUrl, trovesaurusAccountLinkKey)));
        }

        public static TroveServerStatus GetServerStatus()
        {
            using (var client = OpenWebClient()) return JsonConvert.DeserializeObject<TroveServerStatus>(client.DownloadString(AddTicksQuerystring(ServerStatusUrl)));
        }

        public static string GetServerStatusHtml()
        {
            using (var client = OpenWebClient()) return client.DownloadString(ServerStatusHtmlUrl);
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
            string fileName = string.Format("{0}+{1}.zip", TroveMod.FilterModFilename(mod.Name), unixDate);
            string localPath = Path.Combine(SettingsDataProvider.ModsFolder, fileName);

            using (var client = OpenWebClient())
            {
                client.DownloadFile(string.Format(ModDownloadUrl, mod.Id, fileId), localPath);
            }
            try { mod.UnixTimeSeconds = Convert.ToInt64(unixDate); } catch { }

            return localPath;
        }

        public static void LaunchTrovesaurus(string url = TrovesaurusBaseUrl)
        {
            // Launch site in default browser
            string launchUrl = url ?? TrovesaurusBaseUrl;
            if (!string.IsNullOrEmpty(launchUrl))
                Process.Start(launchUrl);
            else
                log.ErrorFormat("Invalid URL to launch passed: [{0}]", url);
        }

        public static void LaunchModSite(string id)
        {
            // Launch site in default browser
            Process.Start(string.Format(ModViewUrl, id));
        }

        public static void LaunchTrovesaurusNewsTag(string tag)
        {
            // Launch site in default browser
            Process.Start(string.Format(NewsTagUrl, tag));
        }

        public static string UpdateTroveGameStatus(string accountLinkKey, bool online)
        {
            using (var client = OpenWebClient())
            {
                string url = online ? string.Format(TroveLaunchUrl, accountLinkKey) : string.Format(TroveCloseUrl, accountLinkKey);
                return client.DownloadString(url);
            }
        }
        #endregion

        #region Private static methods
        private static WebClientExtended OpenWebClient()
        {
            WebClientExtended client = new WebClientExtended();

            client.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore); // no caching
            client.KeepAlive = true;
            client.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            return client;
        }

        /// <summary>
        /// Returns a URL with the current time ticks to attempt to prevent caching
        /// </summary>
        private static string AddTicksQuerystring(string url)
        {
            if (url.Contains("?")) return string.Format("{0}&ticks={1}", url, DateTime.Now.Ticks);
            return string.Format("{0}?ticks={1}", url, DateTime.Now.Ticks);
        }
        #endregion
    }
}
