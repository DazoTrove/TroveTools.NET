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
                Settings.Default.Reload();
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
                Settings.Default.Reload();
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

        public static string LastAddModLocation
        {
            get
            {
                Settings.Default.Reload();
                return Settings.Default.LastAddModLocation;
            }
            set
            {
                Settings.Default.LastAddModLocation = value;
                Settings.Default.Save();
            }
        }

        public static string TrovesaurusAccountLinkKey
        {
            get
            {
                Settings.Default.Reload();
                return Settings.Default.TrovesaurusAccountLinkKey;
            }
            set
            {
                Settings.Default.TrovesaurusAccountLinkKey = value;
                Settings.Default.Save();
            }
        }

        public static bool UpdateTroveGameStatus
        {
            get
            {
                Settings.Default.Reload();
                return Settings.Default.UpdateTroveGameStatus;
            }
            set
            {
                Settings.Default.UpdateTroveGameStatus = value;
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
    }
}
