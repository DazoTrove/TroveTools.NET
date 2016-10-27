using Humanizer;
using log4net;
using Newtonsoft.Json;
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

namespace TroveTools.NET.ViewModel
{
    class ModderToolsViewModel : ViewModelBase
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static object _lockModFiles = new object(), _lockModTags = new object(), _lockExtractableFolders = new object();

        private DelegateCommand _BuildTmodCommand, _ClearCommand, _RefreshArchivesCommand, _ExtractAllCommand, _OpenExtractFolderCommand, _ExtractTmodCommand;
        private DelegateCommand<string> _AddFileCommand, _UpdatePreviewCommand, _LoadYamlCommand, _SaveYamlCommand, _OpenPathCommand;
        private DelegateCommand<IList> _RemoveFilesCommand, _ExtractSelectedCommand, _ListSelectedContentsCommand;
        private CollectionViewSource _ModFilesView = new CollectionViewSource(), _ModTagsView = new CollectionViewSource(), _ExtractableFoldersView = new CollectionViewSource();

        public enum ExtractMethod { TroveTools, TroveDevTool };

        public ModderToolsViewModel()
        {
            // Enable Collection Synchronization on all ObservableCollection objects
            BindingOperations.EnableCollectionSynchronization(ModFiles, _lockModFiles);
            BindingOperations.EnableCollectionSynchronization(ModTags, _lockModTags);
            BindingOperations.EnableCollectionSynchronization(ExtractableFolders, _lockExtractableFolders);

            DisplayName = Strings.ModderToolsViewModel_DisplayName;
            _ModFilesView.Source = ModFiles;
            _ModTagsView.Source = ModTags;
            _ExtractableFoldersView.Source = ExtractableFolders;

            // Load mod tags
            foreach (var tag in JsonConvert.DeserializeObject<List<ModTagViewModel>>(Resources.ModTags))
            {
                ModTags.Add(tag);
            }

            _ModTagsView.IsLiveGroupingRequested = true;
            ModTagsView.GroupDescriptions.Add(new PropertyGroupDescription("Category"));

            ModTagsView.SortDescriptions.Clear();
            ModTagsView.SortDescriptions.Add(new SortDescription("Category", ListSortDirection.Descending));
            ModTagsView.SortDescriptions.Add(new SortDescription("Title", ListSortDirection.Ascending));
        }

        #region Public Methods
        public void LoadData()
        {
            ExtractedPath = Path.Combine(PrimaryLocationPath, "extracted");
            LoadArchiveFolders();

            TModExractFolder = SettingsDataProvider.ModsFolder;
        }
        #endregion

        #region Mod Tag Class
        public class ModTagViewModel : ViewModelBase
        {
            private string _Category = string.Empty;
            public string Category
            {
                get { return _Category; }
                set
                {
                    _Category = value;
                    RaisePropertyChanged("Category");
                }
            }

            private string _Title = string.Empty;
            public string Title
            {
                get { return _Title; }
                set
                {
                    _Title = value;
                    RaisePropertyChanged("Title");
                }
            }

            private bool _Selected = false;
            public bool Selected
            {
                get { return _Selected; }
                set
                {
                    _Selected = value;
                    RaisePropertyChanged("Selected");
                }
            }

            private string _AltTitle = string.Empty;
            public string AltTitle
            {
                get { return _AltTitle; }
                set
                {
                    _AltTitle = value;
                    RaisePropertyChanged("AltTitle");
                }
            }
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

        public ObservableCollection<ModTagViewModel> ModTags { get; } = new ObservableCollection<ModTagViewModel>();

        public ICollectionView ModTagsView
        {
            get { return _ModTagsView.View; }
        }

