using CommonMark;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TroveTools.NET.DataAccess;
using TroveTools.NET.Framework;
using TroveTools.NET.Model;
using TroveTools.NET.Properties;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.ObjectFactories;

namespace TroveTools.NET.ViewModel
{
    class ModderToolsViewModel : ViewModelBase
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private DelegateCommand _BuildTmodCommand, _ClearCommand, _OpenExtractedPathCommand, _ExtractAllCommand;
        private DelegateCommand<string> _AddFileCommand, _UpdatePreviewCommand, _LoadYamlCommand, _SaveYamlCommand;
        private DelegateCommand<IList> _RemoveFilesCommand, _ExtractSelectedCommand, _ListSelectedContentsCommand;
        private CollectionViewSource _ModFilesView = new CollectionViewSource(), _ExtractableFoldersView = new CollectionViewSource();

        public ModderToolsViewModel()
        {
            DisplayName = Strings.ModderToolsViewModel_DisplayName;
            _ModFilesView.Source = ModFiles;
            _ExtractableFoldersView.Source = ExtractableFolders;
        }

        #region Public Methods
        public void LoadData()
        {
            ExtractedPath = Path.Combine(PrimaryLocationPath, "extracted");
            foreach (var folder in TroveMod.ExtractableFolders) ExtractableFolders.Add(folder);
        }
        #endregion

        #region Public Properties
        private int _TabSelectedIndex = 0;
        public int TabSelectedIndex
        {
            get { return _TabSelectedIndex; }
            set
            {
                _TabSelectedIndex = value;
                RaisePropertyChanged("TabSelectedIndex");
            }
        }

        public ObservableCollection<string> ModFiles { get; } = new ObservableCollection<string>();

        public ICollectionView ModFilesView
        {
            get { return _ModFilesView.View; }
        }

        private TroveModViewModel _CurrentMod = null;
        public TroveModViewModel CurrentMod
        {
            get { return _CurrentMod; }
            set
            {
                ModFiles.Clear();
                if (value != null)
                {
                    _CurrentMod = value;
                    ModTitle = value.DisplayName;
                    ModAuthor = value.DataObject.Author;
                    ModNotes = value.DataObject.CleanNotes;
                    string previewPath = value.DataObject.DownloadImage(PreviewLocation);
                    if (previewPath != null) UpdatePreview(previewPath);

                    foreach (var file in value.DataObject.ModFiles)
                    {
                        ModFiles.Add(file);
                    }
                }
                RaisePropertyChanged("CurrentMod");
            }
        }

        private string _ModTitle = string.Empty;
        public string ModTitle
        {
            get { return _ModTitle; }
            set
            {
                _ModTitle = value;
                RaisePropertyChanged("ModTitle");
            }
        }

        private string _ModPreview = string.Empty;
        public string ModPreview
        {
            get { return _ModPreview; }
            set
            {
                _ModPreview = value;
                RaisePropertyChanged("ModPreview");
            }
        }

        private string _ModAuthor = string.Empty;
        public string ModAuthor
        {
            get { return _ModAuthor; }
            set
            {
                _ModAuthor = value;
                RaisePropertyChanged("ModAuthor");
            }
        }

        private string _ModNotes = string.Empty;
        public string ModNotes
        {
            get { return _ModNotes; }
            set
            {
                _ModNotes = value;
                RaisePropertyChanged("ModNotes");
            }
        }

        private string _PreviewImage = string.Empty;
        public string PreviewImage
        {
            get { return _PreviewImage; }
            set
            {
                _PreviewImage = value;
                RaisePropertyChanged("PreviewImage");
            }
        }

        private string _DevToolOutput = string.Empty;
        public string DevToolOutput
        {
            get { return _DevToolOutput; }
            set
            {
                _DevToolOutput = value;
                RaisePropertyChanged("DevToolOutput");
            }
        }

        public string YamlPath
        {
            get { return Path.Combine(SettingsDataProvider.ModsFolder, SettingsDataProvider.GetSafeFilename(string.Format("{0}.yaml", ModTitle))); }
        }

