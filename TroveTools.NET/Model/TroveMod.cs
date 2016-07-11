using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TroveTools.NET.Converter;
using TroveTools.NET.DataAccess;
using TroveTools.NET.Framework;
using TroveTools.NET.Properties;
using System.Runtime.Serialization;
using System.Windows;

namespace TroveTools.NET.Model
{
    /// <summary>
    /// Represents a downloaded mod that can be installed
    /// </summary>
    class TroveMod
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public const string OverrideFolder = "override";
        public const string IndexFile = "index.tfi";
        public const string FileTypeSearchPattern = "*.zip";
        public const string TroveUriFormat = "trove://{0};{1}";
        private static List<TroveMod> _myMods;

        #region Constructors
        public TroveMod() { }

        [JsonConstructor]
        public TroveMod(string Name, string FilePath, string ImagePath, string Status, bool Enabled, long UnixTimeSeconds)
        {
            this.Name = Name;
            this.FilePath = FilePath;
            this.ImagePath = ImagePath;
            this.Status = Status;
            _enabled = Enabled; // do not set Enabled property when constructing since it installs/uninstalls
            this.UnixTimeSeconds = UnixTimeSeconds;
        }

        public TroveMod(string filePath)
        {
            log.InfoFormat("Loading mod from file: {0}", filePath);
            FilePath = filePath;

            Match filenameMatch = Regex.Match(Path.GetFileNameWithoutExtension(filePath), @"^(?<Name>.*?)(?:\+(?<UnixTimeSeconds>\d+))?$");
            if (filenameMatch.Success)
                Name = filenameMatch.Groups["Name"].Value;
            else
                Name = Path.GetFileNameWithoutExtension(filePath);

            if (filenameMatch.Success && !string.IsNullOrEmpty(filenameMatch.Groups["UnixTimeSeconds"]?.Value))
                UnixTimeSeconds = Convert.ToInt64(filenameMatch.Groups["UnixTimeSeconds"].Value);
            else
                UnixTimeSeconds = new DateTimeOffset(new FileInfo(filePath).LastWriteTime).ToUnixTimeSeconds();

            // Attempt to find matching mod from Trovesaurus API and load additional data
            TroveMod mod = FindTrovesaurusMod();
            UpdatePropertiesFromTrovesaurus(mod);
        }
        #endregion

        #region Download Class
        /// <summary>
        /// Individual download file for a mod from Trovesaurus
        /// </summary>
        public class Download
        {
            [JsonProperty("fileid")]
            public string FileId { get; set; }

            [JsonProperty("version")]
            public string Version { get; set; }

            [JsonProperty("date")]
            public string Date { get; set; }

            [JsonProperty("downloads")]
            public int Downloads { get; set; }

            [JsonProperty("changes")]
            public string Changes { get; set; }

            public override string ToString()
            {
                return string.Format(Strings.TroveMod_Download_ToStringFormat, Version, UnixTimeSecondsToDateTimeConverter.GetLocalDateTime(Date), FileId, Downloads);
            }

            [JsonIgnore]
            public DateTime DateTime
            {
                get { return UnixTimeSecondsToDateTimeConverter.GetLocalDateTime(Date); }
            }
        }
        #endregion

        #region Trovesaurus Mod Properties
        [JsonProperty("id"), AffectsProperty("CanUpdateMod")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("subtype")]
        public string SubType { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("date")]
        public string DateCreated { get; set; }

        [JsonProperty("status2")]
        public string TrovesaurusStatus { get; set; }

        [JsonProperty("replaces")]
        public string Replaces { get; set; }

        [JsonProperty("totaldownloads")]
        public int TotalDownloads { get; set; }

        [JsonProperty("votes")]
        public int Votes { get; set; }

        [JsonProperty("views")]
        public int Views { get; set; }

        [JsonProperty("downloads"), AffectsProperty("CanUpdateMod"), AffectsProperty("LatestDownload"), AffectsProperty("CurrentDownload")]
        public List<Download> Downloads { get; set; }

        [JsonProperty("image")]
        public string ImagePath { get; set; }
        #endregion

        #region TroveTools.NET Properties
        public string FilePath { get; set; }

        [AffectsProperty("CanUpdateMod")]
        public string Status { get; set; } = string.Empty;

        private bool _enabled = true;
        [AffectsProperty("Status"), AffectsProperty("CanUpdateMod")]
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (value == _enabled) return;
                _enabled = value;