        private TroveModViewModel _CurrentMod = null;
        public TroveModViewModel CurrentMod
        {
            get { return _CurrentMod; }
            set
            {
                if (value != null)
                {
                    Clear();
                    CanUpdateCurrentMod = UpdateCurrentMod = true;
                    ModTitle = value.DisplayName;
                    ModAuthor = value.DataObject.Author;
                    ModNotes = value.DataObject.CleanNotes;
                    string previewPath = value.DataObject.DownloadImage(PreviewLocation);
                    if (previewPath != null) UpdatePreview(previewPath);

                    foreach (var file in value.DataObject.ModFiles)
                    {
                        ModFiles.Add(file);
                    }

                    var ic = StringComparison.OrdinalIgnoreCase;
                    foreach (var tag in ModTags)
                    {
                        string title = tag.Title ?? string.Empty;
                        string altTitle = tag.AltTitle ?? string.Empty;

                        if (!string.IsNullOrWhiteSpace(title) && (title.Equals(value.DataObject.Type, ic) || title.Equals(value.DataObject.SubType, ic)))
                            tag.Selected = true;

                        if (!string.IsNullOrWhiteSpace(altTitle) && (altTitle.Equals(value.DataObject.Type, ic) || altTitle.Equals(value.DataObject.SubType, ic)))
                            tag.Selected = true;
                    }
                }
                else
                {
                    CanUpdateCurrentMod = UpdateCurrentMod = false;
                }
                _CurrentMod = value;
                RaisePropertyChanged("CurrentMod");
            }
        }

        private bool _CanUpdateCurrentMod = false;
        public bool CanUpdateCurrentMod
        {
            get { return _CanUpdateCurrentMod; }
            set
            {
                _CanUpdateCurrentMod = value;
                RaisePropertyChanged("CanUpdateCurrentMod");
            }
        }