        public string PrimaryLocationPath
        {
            get { return TroveLocation.PrimaryLocation.LocationPath; }
        }

        public string PreviewLocation
        {
            get { return SettingsDataProvider.ResolveFolder(Path.Combine(TroveLocation.PrimaryLocation.LocationPath, "ui", TroveMod.OverrideFolder)); }
        }

        public string ModsFolder
        {
            get { return SettingsDataProvider.ModsFolder; }
        }

        public ObservableCollection<string> ExtractableFolders { get; } = new ObservableCollection<string>();

        public ICollectionView ExtractableFoldersView
        {
            get { return _ExtractableFoldersView.View; }
        }

        private string _ExtractedPath = string.Empty;
        public string ExtractedPath
        {
            get { return _ExtractedPath; }
            set
            {
                _ExtractedPath = value;
                RaisePropertyChanged("ExtractedPath");
            }
        }

        private bool _ProgressVisible = false;
        public bool ProgressVisible
        {
            get { return _ProgressVisible; }
            set
            {
                _ProgressVisible = value;
                RaisePropertyChanged("ProgressVisible");
            }
        }

        private double _ProgressValue = 0;
        public double ProgressValue
        {
            get { return _ProgressValue; }
            set
            {
                _ProgressValue = value;
                RaisePropertyChanged("ProgressValue");
            }
        }
        #endregion

        #region Commands
        public DelegateCommand<string> LoadYamlCommand
        {
            get
            {
                if (_LoadYamlCommand == null) _LoadYamlCommand = new DelegateCommand<string>(LoadYaml);
                return _LoadYamlCommand;
            }
        }

        public DelegateCommand<string> SaveYamlCommand
        {
            get
            {
                if (_SaveYamlCommand == null) _SaveYamlCommand = new DelegateCommand<string>(SaveYaml);
                return _SaveYamlCommand;
            }
        }

        public DelegateCommand<string> UpdatePreviewCommand
        {
            get
            {
                if (_UpdatePreviewCommand == null) _UpdatePreviewCommand = new DelegateCommand<string>(UpdatePreview);
                return _UpdatePreviewCommand;
            }
        }

        public DelegateCommand<string> AddFileCommand
        {
            get
            {
                if (_AddFileCommand == null) _AddFileCommand = new DelegateCommand<string>(AddFile);
                return _AddFileCommand;
            }
        }

        public DelegateCommand<IList> RemoveFilesCommand
        {
            get
            {
                if (_RemoveFilesCommand == null) _RemoveFilesCommand = new DelegateCommand<IList>(items => RemoveFiles(items), items => items != null && items.Count > 0);
                return _RemoveFilesCommand;
            }
        }

        public DelegateCommand BuildTmodCommand
        {
            get
            {
                if (_BuildTmodCommand == null) _BuildTmodCommand = new DelegateCommand(BuildTmod);
                return _BuildTmodCommand;
            }
        }

        public DelegateCommand ClearCommand
        {
            get
            {
                if (_ClearCommand == null) _ClearCommand = new DelegateCommand(Clear);
                return _ClearCommand;
            }
        }

        public DelegateCommand OpenExtractedPathCommand
        {
            get
            {
                if (_OpenExtractedPathCommand == null) _OpenExtractedPathCommand = new DelegateCommand(OpenExtractedPath);
                return _OpenExtractedPathCommand;
            }
        }

        public DelegateCommand ExtractAllCommand
        {
            get
            {
                if (_ExtractAllCommand == null) _ExtractAllCommand = new DelegateCommand(ExtractAll);
                return _ExtractAllCommand;
            }
        }

        public DelegateCommand<IList> ExtractSelectedCommand
        {
            get
            {
                if (_ExtractSelectedCommand == null) _ExtractSelectedCommand = new DelegateCommand<IList>(items => ExtractSelected(items), items => items != null && items.Count > 0);
                return _ExtractSelectedCommand;
            }
        }

