using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TroveTools.NET.Model;
using TroveTools.NET.Properties;

namespace TroveTools.NET.DataAccess
{
    static class SettingsDataProvider
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static JsonSerializerSettings jsonSettings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore
        };

        public static string AppDataFolder
        {
            get { return ResolveFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TroveTools.NET")); }
        }

        public static string ModsFolder
        {
            get { return ResolveFolder(Path.Combine(AppDataFolder, "mods")); }
        }

        public static string TroveToolboxModsFolder
        {
            get
            {
                string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Trove Toolbox", "mods");
                if (Directory.Exists(folder)) return folder;
                return null;
            }
        }

        public static List<TroveLocation> Locations
        {
            get
            {
                if (string.IsNullOrEmpty(Settings.Default.LocationsJson))
                    return new List<TroveLocation>();
                else
                    return JsonConvert.DeserializeObject<List<TroveLocation>>(Settings.Default.LocationsJson);
            }
            set
            {
                Settings.Default.LocationsJson = JsonConvert.SerializeObject(value, jsonSettings);
                Settings.Default.Save();
            }
        }

        public static List<TroveMod> MyMods
        {
            get
            {
                if (string.IsNullOrEmpty(Settings.Default.MyModsJson))
                    return new List<TroveMod>();
                else
                    return JsonConvert.DeserializeObject<List<TroveMod>>(Settings.Default.MyModsJson);
            }
            set
            {
                Settings.Default.MyModsJson = JsonConvert.SerializeObject(value, jsonSettings);
                Settings.Default.Save();
            }
        }

        public static List<TroveMod> TrovesaurusMods
        {
            get
            {
                if (string.IsNullOrEmpty(Settings.Default.TrovesaurusModsJson))
                    return new List<TroveMod>();
                else
                    return JsonConvert.DeserializeObject<List<TroveMod>>(Settings.Default.TrovesaurusModsJson);
            }
            set
            {
                Settings.Default.TrovesaurusModsJson = JsonConvert.SerializeObject(value, jsonSettings);
                Settings.Default.Save();
            }
        }

        public static List<TroveModPack> MyModPacks
        {
            get
            {
                if (string.IsNullOrEmpty(Settings.Default.MyModPacksJson))
                    return new List<TroveModPack>();
                else
                    return JsonConvert.DeserializeObject<List<TroveModPack>>(Settings.Default.MyModPacksJson);
            }
            set
            {
                Settings.Default.MyModPacksJson = JsonConvert.SerializeObject(value, jsonSettings);
                Settings.Default.Save();
            }
        }

        public static string LastAddModLocation
        {
            get { return Settings.Default.LastAddModLocation; }
            set
            {
                Settings.Default.LastAddModLocation = value;
                Settings.Default.Save();
            }
        }

        public static string TrovesaurusAccountLinkKey
        {
            get { return Settings.Default.TrovesaurusAccountLinkKey; }
            set
            {
                Settings.Default.TrovesaurusAccountLinkKey = value;
                Settings.Default.Save();
            }
        }

        public static bool UpdateTroveGameStatus
        {
            get { return Settings.Default.UpdateTroveGameStatus; }
            set
            {
                Settings.Default.UpdateTroveGameStatus = value;
                Settings.Default.Save();
            }
        }

        public static bool TrovesaurusCheckMail
        {
            get { return Settings.Default.TrovesaurusCheckMail; }
            set
            {
                Settings.Default.TrovesaurusCheckMail = value;
                Settings.Default.Save();
            }
        }

        public static bool TrovesaurusServerStatus
        {
            get { return Settings.Default.TrovesaurusServerStatus; }
            set
            {
                Settings.Default.TrovesaurusServerStatus = value;
                Settings.Default.Save();
            }
        }

        public static bool StartMinimized
        {
            get { return Settings.Default.StartMinimized; }
            set
            {
                Settings.Default.StartMinimized = value;
                Settings.Default.Save();
            }
        }

        public static bool MinimizeToTray
        {
            get { return Settings.Default.MinimizeToTray; }
            set
            {
                Settings.Default.MinimizeToTray = value;
                Settings.Default.Save();
            }
        }

        public static bool IsTroveUrlProtocolRegistered
        {
            get { return Settings.Default.IsTroveUrlProtocolRegistered; }
            set
            {
                Settings.Default.IsTroveUrlProtocolRegistered = value;
                Settings.Default.Save();
            }
        }

        public static bool AutoUpdateMods
        {
            get { return Settings.Default.AutoUpdateMods; }
            set
            {
                Settings.Default.AutoUpdateMods = value;
                Settings.Default.Save();
            }
        }

        public static TimeSpan AutoUpdateInterval
        {
            get { return Settings.Default.AutoUpdateInterval; }
            set
            {
                Settings.Default.AutoUpdateInterval = value;
                Settings.Default.Save();
            }
        }

        /// <summary>
        /// Resolves the path to a folder and creates the folder if it does not already exist
        /// </summary>
        public static string ResolveFolder(string path)
        {
            if (!Path.IsPathRooted(path)) path = Path.GetFullPath(path);
            if (!Directory.Exists(path) && !File.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

        /// <summary>
        /// Returns a safe filename by replacing all invalid characters with underscores
        /// </summary>
        public static string GetSafeFilename(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
