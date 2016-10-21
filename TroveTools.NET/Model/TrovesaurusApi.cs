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
        public const string ModPacksFeedUrl = TrovesaurusBaseUrl + "feeds/modpacks";
        public const string ModPackCreateUrl = TrovesaurusBaseUrl + "feeds/createmodpack.php?name={0}&mods={1}";
        public const string UserProfileUrl = TrovesaurusBaseUrl + "user={0}/{1}";
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
        public const string TroveLaunchUrl = TrovesaurusBaseUrl + "toolbox/ping.php?action=launch";
        public const string TroveCloseUrl = TrovesaurusBaseUrl + "toolbox/ping.php?action=close";
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
            try
            {
                string url = AddQuerystring(ModListUrl);
                using (var client = OpenWebClient()) _ModList = JsonConvert.DeserializeObject<List<TroveMod>>(client.DownloadString(url));

                // Save mod list to settings
                try { SettingsDataProvider.TrovesaurusMods = _ModList; } catch { }
            }
            catch (Exception ex) { log.Error("Error refreshing Trovesaurus mod list", ex); }
            finally
            {
                // Load from cached mod list if an error occurred
                if (_ModList == null) _ModList = SettingsDataProvider.TrovesaurusMods;
            }
        }

        public static TroveMod GetMod(string id, string name = "")
        {
            try
            {
                var ic = StringComparison.OrdinalIgnoreCase;
                var mod = ModList.FirstOrDefault(m => m.Id.Equals(id, ic));
                if (mod == null)
                    mod = ModList.FirstOrDefault(m => TroveMod.FilterModFilename(m.Name).Equals(TroveMod.FilterModFilename(name), ic));
                return mod;
            }
            catch (Exception ex) { log.ErrorFormat("Error retrieving matching mod from Trovesaurus for mod {0}: {1}", string.IsNullOrEmpty(name) ? "id " + id : name, ex.Message); }
            return null;
        }

        public static void RefreshModPackList()
        {
            try
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
            catch (Exception ex) { log.Error("Error refreshing Trovesaurus mod pack list", ex); }
            finally { if (_ModPackList == null) _ModPackList = new List<TroveModPack>(); }
        }

        public static TroveModPack GetModPack(string packId)
        {
            return ModPackList.FirstOrDefault(p => p.PackId == packId);
        }

        public static void RefreshNewsList()
        {
            try
            {
                string url = AddQuerystring(NewsUrl);
                using (var client = OpenWebClient()) _NewsList = JsonConvert.DeserializeObject<List<TrovesaurusNewsItem>>(client.DownloadString(url));
            }
            catch (Exception ex) { log.Error("Error refreshing Trovesaurus news list", ex); }
            finally { if (_NewsList == null) _NewsList = new List<TrovesaurusNewsItem>(); }
        }

        public static void RefreshCalendarList()
        {
            try
            {
                string url = AddQuerystring(CalendarUrl, true);
                using (var client = OpenWebClient()) _CalendarList = JsonConvert.DeserializeObject<List<TrovesaurusCalendarItem>>(client.DownloadString(url));
            }
            catch (Exception ex) { log.Error("Error refreshing Trovesaurus calendar list", ex); }
            finally { if (_CalendarList == null) _CalendarList = new List<TrovesaurusCalendarItem>(); }
        }

        public static void RefreshStreamList()
        {
            try
            {
                string url = AddQuerystring(OnlineStreamsUrl);
                using (var client = OpenWebClient()) _StreamList = JsonConvert.DeserializeObject<List<TrovesaurusOnlineStream>>(client.DownloadString(url));
            }
            catch (Exception ex) { log.Error("Error refreshing Trovesaurus online streams list", ex); }
            finally { if (_StreamList == null) _StreamList = new List<TrovesaurusOnlineStream>(); }
        }

        public static int? GetMailCount()
        {
            string content = null;
            try
            {
                string url = AddQuerystring(MailCountUrl);
                using (var client = OpenWebClient()) content = client.DownloadString(url);
                return Convert.ToInt32(content);
            }
            catch (Exception ex) { log.ErrorFormat("Error getting Trovesaurus mail count (content {0}): {1}", content, ex.Message); }
            return null;
        }

        public static TroveServerStatus GetServerStatus()
        {
            try
            {
                string url = AddQuerystring(ServerStatusUrl, true);
                using (var client = OpenWebClient()) return JsonConvert.DeserializeObject<TroveServerStatus>(client.DownloadString(url));
            }
            catch (Exception ex) { log.Error("Error getting Server Status from Trovesaurus", ex); }
            return null;
        }

        public static string GetServerStatusHtml()
        {
            try
            {
                string url = AddQuerystring(ServerStatusHtmlUrl);
                using (var client = OpenWebClient()) return client.DownloadString(url);
            }
            catch (Exception ex) { log.Error("Error getting Server Status HTML from Trovesaurus", ex); }
            return null;
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

            var download = mod.Downloads.First(m => m.FileId == fileId);
            string unixDate = download.Date;
            string fileName = string.Format("{0}+{1}.{2}", TroveMod.FilterModFilename(mod.Name), unixDate, string.IsNullOrEmpty(download.Format) ? "zip" : download.Format);
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
            string launchUrl = AddQuerystring(url ?? TrovesaurusBaseUrl);
            if (!string.IsNullOrEmpty(launchUrl))
            {
                // Launch site in default browser
                Process.Start(launchUrl);
            }
            else
                log.ErrorFormat("Invalid URL to launch passed: [{0}]", url);
        }

        public static void LaunchModSite(string id)
        {
            LaunchTrovesaurus(string.Format(ModViewUrl, id));
        }

        public static void LaunchTrovesaurusNewsTag(string tag)
        {
            LaunchTrovesaurus(string.Format(NewsTagUrl, tag));
        }

        public static void LaunchUserProfile(string userId, string userName)
        {
            LaunchTrovesaurus(string.Format(UserProfileUrl, userId, StandardizeNameForUrl(userName)));
        }

        public static string UpdateTroveGameStatus(bool online)
        {
            try
            {
                using (var client = OpenWebClient())
                {
                    string url = AddQuerystring(online ? TroveLaunchUrl : TroveCloseUrl);
                    return client.DownloadString(url);
                }
            }
            catch (Exception ex) { log.Error("Error updating Trovesaurus online game status", ex); }
            return null;
        }

        public static void DownloadFile(string downloadUrl, string localPath)
        {
            using (var client = OpenWebClient())
            {
                string url = AddQuerystring(downloadUrl);
                client.DownloadFile(url, localPath);
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

        private static string StandardizeNameForUrl(string name)
        {
            return Regex.Replace(name.ToLower(), @"[^ a-z]", "").Replace(" ", "-");
        }
        #endregion
    }
}