        public DelegateCommand<IList> ListSelectedContentsCommand
        {
            get
            {
                if (_ListSelectedContentsCommand == null) _ListSelectedContentsCommand = new DelegateCommand<IList>(items => ListSelectedContents(items), items => items != null && items.Count > 0);
                return _ListSelectedContentsCommand;
            }
        }
        #endregion

        #region YAML details class
        public class ModDetails
        {
            public string Author { get; set; }
            public string Title { get; set; }
            public string Notes { get; set; }
            public string PreviewPath { get; set; }
            public List<string> Files { get; set; }
        }
        #endregion

        #region Private methods
        private void LoadYaml(string yamlPath)
        {
            try
            {
                log.InfoFormat("Loading YAML file: {0}", yamlPath);

                string yamlContents = File.ReadAllText(yamlPath);
                var deserializer = new DeserializerBuilder().WithNamingConvention(new CamelCaseNamingConvention()).Build();
                var details = deserializer.Deserialize<ModDetails>(yamlContents);

                Clear();
                ModAuthor = details.Author;
                ModTitle = details.Title;
                ModNotes = details.Notes;
                ModPreview = details.PreviewPath;
                string preview = TroveMod.GetOverridePath(details.PreviewPath, PrimaryLocationPath);
                if (!string.IsNullOrWhiteSpace(preview) && File.Exists(preview) && (preview.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || preview.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)))
                    PreviewImage = preview;

                foreach (string file in details.Files)
                {
                    ModFiles.Add(file);
                }
            }
            catch (Exception ex) { log.Error(string.Format("Error loading YAML file {0}", yamlPath), ex); }
        }

        private void SaveYaml(string yamlPath)
        {
            try
            {
                log.InfoFormat("Saving YAML file: {0}", yamlPath);

                var serializer = new SerializerBuilder().WithNamingConvention(new CamelCaseNamingConvention()).Build();
                ModDetails details = new ModDetails() { Author = ModAuthor, Title = ModTitle, Notes = ModNotes, PreviewPath = ModPreview, Files = ModFiles.ToList() };
                string yaml = serializer.Serialize(details);

                using (StreamWriter sw = new StreamWriter(yamlPath, false))
                {
                    sw.WriteLine("---");
                    sw.Write(yaml);
                    sw.WriteLine("...");
                }
            }
            catch (Exception ex) { log.Error(string.Format("Error saving YAML file {0}", yamlPath), ex); }
        }

        private void UpdatePreview(string file)
        {
            try
            {
                if (!file.Contains(TroveMod.OverrideFolder))
                {
                    string newFile = Path.Combine(PreviewLocation, Path.GetFileName(file));
                    File.Copy(file, newFile, true);
                    file = newFile;
                }
                if (file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)) PreviewImage = file;
                ModPreview = TroveMod.MakeRelativePath(file, PrimaryLocationPath);
                if (!ModFiles.Contains(ModPreview)) ModFiles.Add(ModPreview);
            }
            catch (Exception ex) { log.Error(string.Format("Error updating preview to {0}", file), ex); }
        }

        private void AddFile(string file)
        {
            try
            {
                string relativePath = TroveMod.MakeRelativePath(file, PrimaryLocationPath);
                if (!ModFiles.Contains(relativePath)) ModFiles.Add(relativePath);
            }
            catch (Exception ex) { log.Error(string.Format("Error adding file {0}", file), ex); }
        }

        private void RemoveFiles(IList items)
        {
            try
            {
                if (items == null) return;
                foreach (string item in items.Cast<string>().ToList()) ModFiles.Remove(item);
            }
            catch (Exception ex) { log.Error("Error removing selected files", ex); }
        }

        private void Clear(object obj = null)
        {
            try
            {
                ModFiles.Clear();
                CurrentMod = null;
                ModTitle = ModAuthor = ModNotes = ModPreview = PreviewImage = DevToolOutput = null;
            }
            catch (Exception ex) { log.Error("Error clearing build mod settings", ex); }
        }

        private void BuildTmod(object obj = null)
        {
            try
            {
                // Create YAML file in app data mods folder
                string yamlPath = YamlPath;
                SaveYaml(yamlPath);

                // Run Trove Build Mod command (Trove.exe -tool buildmod -meta "%AppData%\TroveTools.NET\mods\{0}.yaml") and show results
                TroveLocation.PrimaryLocation.RunDevTool(string.Format("-tool buildmod -meta \"{0}\"", yamlPath), output =>
                {
                    try
                    {
                        // Show output results
                        DevToolOutput = output;

                        // Update current mod with new file
                        if (CurrentMod != null)
                        {
                            string modPath = Path.Combine(TroveLocation.PrimaryLocation.LocationPath, TroveMod.ModsFolder, string.Format("{0}.tmod", ModTitle));
                            if (File.Exists(modPath))
                            {
                                dynamic mod = CurrentMod;
                                mod.UpdateModPath(modPath);
                            }
                            else
                                log.WarnFormat("Unable to find mod file: {0}", modPath);
                        }
                    }
                    catch (Exception ex) { log.Error(string.Format("Error processing TMOD build results for {0}", ModTitle), ex); }
                });
            }
            catch (Exception ex) { log.Error(string.Format("Error building TMOD for {0}", ModTitle), ex); }
        }

        private void OpenExtractedPath(object obj = null)
        {
            try { Process.Start("explorer.exe", SettingsDataProvider.ResolveFolder(ExtractedPath)); }
            catch (Exception ex) { log.Error(string.Format("Error opening extracted path: {0}", ExtractedPath), ex); }
        }

        private void ExtractAll(object obj = null)
        {
            try
            {
                try
                {
                    if (Directory.Exists(ExtractedPath))
                    {
                        log.InfoFormat("Clearing old files in {0}", ExtractedPath);
                        Directory.Delete(ExtractedPath, true);
                    }
                }
                catch { }

                ExtractArchives(ExtractableFolders);
            }
            catch (Exception ex) { log.Error("Error extracting all archives", ex); }
        }

        private void ExtractSelected(IList items)
        {
            try { ExtractArchives(items.Cast<string>()); }
            catch (Exception ex) { log.Error("Error extracting selected archives", ex); }
        }

        private void ExtractArchives(IEnumerable<string> folders)
        {
            string extractFolder = SettingsDataProvider.ResolveFolder(ExtractedPath);
            if (extractFolder.StartsWith(PrimaryLocationPath, StringComparison.OrdinalIgnoreCase))
                extractFolder = extractFolder.Remove(0, PrimaryLocationPath.Length + (PrimaryLocationPath.EndsWith(Path.DirectorySeparatorChar.ToString()) ? 0 : 1));

            RunCommand(folders.ToList(), string.Format("-tool extractarchive \"{{0}}\" \"{0}\\{{0}}\"", extractFolder), 0);
        }

        private void ListSelectedContents(IList items)
        {
            try
            {
                var list = items.Cast<string>().ToList();
                RunCommand(list, "-tool listarchive \"{0}\"", 0);
            }
            catch (Exception ex) { log.Error("Error listing selected archive contents", ex); }
        }

        private void RunCommand(List<string> list, string commandFormat, int i)
        {
            // Show progress of processing all folders
            ProgressValue = (i + 1d) / list.Count * 100d;
            ProgressVisible = true;

            // Run Trove command on each folder and show results
            TroveLocation.PrimaryLocation.RunDevTool(string.Format(commandFormat, list[i]), output =>
            {
                DevToolOutput = output;

                // Recursively run command on remaining items in the list
                if (i < list.Count - 1)
                    RunCommand(list, commandFormat, i + 1);
                else
                {
                    log.InfoFormat("Completed processing {0} folders", list.Count);
                    ProgressVisible = false;
                }
            }, i == 0);
        }
        #endregion
    }
}
