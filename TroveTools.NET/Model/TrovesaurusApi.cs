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
        public const string ModPacksUrl = TrovesaurusBaseUrl + "modpacks";
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
        public const string MailCountUrl = TrovesaurusBaseUrl + "toolbox/mailcount.php";
        public const string MailboxUrl = TrovesaurusBaseUrl + "mail";

        public const string ModLoaderQuerystring = "ml=TroveTools.NET";
        public const string ModPackRegex = @"<h3><a href=""(?<Url>https://www\.trovesaurus\.com/modpack=(?<PackId>\d+)/[^""]*)"">(?<Name>[^<]*)</a></h3>.*?Created by <a href=[^>]+>(?<Author>[^<]+)</a>(?<Details>.*?)<hr/>";
        public const string PackModsRegex = @"<a href=""https://www\.trovesaurus\.com/mod=(?<ModId>\d+)";

        private static List<TroveMod> _ModList = null;
        private static List<TroveModPack> _ModPackList = null;
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
        public static List<TroveModPack> ModPackList
        {
            get
            {
                if (_ModPackList == null) RefreshModPackList();
                return _ModPackList;
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
            string url = AddQuerystring(ModListUrl);
            using (var client = OpenWebClient()) _ModList = JsonConvert.DeserializeObject<List<TroveMod>>(client.DownloadString(url));
        }

        public static TroveMod GetMod(string id, string name = "")
        {
            var ic = StringComparison.OrdinalIgnoreCase;
            var mod = ModList.FirstOrDefault(m => m.Id.Equals(id, ic));
            if (mod == null) mod = ModList.FirstOrDefault(m => TroveMod.FilterModFilename(m.Name).Equals(TroveMod.FilterModFilename(name), ic));
            return mod;
        }

        public static void RefreshModPackList()
        {
            string url = AddQuerystring(ModPacksUrl), packsHtml;
            using (var client = OpenWebClient()) packsHtml = client.DownloadString(url);

            if (_ModPackList == null)
                _ModPackList = new List<TroveModPack>();
            else
                _ModPackList.Clear();

            // Parse mod packs HTML
            foreach (Match packMatch in Regex.Matches(packsHtml, ModPackRegex, RegexOptions.Singleline))
            {
                var mods = Regex.Matches(packMatch.Groups["Details"].Value, PackModsRegex, RegexOptions.Singleline);
                if (mods.Count > 0)
                {
                    TroveModPack pack = new TroveModPack();
                    pack.PackId = packMatch.Groups["PackId"].Value;
                    pack.Url = packMatch.Groups["Url"].Value;
                    pack.Name = packMatch.Groups["Name"].Value;
                    pack.Author = packMatch.Groups["Author"].Value;
                    pack.Source = "Trovesaurus";
                    
                    foreach (Match modMatch in mods)
                    {
                        var mod = ModList.FirstOrDefault(m => m.Id == modMatch.Groups["ModId"].Value);
                        if (mod != null) pack.Mods.Add(mod);
                    }
                    _ModPackList.Add(pack);
                }
            }
        }

        public static TroveModPack GetModPack(string packId)
        {
            return ModPackList.FirstOrDefault(p => p.PackId == packId);
        }

        public static void RefreshNewsList()
        {
            string url = AddQuerystring(NewsUrl);
            using (var client = OpenWebClient()) _NewsList = JsonConvert.DeserializeObject<List<TrovesaurusNewsItem>>(client.DownloadString(url));
        }

        public static void RefreshCalendarList()
        {
            string url = AddQuerystring(CalendarUrl, true);
            using (var client = OpenWebClient()) _CalendarList = JsonConvert.DeserializeObject<List<TrovesaurusCalendarItem>>(client.DownloadString(url));
        }

        public static void RefreshStreamList()
        {
            string url = AddQuerystring(OnlineStreamsUrl);
            using (var client = OpenWebClient()) _StreamList = JsonConvert.DeserializeObject<List<TrovesaurusOnlineStream>>(client.DownloadString(url));
        }

        public static int GetMailCount()
        {
            string url = AddQuerystring(MailCountUrl);
            using (var client = OpenWebClient()) return Convert.ToInt32(client.DownloadString(url));
        }

        public static TroveServerStatus GetServerStatus()
        {
            string url = AddQuerystring(ServerStatusUrl, true);
            using (var client = OpenWebClient()) return JsonConvert.DeserializeObject<TroveServerStatus>(client.DownloadString(url));
        }

        public static string GetServerStatusHtml()
        {
            string url = AddQuerystring(ServerStatusHtmlUrl);
            using (var client = OpenWebClient()) return client.DownloadString(url);
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
                string url = AddQuerystring(string.Format(ModDownloadUrl, mod.Id, fileId));
                client.DownloadFile(url, localPath);
            }
            mod.CurrentFileId = fileId;
            try { mod.UnixTimeSeconds = Convert.ToInt64(unixDate); } catch { }

            return localPath;
        }

        public static void LaunchTrovesaurus(string url = TrovesaurusBaseUrl)
        {
            // Launch site in default browser
            string launchUrl = AddQuerystring(url ?? TrovesaurusBaseUrl);
            if (!string.IsNullOrEmpty(launchUrl))
                Process.Start(launchUrl);
            else
                log.ErrorFormat("Invalid URL to launch passed: [{0}]", url);
        }

        public static void LaunchModSite(string id)
        {
            // Launch site in default browser
            string url = AddQuerystring(string.Format(ModViewUrl, id));
            Process.Start(url);
        }

        public static void LaunchTrovesaurusNewsTag(string tag)
        {
            // Launch site in default browser
            string url = AddQuerystring(string.Format(NewsTagUrl, tag));
            Process.Start(url);
        }

        public static string UpdateTroveGameStatus(bool online)
        {
            using (var client = OpenWebClient())
            {
                string url = AddQuerystring(online ? string.Format(TroveLaunchUrl, SettingsDataProvider.TrovesaurusAccountLinkKey) :
                    string.Format(TroveCloseUrl, SettingsDataProvider.TrovesaurusAccountLinkKey), includeKey: false);
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
        /// Returns a URL with the name of the mod loader added as a querystring
        /// </summary>
        private static string AddQuerystring(string url, bool includeTicks = false, bool includeKey = true)
        {
            StringBuilder newUrl = new StringBuilder();
            newUrl.Append(url);
            newUrl.Append(url.Contains("?") ? "&" : "?");
            newUrl.Append(ModLoaderQuerystring);
            if (includeKey && !string.IsNullOrEmpty(SettingsDataProvider.TrovesaurusAccountLinkKey)) newUrl.AppendFormat("&key={0}", SettingsDataProvider.TrovesaurusAccountLinkKey);
            if (includeTicks) newUrl.AppendFormat("&ticks={0}", DateTime.Now.Ticks);
            return newUrl.ToString();
        }
        #endregion
    }
}
