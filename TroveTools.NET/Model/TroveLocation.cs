using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TroveTools.NET.DataAccess;
using TroveTools.NET.Framework;
using TroveTools.NET.Properties;

namespace TroveTools.NET.Model
{
    public class TroveLocation : IDataErrorInfo
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public const string TroveAppDataFolder = "Trove";
        public const string DevToolLogFileName = "DevTool.log";
        public const string TroveExecutableFileName = "Trove.exe";
        private static List<TroveLocation> _locations;

        public TroveLocation(string locationName, string locationPath, bool enabled = true, bool primary = false)
        {
            LocationName = locationName;
            LocationPath = locationPath;
            Enabled = enabled;
            Primary = primary;
        }

        public string LocationName { get; set; }

        public string LocationPath { get; set; }

        public bool Enabled { get; set; } = true;

        public bool Primary { get; set; } = false;

        #region IDataErrorInfo Members   
        /// <summary>
        /// Gets an error message indicating what is wrong with this object.
        /// </summary>
        /// <returns>An error message indicating what is wrong with this object. The default is an empty string ("").</returns>
        [JsonIgnore]
        public string Error
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <param name="columnName">The name of the property whose error message to get.</param>
        /// <returns>The error message for the property. The default is an empty string ("").</returns>
        public string this[string columnName]
        {
            get
            {
                string error = string.Empty;
                switch (columnName)
                {
                    case "LocationName":
                        if (LocationName.IsStringMissing()) error = Strings.TroveLocation_Error_LocationNameMissing;
                        break;

                    case "LocationPath":
                        if (LocationPath.IsStringMissing())
                            error = Strings.TroveLocation_Error_LocationPathMissing;
                        else if (!File.Exists(Path.Combine(LocationPath, TroveExecutableFileName)))
                            error = Strings.TroveLocation_Error_LocationPathInvalid;
                        break;
                }
                return error;
            }
        }
        #endregion

        [JsonIgnore]
        public bool IsValid
        {
            get
            {
                if (this["LocationName"] != null) return false;
                if (this["LocationPath"] != null) return false;
                return true;
            }
        }