                if (_enabled)
                    InstallMod();
                else
                    UninstallMod();
            }
        }

        public long UnixTimeSeconds { get; set; }

        private bool _UpdatesDisabled = false;
        [AffectsProperty("CanUpdateMod")]
        public bool UpdatesDisabled
        {
            get { return _UpdatesDisabled; }
            set
            {
                _UpdatesDisabled = value;
                if (_UpdatesDisabled)
                    log.InfoFormat("Updates for mod '{0}' are now disabled", Name);
                else
                    log.InfoFormat("Updates for mod '{0}' are now enabled", Name);
            }
        }

        [AffectsProperty("CurrentDownload")]
        public string CurrentFileId { get; set; }

        public string PackName { get; set; }
        #endregion

        #region Derived Properties
        [JsonIgnore]
        public DateTime? LastUpdated
        {
            get
            {
                DateTime? updated = null;
                long seconds = UnixTimeSeconds != 0 ? UnixTimeSeconds : Convert.ToInt64(LatestDownload.Date);
                if (seconds != 0) updated = UnixTimeSecondsToDateTimeConverter.GetLocalDateTime(seconds);
                return updated;
            }
        }

        [JsonIgnore]
        public bool CanUpdateMod
        {
            get { return !UpdatesDisabled && !string.IsNullOrEmpty(Id) && Downloads?.Count > 0 && (Status == Strings.TroveMod_Status_NewVersionAvailable || HasErrorStatus); }
        }

        [JsonIgnore]
        public bool HasErrorStatus
        {
            get { return string.IsNullOrEmpty(Status) ? false : Status.StartsWith(string.Format(Strings.TroveMod_Status_Error, string.Empty)); }
        }

        [JsonIgnore]
        public Download LatestDownload
        {
            get
            {
                try
                {
                    string fileId = Downloads?.Max(m => Convert.ToInt32(m.FileId)).ToString();
                    Download latest = Downloads?.FirstOrDefault(m => m.FileId == fileId);
                    return latest;
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Error getting latest download for mod: {0}", Name), ex);
                    return null;
                }
            }
        }

        [JsonIgnore]
        public Download CurrentDownload
        {
            get
            {
                try
                {
                    Download current = Downloads?.FirstOrDefault(m => m.FileId == CurrentFileId);
                    return current;
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Error getting current download for mod: {0}", Name), ex);
                    return null;
                }
            }
        }
        #endregion

        #region Individual Mod Action Methods
        /// <summary>
        /// Add mod to Trove Tools mod folder
        /// </summary>
        [AffectsProperty("Status"), AffectsProperty("FilePath"), AffectsProperty("CanUpdateMod")]
        public void AddMod()
        {
            try
            {
                log.InfoFormat("Adding mod: {0}", Name);
                Status = Strings.TroveMod_Status_Installing;

                // Copy new mod zip file to TroveTools.NET mods folder
                string newPath = Path.Combine(SettingsDataProvider.ModsFolder, Path.GetFileName(FilePath));
                if (!newPath.Equals(FilePath, StringComparison.OrdinalIgnoreCase))
                {
                    File.Copy(FilePath, newPath, true);
                    FilePath = newPath;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error adding mod: " + Name, ex);
                Status = string.Format(Strings.TroveMod_Status_Error, ex.Message);
            }
        }

        /// <summary>
        /// Uninstalls and removes mod from TroveTools.NET mods folder
        /// </summary>
        [AffectsProperty("Status"), AffectsProperty("CanUpdateMod"), AffectsProperty("Enabled")]
        public void RemoveMod()
        {
            if (string.IsNullOrEmpty(FilePath)) return;
            try
            {
                log.InfoFormat("Removing mod: {0}", Name);
                Enabled = false; // setting enabled to false also uninstalls the mod

                if (!HasErrorStatus)
                {
                    // Remove mod from download folder
                    if (FilePath.StartsWith(SettingsDataProvider.ModsFolder, StringComparison.OrdinalIgnoreCase))
                    {
                        log.InfoFormat("Removing mod zip file: {0}", FilePath);
                        File.Delete(FilePath);
                    }
                    else
                        log.WarnFormat("File [{0}] is not in mods folder [{1}]", FilePath, SettingsDataProvider.ModsFolder);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error removing mod: " + Name, ex);
                Status = string.Format(Strings.TroveMod_Status_Error, ex.Message);
            }
        }

        /// <summary>
        /// Installs mod to Trove folder
        /// </summary>
        [AffectsProperty("Status"), AffectsProperty("CanUpdateMod")]
        public void InstallMod()
        {
            if (string.IsNullOrEmpty(FilePath)) return;
            try
            {
                log.InfoFormat("Installing mod: {0}", Name);
                Status = Strings.TroveMod_Status_Installing;

                // Install mod to each enabled location
                foreach (TroveLocation loc in TroveLocation.Locations.Where(l => l.Enabled))
                {
                    log.DebugFormat("Installing in path: [{0}], name: [{1}]", loc.LocationPath, loc.LocationName);
                    using (FileStream file = File.OpenRead(FilePath))
                    {
                        using (ZipArchive zip = new ZipArchive(file, ZipArchiveMode.Read))
                        {
                            foreach (ZipArchiveEntry entry in zip.Entries)
                            {
                                // Skip over folder entries
                                if (string.IsNullOrEmpty(entry.Name)) continue;

                                string extractPath = GetZipEntryExtractPath(loc, entry);
                                if (extractPath != null)
                                {
                                    if (log.IsInfoEnabled && File.Exists(extractPath)) log.InfoFormat("Overwriting existing file: {0}", extractPath);

                                    // Extract entry from zip file
                                    using (var outputStream = File.Create(extractPath))
                                    {
                                        using (var inputStream = entry.Open())
                                        {
                                            inputStream.CopyTo(outputStream);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                CheckForUpdates();
            }
            catch (Exception ex)
            {
                log.Error("Error installing mod: " + Name, ex);
                Status = string.Format(Strings.TroveMod_Status_Error, ex.Message);
            }
        }

        /// <summary>
        /// Uninstalls mod from Trove folder
        /// </summary>
        [AffectsProperty("Status"), AffectsProperty("CanUpdateMod")]
        public void UninstallMod(bool previousVersion = false)
        {
            if (string.IsNullOrEmpty(FilePath) || !File.Exists(FilePath)) return;
            try
            {
                if (previousVersion)
                    log.InfoFormat("Uninstalling previous version of mod: {0}{1}", Name, CurrentDownload != null ? string.Format(" (v {0})", CurrentDownload.Version) : null);
                else
                    log.InfoFormat("Uninstalling mod: {0}", Name);

                // Uninstall mod from each enabled location
                foreach (TroveLocation loc in TroveLocation.Locations.Where(l => l.Enabled))
                {
                    log.DebugFormat("Uninstalling from path: [{0}], name: [{1}]", loc.LocationPath, loc.LocationName);
                    using (FileStream file = File.OpenRead(FilePath))
                    {
                        using (ZipArchive zip = new ZipArchive(file, ZipArchiveMode.Read))
                        {
                            foreach (ZipArchiveEntry entry in zip.Entries)
                            {
                                // Skip over folder entries
                                if (string.IsNullOrEmpty(entry.Name)) continue;

                                string entryFilePath = GetZipEntryExtractPath(loc, entry);
                                if (File.Exists(entryFilePath))
                                {
                                    FileInfo info = new FileInfo(entryFilePath);
                                    if (info.Length == entry.Length)
                                    {
                                        log.DebugFormat("Removing file: {0}", entryFilePath);
                                        File.Delete(entryFilePath);
                                    }
                                    else
                                        log.WarnFormat("File size of {0} [{1:N}] does not match size in mod zip file [{2:N}], skipping file", entryFilePath, info.Length, entry.Length);
                                }
                                else
                                    log.WarnFormat("File {0} does not exist, skipping file", entryFilePath);
                            }
                        }
                    }
                }

                CheckForUpdates();
            }
            catch (Exception ex)
            {
                log.Error("Error uninstalling mod: " + Name, ex);
                Status = string.Format(Strings.TroveMod_Status_Error, ex.Message);
            }
        }

        /// <summary>
        /// Checks Trovesaurus for updates
        /// </summary>
        [AffectsProperty("Status"), AffectsProperty("CanUpdateMod")]
        public void CheckForUpdates()
        {
            try
            {
                log.DebugFormat("Checking for updates for mod: {0}", Name);
                bool updatesAvailable = false;

                // Verify mod zip file exists
                if (!File.Exists(FilePath))
                {
                    log.ErrorFormat("File [{0}] not found for mod {1}", FilePath, Name);
                    Status = string.Format(Strings.TroveMod_Status_Error, "File not found");
                    Enabled = false;
                    return;
                }

                // Check for updates
                TroveMod mod = FindTrovesaurusMod();
                if (mod != null)
                {
                    if (Convert.ToInt64(mod.LatestDownload.Date) > UnixTimeSeconds) updatesAvailable = true;
                    UpdatePropertiesFromTrovesaurus(mod);
                }

                if (updatesAvailable)
                    Status = Strings.TroveMod_Status_NewVersionAvailable;
                else
                    Status = Strings.TroveMod_Status_UpToDate;
            }
            catch (Exception ex)
            {
                log.Error("Error checking for updates for mod: " + Name, ex);
                Status = string.Format(Strings.TroveMod_Status_Error, ex.Message);
            }
        }

        /// <summary>
        /// Downloads update for mod and installs mod
        /// </summary>
        [AffectsProperty("Status"), AffectsProperty("CanUpdateMod")]
        public void UpdateMod()
        {
            try
            {
                // Uninstall existing mod prior to installing new version
                string oldFile = FilePath;
                if (Enabled && !string.IsNullOrEmpty(oldFile) && File.Exists(oldFile)) UninstallMod(true);

                log.InfoFormat("Downloading mod: {0}", Name);
                Status = Strings.TroveMod_Status_Downloading;
                FilePath = TrovesaurusApi.DownloadMod(this);

                if (!string.IsNullOrEmpty(oldFile) && FilePath != oldFile && File.Exists(oldFile))
                {
                    try { File.Delete(oldFile); }
                    catch (Exception ex) { log.Warn("Error removing previous file: " + oldFile, ex); }
                }

                if (Enabled)
                    InstallMod();
                else
                    Status = Strings.TroveMod_Status_UpToDate;
            }
            catch (Exception ex)
            {
                log.Error("Error downloading mod: " + Name, ex);
                Status = string.Format(Strings.TroveMod_Status_Error, ex.Message);
            }
        }

        /// <summary>
        /// Downloads update for mod and installs mod
        /// </summary>
        [AffectsProperty("Status"), AffectsProperty("CanUpdateMod")]
        public void UpdateMod(string fileId)
        {
            try
            {
                // Uninstall existing mod prior to installing new version
                string oldFile = FilePath;
                if (Enabled && !string.IsNullOrEmpty(oldFile) && File.Exists(oldFile)) UninstallMod(true);

                log.InfoFormat("Downloading mod: {0} with download file ID {1}", Name, fileId);
                Status = Strings.TroveMod_Status_Downloading;
                FilePath = TrovesaurusApi.DownloadMod(this, fileId);

                if (!string.IsNullOrEmpty(oldFile) && FilePath != oldFile && File.Exists(oldFile))
                {
                    try { File.Delete(oldFile); }
                    catch (Exception ex) { log.Warn("Error removing previous file: " + oldFile, ex); }
                }

                if (Enabled)
                    InstallMod();
                else
                    CheckForUpdates();
            }
            catch (Exception ex)
            {
                log.Error("Error downloading mod: " + Name, ex);
                Status = string.Format(Strings.TroveMod_Status_Error, ex.Message);
            }
        }

        /// <summary>
        /// Updates the current mod's properties from the provided mod object from Trovesaurus API
        /// </summary>
        /// <param name="mod"></param>
        [AffectsProperty("Id"), AffectsProperty("Name"), AffectsProperty("Author"), AffectsProperty("Type"), AffectsProperty("SubType"), AffectsProperty("Description"),
            AffectsProperty("DateCreated"), AffectsProperty("TrovesaurusStatus"), AffectsProperty("Replaces"), AffectsProperty("TotalDownloads"), AffectsProperty("Votes"),
            AffectsProperty("Views"), AffectsProperty("Downloads"), AffectsProperty("ImagePath"), AffectsProperty("CurrentFileId")]
        public void UpdatePropertiesFromTrovesaurus(TroveMod mod)
        {
            if (mod != null)
            {
                Id = mod.Id;
                Name = mod.Name;
                Author = mod.Author;
                Type = mod.Type;
                SubType = mod.SubType;
                Description = mod.Description;
                DateCreated = mod.DateCreated;
                TrovesaurusStatus = mod.TrovesaurusStatus;
                Replaces = mod.Replaces;
                TotalDownloads = mod.TotalDownloads;
                Votes = mod.Votes;
                Views = mod.Views;
                Downloads = new List<Download>(mod.Downloads);
                ImagePath = mod.ImagePath;

                // Get the current file ID using the file date if it is not already set
                if (string.IsNullOrEmpty(CurrentFileId))
                {
                    var download = Downloads.FirstOrDefault(d => d.Date == UnixTimeSeconds.ToString());
                    if (download != null) CurrentFileId = download.FileId;
                }
            }
        }

        /// <summary>
        /// Copies the mod installation URI to the clipboard
        /// </summary>
        public void CopyModUri()
        {
            if (string.IsNullOrEmpty(Id))
            {
                log.WarnFormat("Unable to copy mod URI for {0}: mod ID not found", Name, Id);
                return;
            }
            string uri = string.Format(TroveUriFormat, Id, string.IsNullOrEmpty(CurrentFileId) ? LatestDownload.FileId : CurrentFileId);
            Clipboard.SetText(uri);
            log.InfoFormat("Copied mod installation URI for {0} to clipboard: {1}", Name, uri);
        }

        private TroveMod FindTrovesaurusMod()
        {
            return TrovesaurusApi.GetMod(Id, Name);
        }

        #endregion

        #region Public Static Methods and Properties
        public static void RemoveModFolders()
        {
            try
            {
                log.Info("Removing all Trove override folders");
                int count = 0;
                foreach (TroveLocation loc in TroveLocation.Locations.Where(l => l.Enabled))
                {
                    foreach (string folder in Directory.GetDirectories(loc.LocationPath, "*", SearchOption.AllDirectories))
                    {
                        if (Path.GetFileName(folder).Equals(OverrideFolder, StringComparison.OrdinalIgnoreCase))
                        {
                            Directory.Delete(folder, true);
                            count++;
                        }
                    }
                }
                log.InfoFormat("Removed {0} override folder{1}", count, count == 1 ? "" : "s");
            }
            catch (Exception ex)
            {
                log.Error("Error removing Trove override folders", ex);
            }
        }

        public static List<TroveMod> MyMods
        {
            get
            {
                if (_myMods == null)
                {
                    _myMods = SettingsDataProvider.MyMods;

                    // Attempt to auto-detect mods if none were loaded
                    if (_myMods.Count == 0) DetectMyMods(_myMods);
                }
                return _myMods;
            }
        }

        public static TroveMod GetMod(string id)
        {
            var mod = MyMods.FirstOrDefault(m => m.Id == id);
            if (mod != null) return mod;

            mod = TrovesaurusApi.GetMod(id);
            return mod;
        }

        public static void SaveMyMods(List<TroveMod> myMods)
        {
            _myMods = myMods;
            SettingsDataProvider.MyMods = _myMods;
        }

        public static string FilterModFilename(string name)
        {
            return Regex.Replace(name, @"[+.\\/:*?""<>|]", "");
        }
        #endregion

        #region Private Static Helper Methods
        /// <summary>
        /// Builds and returns the extract path for the given Trove location and zip entry
        /// </summary>
        private static string GetZipEntryExtractPath(TroveLocation loc, ZipArchiveEntry entry)
        {
            // Build extract folder path
            string folder = Path.Combine(loc.LocationPath, Path.GetDirectoryName(entry.FullName));

            // Add override folder at deepest folder level if not already included in zip file
            if (!folder.EndsWith(OverrideFolder, StringComparison.OrdinalIgnoreCase)) folder = Path.Combine(folder, OverrideFolder);

            // Check for valid installation location
            if (!File.Exists(Path.Combine(Path.GetDirectoryName(folder), IndexFile)))
            {
                log.ErrorFormat("Incorrectly packaged mod: {0} is not an overridable folder (zip entry path: {1})", Path.GetDirectoryName(folder), entry.FullName);
                return null;
            }

            // Resolve folder path and combine with zip entry filename
            return Path.Combine(SettingsDataProvider.ResolveFolder(folder), entry.Name);
        }

        private static void DetectMyMods(List<TroveMod> myMods)
        {
            try
            {
                log.Info("Detecting My Mods");
                string modsFolder = SettingsDataProvider.ModsFolder;
                List<string> zipFiles = new List<string>(Directory.GetFiles(modsFolder, FileTypeSearchPattern));

                string toolboxMods = SettingsDataProvider.TroveToolboxModsFolder;
                if (toolboxMods != null) zipFiles.AddRange(Directory.GetFiles(toolboxMods, FileTypeSearchPattern));

                foreach (string zipFile in zipFiles)
                {
                    // Add the mod if there are no other mods with this file name already
                    if (!myMods.Any(m => Path.GetFileName(m.FilePath).Equals(Path.GetFileName(zipFile), StringComparison.OrdinalIgnoreCase)))
                    {
                        TroveMod mod = new TroveMod(zipFile);
                        mod.AddMod();
                        mod.InstallMod();
                        myMods.Add(mod);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Warn("Error detecting my mods", ex);
            }
        }
        #endregion
    }

    [Serializable]
    class TroveModException : Exception
    {
        public TroveModException() { }
        public TroveModException(string message) : base(message) { }
        public TroveModException(string message, Exception innerException) : base(message, innerException) { }
        protected TroveModException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