        private bool _UpdateCurrentMod = false;
        public bool UpdateCurrentMod
        {
            get { return _UpdateCurrentMod; }
            set
            {
                _UpdateCurrentMod = value;
                RaisePropertyChanged("UpdateCurrentMod");
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

        private string _AdditionalTags = string.Empty;
        public string AdditionalTags
        {
            get { return _AdditionalTags; }
            set
            {
                _AdditionalTags = value;
                RaisePropertyChanged("AdditionalTags");
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

        private string _TmodFile = string.Empty;
        public string TmodFile
        {
            get { return _TmodFile; }
            set
            {
                _TmodFile = value;
                RaisePropertyChanged("TmodFile");
            }
        }

        private string _TModExractFolder = string.Empty;
        public string TModExractFolder
        {
            get { return _TModExractFolder; }
            set
            {
                _TModExractFolder = value;
                RaisePropertyChanged("TModExractFolder");
                RaisePropertyChanged("TModExractFolderResolved");
            }
        }

        private ExtractMethod _ExtractTModMethod = ExtractMethod.TroveDevTool;
        public ExtractMethod ExtractTModMethod
        {
            get { return _ExtractTModMethod; }
            set
            {
                _ExtractTModMethod = value;
                RaisePropertyChanged("ExtractTModMethod");
            }
        }

        private bool _TModCreateSubfolder = true;
        public bool TModCreateSubfolder
        {
            get { return _TModCreateSubfolder; }
            set
            {
                _TModCreateSubfolder = value;
                RaisePropertyChanged("TModCreateSubfolder");
                RaisePropertyChanged("TModExractFolderResolved");
            }
        }

        private bool _TModCreateOverrideFolders = true;
        public bool TModCreateOverrideFolders
        {
            get { return _TModCreateOverrideFolders; }
            set
            {
                _TModCreateOverrideFolders = value;
                RaisePropertyChanged("TModCreateOverrideFolders");
            }
        }

        private bool _TModCreateYamlFile = true;
        public bool TModCreateYamlFile
        {
            get { return _TModCreateYamlFile; }
            set
            {
                _TModCreateYamlFile = value;
                RaisePropertyChanged("TModCreateYamlFile");
            }
        }

        public string TModExractFolderResolved
        {
            get { return SettingsDataProvider.ResolveFolder(TModCreateSubfolder ? Path.Combine(TModExractFolder, Path.GetFileNameWithoutExtension(TmodFile)) : TModExractFolder); }
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

        public DelegateCommand<string> OpenPathCommand
        {
            get
            {
                if (_OpenPathCommand == null) _OpenPathCommand = new DelegateCommand<string>(OpenPath);
                return _OpenPathCommand;
            }
        }

        public DelegateCommand RefreshArchivesCommand
        {
            get
            {
                if (_RefreshArchivesCommand == null) _RefreshArchivesCommand = new DelegateCommand(LoadArchiveFolders);
                return _RefreshArchivesCommand;
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

        public DelegateCommand OpenExtractFolderCommand
        {
            get
            {
                if (_OpenExtractFolderCommand == null) _OpenExtractFolderCommand = new DelegateCommand(o => OpenPath(TModExractFolderResolved));
                return _OpenExtractFolderCommand;
            }
        }

        public DelegateCommand ExtractTmodCommand
        {
            get
            {
                if (_ExtractTmodCommand == null) _ExtractTmodCommand = new DelegateCommand(ExtractTmod);
                return _ExtractTmodCommand;
            }
        }
        #endregion

        #region Private methods
        private void LoadYaml(string yamlPath)
        {
            try
            {
                log.InfoFormat("Loading YAML file: {0}", yamlPath);

                string yamlContents = File.ReadAllText(yamlPath);
                var details = ModDetails.LoadFromYaml(yamlContents);

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

                foreach (string tag in details.Tags)
                {
                    var modTag = ModTags.FirstOrDefault(t => t.Title.Equals(tag, StringComparison.OrdinalIgnoreCase));
                    if (modTag != null)
                        modTag.Selected = true;
                    else
                        AdditionalTags = string.IsNullOrWhiteSpace(AdditionalTags) ? tag : string.Format("{0}, {1}", AdditionalTags, tag);
                }
            }
            catch (Exception ex) { log.Error(string.Format("Error loading YAML file {0}", yamlPath), ex); }
        }

        private void SaveYaml(string yamlPath)
        {
            try
            {
                log.InfoFormat("Saving YAML file: {0}", yamlPath);

                var tags = new List<string>();
                foreach (var tag in ModTags) if (tag.Selected) tags.AddIfMissing(tag.Title);
                if (!string.IsNullOrWhiteSpace(AdditionalTags))
                    foreach (var tag in AdditionalTags.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) tags.AddIfMissing(tag.Trim());

                ModDetails details = new ModDetails() { Author = ModAuthor, Title = ModTitle, Notes = ModNotes, PreviewPath = ModPreview, Files = ModFiles.ToList(), Tags = tags };
                details.SaveYamlFile(yamlPath);
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
                CurrentMod = null;
                ModFiles.Clear();
                ModTitle = ModAuthor = ModNotes = ModPreview = PreviewImage = AdditionalTags = DevToolOutput = null;
                foreach (var tag in ModTags) tag.Selected = false;
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
                        if (CurrentMod != null && UpdateCurrentMod)
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

        private void OpenPath(string path = null)
        {
            try
            {
                log.InfoFormat("Opening folder {0}", path);
                Process.Start("explorer.exe", SettingsDataProvider.ResolveFolder(path));
            }
            catch (Exception ex) { log.Error(string.Format("Error opening folder: {0}", path), ex); }
        }

        private void LoadArchiveFolders(object obj = null)
        {
            ExtractableFolders.Clear();
            foreach (var folder in TroveMod.ExtractableFolders) ExtractableFolders.Add(folder);
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

        private void ExtractTmod(object obj = null)
        {
            try
            {
                ProgressVisible = true;
                log.InfoFormat("Extracting TMod using {0}: {1}", ExtractTModMethod.Humanize(LetterCasing.Title), TmodFile);
                if (ExtractTModMethod == ExtractMethod.TroveTools)
                    TModFormat.ExtractTmod(TmodFile, TModExractFolderResolved, TModCreateOverrideFolders, TModCreateYamlFile, p => ProgressValue = p);
                else
                {
                    // Run Trove Extract Mod command (Trove.exe -tool extractmod -file "<file>" -override -output "<dir>" -meta "<file.yaml>") and show results
                    TroveLocation.PrimaryLocation.RunDevTool(string.Format("-tool extractmod -file \"{0}\"{1} -output \"{2}\"{3}", TmodFile,
                        TModCreateOverrideFolders ? " -override" : "", TModExractFolderResolved,
                        TModCreateYamlFile ? string.Format(" -meta \"{0}.yaml\"", Path.Combine(TModExractFolderResolved, Path.GetFileNameWithoutExtension(TmodFile))) : ""),
                        output => DevToolOutput = output);
                }
            }
            catch (Exception ex) { log.Error(string.Format("Error extracting TMod file: {0} to {1}", TmodFile, TModExractFolder), ex); }
            finally { ProgressVisible = false; }
        }
        #endregion
    }
}