        public void RunDevTool(string commandLineArgs, Action<string> devToolOutput, bool renameOldLog = true)
        {
            string devToolLog = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), TroveAppDataFolder, DevToolLogFileName);

            if (renameOldLog)
            {
                // Rename DevTool.log to DevTool.{0:yyyy-MM-dd.HH-mm-ss}.log using file last write time
                FileInfo file = new FileInfo(devToolLog);
                if (file.Exists)
                {
                    int i = 1;
                    string oldLog = Path.Combine(file.DirectoryName, string.Format("{0}.{1:yyyy-MM-dd.HH-mm-ss}{2}", Path.GetFileNameWithoutExtension(file.Name), file.LastWriteTime, file.Extension));
                    
                    // If the file for the old log file already exists, find a filename that doesn't exist by adding a number to the filename
                    while (File.Exists(oldLog))
                    {
                        oldLog = Path.Combine(file.DirectoryName, string.Format("{0}.{1:yyyy-MM-dd.HH-mm-ss}.{2}{3}", Path.GetFileNameWithoutExtension(file.Name), file.LastWriteTime, i, file.Extension));
                    }
                    file.MoveTo(oldLog);
                }
            }

            // Run Trove.exe {command line args}
            string path = Path.Combine(LocationPath, TroveExecutableFileName);
            log.InfoFormat("Launching dev tool: {0} {1}", path, commandLineArgs);
            var process = new Process() { StartInfo = new ProcessStartInfo(path, commandLineArgs) };
            process.StartInfo.WorkingDirectory = LocationPath;
            process.EnableRaisingEvents = true;
            process.Exited += (s, e) =>
            {
                try
                {
                    // Return Results from DevTool.log
                    string output = null;
                    if (File.Exists(devToolLog))
                        output = File.ReadAllText(devToolLog);
                    else
                        output = string.Format("Dev Tool ended with no results in {0}{1}", devToolLog, Environment.NewLine);
                    devToolOutput(output);
                }
                catch (Exception ex)
                {
                    log.Error("Error after Dev Tool exited", ex);
                }
            };
            process.Start();
        }

        public static List<TroveLocation> Locations
        {
            get
            {
                if (_locations == null)
                {
                    _locations = SettingsDataProvider.Locations;

                    // Attempt to auto-detect locations if none were loaded
                    if (_locations.Count == 0) DetectLocations(_locations);

                    // Auto-set first location to primary if none are currently set
                    if (_locations.Count > 0 && !_locations.Any(l => l.Primary)) _locations[0].Primary = true;
                }
                return _locations;
            }
            set
            {
                _locations = value;
                SettingsDataProvider.Locations = _locations;
            }
        }

        public static TroveLocation PrimaryLocation
        {
            get { return Locations.FirstOrDefault(l => l.Primary); }
        }

        public static void DetectLocations(List<TroveLocation> locations)
        {
            try
            {
                log.Info("Detecting Trove Locations");
                var potentialLocs = new Dictionary<string, string>();

                log.Debug("Adding locations from registry");
                RegistrySettings.GetTroveLocations(potentialLocs);

                // Try common program files locations
                string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                if (!string.IsNullOrEmpty(programFilesX86))
                {
                    log.Debug("Adding program files x86 locations");
                    potentialLocs.AddIfMissing(Path.Combine(programFilesX86, @"Glyph\Games\Trove\Live"), "Trove Live (Glyph)");
                    potentialLocs.AddIfMissing(Path.Combine(programFilesX86, @"Glyph\Games\Trove\PTS"), "Trove PTS (Glyph)");
                    potentialLocs.AddIfMissing(Path.Combine(programFilesX86, @"Steam\steamapps\common\Trove\Live"), "Trove Live (Steam)");
                    potentialLocs.AddIfMissing(Path.Combine(programFilesX86, @"Steam\steamapps\common\Trove\Games\Trove\Live"), "Trove Live (Steam)");
                    potentialLocs.AddIfMissing(Path.Combine(programFilesX86, @"Steam\steamapps\common\Trove\PTS"), "Trove PTS (Steam)");
                    potentialLocs.AddIfMissing(Path.Combine(programFilesX86, @"Steam\steamapps\common\Trove\Games\Trove\PTS"), "Trove PTS (Steam)");
                }
                if (!string.IsNullOrEmpty(programFiles) && programFiles != programFilesX86)
                {
                    log.Debug("Adding program files locations");
                    potentialLocs.AddIfMissing(Path.Combine(programFiles, @"Glyph\Games\Trove\Live"), "Trove Live (Glyph)");
                    potentialLocs.AddIfMissing(Path.Combine(programFiles, @"Glyph\Games\Trove\PTS"), "Trove PTS (Glyph)");
                    potentialLocs.AddIfMissing(Path.Combine(programFiles, @"Steam\steamapps\common\Trove\Live"), "Trove Live (Steam)");
                    potentialLocs.AddIfMissing(Path.Combine(programFiles, @"Steam\steamapps\common\Trove\Games\Trove\Live"), "Trove Live (Steam)");
                    potentialLocs.AddIfMissing(Path.Combine(programFiles, @"Steam\steamapps\common\Trove\PTS"), "Trove PTS (Steam)");
                    potentialLocs.AddIfMissing(Path.Combine(programFiles, @"Steam\steamapps\common\Trove\Games\Trove\PTS"), "Trove PTS (Steam)");
                }

                log.InfoFormat("Searching {0} potential locations", potentialLocs.Count);
                foreach (var loc in potentialLocs)
                {
                    if (File.Exists(Path.Combine(loc.Key, TroveExecutableFileName)))
                    {
                        // Add the location if this path does not already exist
                        if (!locations.Any(l => l.LocationPath.ToLower() == loc.Key.ToLower()))
                            locations.Add(new TroveLocation(loc.Value, loc.Key));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Warn("Error detecting locations", ex);
            }
        }
    }
}
