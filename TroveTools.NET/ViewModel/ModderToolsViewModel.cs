using CommonMark;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private DelegateCommand _BuildTmodCommand, _ClearCommand;
        private DelegateCommand<string> _AddFileCommand, _UpdatePreviewCommand;
        private DelegateCommand<IList> _RemoveFilesCommand;
        private CollectionViewSource _ModFilesView = new CollectionViewSource();

        public ModderToolsViewModel()
        {
            DisplayName = Strings.ModderToolsViewModel_DisplayName;
            _ModFilesView.Source = ModFiles;
        }

        #region Public Properties
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

        public string AddFileLocation
        {
            get { return TroveLocation.PrimaryLocation.LocationPath; }
        }

        public string PreviewLocation
        {
            get { return SettingsDataProvider.ResolveFolder(Path.Combine(TroveLocation.PrimaryLocation.LocationPath, "ui", TroveMod.OverrideFolder)); }
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
        #endregion

        #region Commands
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
        #endregion

        #region Private methods
        private void UpdatePreview(string file)
        {
            if (!file.Contains(TroveMod.OverrideFolder))
            {
                string newFile = Path.Combine(PreviewLocation, Path.GetFileName(file));
                File.Copy(file, newFile, true);
                file = newFile;
            }
            if (file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)) PreviewImage = file;
            ModPreview = TroveMod.MakeRelativePath(file, AddFileLocation);
            if (!ModFiles.Contains(ModPreview)) ModFiles.Add(ModPreview);
        }

        private void AddFile(string file)
        {
            string relativePath = TroveMod.MakeRelativePath(file, AddFileLocation);
            if (!ModFiles.Contains(relativePath)) ModFiles.Add(relativePath);
        }

        private void RemoveFiles(IList items)
        {
            if (items == null) return;
            foreach (string item in items.Cast<string>().ToList()) ModFiles.Remove(item);
        }

        private void Clear(object obj = null)
        {
            ModFiles.Clear();
            CurrentMod = null;
            ModTitle = ModAuthor = ModNotes = ModPreview = PreviewImage = DevToolOutput = null;
        }

        private void BuildTmod(object obj = null)
        {
            try
            {
                // Create YAML file in app data mods folder
                string yamlPath = Path.Combine(SettingsDataProvider.ModsFolder, SettingsDataProvider.GetSafeFilename(string.Format("{0}.yaml", ModTitle)));
                using (StreamWriter sw = new StreamWriter(yamlPath, false))
                {
                    sw.WriteLine("---");
                    sw.WriteLine(string.Format("author: \"{0}\"", ModAuthor));
                    sw.WriteLine(string.Format("title: \"{0}\"", ModTitle));
                    sw.WriteLine(string.Format("notes: \"{0}\"", ModNotes));
                    sw.WriteLine(string.Format("previewPath: \"{0}\"", ModPreview));
                    sw.WriteLine("files:");
                    foreach (string file in ModFiles)
                    {
                        sw.WriteLine(string.Format(" - {0}", file));
                    }
                    sw.WriteLine("...");
                }

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
        #endregion
    }
}
