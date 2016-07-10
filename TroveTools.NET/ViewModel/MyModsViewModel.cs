using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;
using TroveTools.NET.Converter;
using TroveTools.NET.DataAccess;
using TroveTools.NET.Framework;
using TroveTools.NET.Model;
using TroveTools.NET.Properties;

namespace TroveTools.NET.ViewModel
{
    class MyModsViewModel : ViewModelBase
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private DelegateCommand _RefreshCommand, _UpdateAllCommand, _UninstallAllCommand, _LaunchModsFolderCommand, _ClearModPackNameCommand;
        private DelegateCommand<TroveModViewModel> _RemoveModCommand;
        private DelegateCommand<TroveModPackViewModel> _LoadModPackCommand, _RemoveModPackCommand, _CopyModPackLinkCommand;
        private DelegateCommand<string> _AddModCommand, _SortCommand, _SaveModPackCommamd, _LaunchTrovesaurusModPacksCommand;
        private CollectionViewSource _MyModsView = new CollectionViewSource(), _ModPacksView = new CollectionViewSource();
        private DispatcherTimer _UpdateTimer = null;
        private bool canSaveData = true;

        #region Constructor

        public MyModsViewModel()
        {
            DisplayName = Strings.MyModsViewModel_DisplayName;
            _MyModsView.Source = MyMods;
            _MyModsView.IsLiveGroupingRequested = true;
            _MyModsView.GroupDescriptions.Add(new PropertyGroupDescription("ModPack"));
            _ModPacksView.Source = ModPacks;
            _ModPacksView.GroupDescriptions.Add(new PropertyGroupDescription("Source"));
            _ModPacksView.SortDescriptions.Add(new SortDescription("Source", ListSortDirection.Ascending));
        }

        #endregion // Constructor

        #region Public Methods
        public void LoadData()
        {
            // Load data and setup saving MyMods list to settings
            canSaveData = false;
            try
            {
                log.Info("Loading my mods");

                // Load mods from model and create view model objects
                foreach (TroveMod mod in TroveMod.MyMods)
                {
                    dynamic modVM = new TroveModViewModel(mod);
                    MyMods.Add(modVM);
                    modVM.CheckForUpdates();
                }

                // Load local mod packs
                foreach (TroveModPack pack in SettingsDataProvider.MyModPacks) ModPacks.Add(new TroveModPackViewModel(pack));

                // Load mod packs from Trovesaurus API
                foreach (TroveModPack pack in TrovesaurusApi.ModPackList) ModPacks.Add(new TroveModPackViewModel(pack));

                // If auto update setting is enabled, update all mods on startup
                if (MainWindowViewModel.Instance.Settings.AutoUpdateMods)
                {
                    UpdateAllMods();
                    StartUpdateTimer(SettingsDataProvider.AutoUpdateInterval);
                }

                // Setup auto-saving of my mods when the collection or items in the collection change
                MyMods.CollectionChanged += (s, e) =>
                {
                    if (e.OldItems != null) foreach (INotifyPropertyChanged item in e.OldItems) item.PropertyChanged -= SaveMyMods;
                    if (e.NewItems != null) foreach (INotifyPropertyChanged item in e.NewItems) item.PropertyChanged += SaveMyMods;

                    SaveMyMods();
                };

                // Setup auto-saving of mod packs when the collection or items in the collection change
                ModPacks.CollectionChanged += (s, e) =>
                {
                    SaveModPacks();
                };

                canSaveData = true;
                SaveMyMods();
                SaveModPacks();

                log.Info("Loaded my mods");
            }
            catch (Exception ex)
            {
                log.Error("Error loading my mods", ex);
            }
            finally
            {
                canSaveData = true;
            }
        }

        public void Closing()
        {
            StopUpdateTimer();
            SaveMyMods();
        }

        public void StartUpdateTimer(TimeSpan autoUpdateInterval)
        {
            try
            {
                log.InfoFormat("Starting update timer, checking every {0}", autoUpdateInterval.ToUserFriendlyString());
                if (_UpdateTimer == null)
                {
                    _UpdateTimer = new DispatcherTimer();
                    _UpdateTimer.Tick += (s, e) => UpdateAllMods();
                }
                _UpdateTimer.Interval = autoUpdateInterval;
                _UpdateTimer.Start();
            }
            catch (Exception ex)
            {
                log.Error("Error starting update timer", ex);
            }
        }

        public void StopUpdateTimer()
        {
            _UpdateTimer?.Stop();
        }
        #endregion

        #region Public Properties
        public ObservableCollection<TroveModViewModel> MyMods { get; } = new ObservableCollection<TroveModViewModel>();

        public ICollectionView MyModsView
        {
            get { return _MyModsView.View; }
        }

        public ObservableCollection<TroveModPackViewModel> ModPacks { get; } = new ObservableCollection<TroveModPackViewModel>();

        public ICollectionView ModPacksView
        {
            get { return _ModPacksView.View; }
        }

        private string _ModPackName = string.Empty;
        public string ModPackName
        {
            get { return _ModPackName; }
            set
            {
                _ModPackName = value;
                RaisePropertyChanged("ModPackName");
            }
        }

        public string LastAddModLocation
        {
            get { return SettingsDataProvider.LastAddModLocation; }
            set
            {
                SettingsDataProvider.LastAddModLocation = value;
                RaisePropertyChanged("LastAddModLocation");
            }
        }

        private ListSortDirection _SortDirection;
        public ListSortDirection SortDirection
        {
            get { return _SortDirection; }
            set
            {
                _SortDirection = value;
                RaisePropertyChanged("SortDirection");
            }
        }

        private string _SortBy;
        public string SortBy
        {
            get { return _SortBy; }
            set
            {
                _SortBy = value;
                RaisePropertyChanged("SortBy");
            }
        }
        #endregion

        #region Commands
        public DelegateCommand RefreshCommand
        {
            get
            {
                if (_RefreshCommand == null) _RefreshCommand = new DelegateCommand(RefreshMods);
                return _RefreshCommand;
            }
        }

        public DelegateCommand UpdateAllCommand
        {
            get
            {
                if (_UpdateAllCommand == null) _UpdateAllCommand = new DelegateCommand(UpdateAllMods, p => MyMods.Count > 0);
                return _UpdateAllCommand;
            }
        }

        public DelegateCommand<string> AddModCommand
        {
            get
            {
                if (_AddModCommand == null) _AddModCommand = new DelegateCommand<string>(AddMod);
                return _AddModCommand;
            }
        }

        public DelegateCommand<TroveModViewModel> RemoveModCommand
        {
            get
            {
                if (_RemoveModCommand == null) _RemoveModCommand = new DelegateCommand<TroveModViewModel>(
                    currentItem => RemoveMod(currentItem), currentItem => currentItem != null);
                return _RemoveModCommand;
            }
        }

        public DelegateCommand UninstallAllCommand
        {
            get
            {
                if (_UninstallAllCommand == null) _UninstallAllCommand = new DelegateCommand(UninstallAllMods);
                return _UninstallAllCommand;
            }
        }

        public DelegateCommand LaunchModsFolderCommand
        {
            get
            {
                if (_LaunchModsFolderCommand == null) _LaunchModsFolderCommand = new DelegateCommand(p => TroveModViewModel.LaunchPath(SettingsDataProvider.ModsFolder));
                return _LaunchModsFolderCommand;
            }
        }

        public DelegateCommand<string> SortCommand
        {
            get
            {
                if (_SortCommand == null) _SortCommand = new DelegateCommand<string>(SortModList);
                return _SortCommand;
            }
        }

        public DelegateCommand<string> LaunchTrovesaurusModPacksCommand
        {
            get
            {
                if (_LaunchTrovesaurusModPacksCommand == null) _LaunchTrovesaurusModPacksCommand = new DelegateCommand<string>(p => LaunchTrovesaurus(TrovesaurusApi.ModPacksUrl));
                return _LaunchTrovesaurusModPacksCommand;
            }
        }

        public DelegateCommand ClearModPackNameCommand
        {
            get
            {
                if (_ClearModPackNameCommand == null) _ClearModPackNameCommand = new DelegateCommand(p => ModPackName = string.Empty);
                return _ClearModPackNameCommand;
            }
        }

        public DelegateCommand<TroveModPackViewModel> LoadModPackCommand
        {
            get
            {
                if (_LoadModPackCommand == null) _LoadModPackCommand = new DelegateCommand<TroveModPackViewModel>(
                    currentItem => LoadModPack(currentItem), currentItem => currentItem != null);
                return _LoadModPackCommand;
            }
        }

        public DelegateCommand<TroveModPackViewModel> RemoveModPackCommand
        {
            get
            {
                if (_RemoveModPackCommand == null) _RemoveModPackCommand = new DelegateCommand<TroveModPackViewModel>(
                    currentItem => RemoveModPack(currentItem), currentItem => currentItem != null && currentItem.DataObject.Source == TroveModPack.LocalSource);
                return _RemoveModPackCommand;
            }
        }

        public DelegateCommand<string> SaveModPackCommamd
        {
            get
            {
                if (_SaveModPackCommamd == null) _SaveModPackCommamd = new DelegateCommand<string>(SaveModPack, s => !string.IsNullOrEmpty(s));
                return _SaveModPackCommamd;
            }
        }

        public DelegateCommand<TroveModPackViewModel> CopyModPackLinkCommand
        {
            get
            {
                if (_CopyModPackLinkCommand == null) _CopyModPackLinkCommand = new DelegateCommand<TroveModPackViewModel>(
                    currentItem => CopyModPackLink(currentItem), currentItem => currentItem != null);
                return _CopyModPackLinkCommand;
            }
        }
        #endregion

        #region Private methods
        private void RefreshMods(object obj)
        {
            // Refresh mod list
            MainWindowViewModel.Instance.GetMoreMods.RefreshCommand.Execute(null);

            log.Info("Checking all mods for updates");
            foreach (dynamic mod in MyMods)
            {
                mod.CheckForUpdates();
            }
        }

        private void SaveMyMods(object sender = null, PropertyChangedEventArgs e = null)
        {
            // Ignore property change events for CanUpdateMod and Enabled since those changes will be covered by the Status property changing
            if (canSaveData && e?.PropertyName != "CanUpdateMod" && e?.PropertyName != "Enabled")
            {
                try { TroveMod.SaveMyMods((from mod in MyMods select mod.DataObject).ToList()); }
                catch (Exception ex) { log.Error("Error saving my mods", ex); }
            }
        }

        private void SaveModPacks(object sender = null, PropertyChangedEventArgs e = null)
        {
            if (canSaveData)
            {
                SettingsDataProvider.MyModPacks = ModPacks.Where(p => p.DataObject.Source == TroveModPack.LocalSource).Select(p => p.DataObject).ToList();
            }
        }

        private void UpdateAllMods(object param = null)
        {
            try
            {
                // Refresh mod list
                MainWindowViewModel.Instance.GetMoreMods.RefreshCommand.Execute(null);

                log.Info("Checking all mods for updates and installing updates if available");

                // Update all mods
                foreach (dynamic mod in MyMods)
                {
                    mod.CheckForUpdates();
                    if (mod.DataObject.CanUpdateMod) mod.UpdateMod();
                }
            }
            catch (Exception ex)
            {
                log.Error("Error updating all mods", ex);
            }
        }

        private void AddMod(string filePath)
        {
            try
            {
                // Use a dynamic ViewModel object to trigger status property notifications
                dynamic modVM = new TroveModViewModel(filePath);
                MyMods.Add(modVM);

                modVM.AddMod();
                modVM.InstallMod();
            }
            catch (Exception ex)
            {
                log.Error("Error adding mod: " + filePath, ex);
            }
        }

        private void RemoveMod(dynamic mod)
        {
            try
            {
                // Use a dynamic ViewModel object to trigger status property notifications
                mod.RemoveMod();

                // Remove mod from MyMods collection as well
                MyMods.Remove(mod);
            }
            catch (Exception ex)
            {
                log.Error("Error removing selected mod", ex);
            }
        }

        private void UninstallAllMods(object param = null)
        {
            try
            {
                log.Info("Uninstalling all mods");
                foreach (dynamic mod in MyMods.Where(m => m.DataObject.Enabled))
                {
                    mod.Enabled = false;
                }
                TroveMod.RemoveModFolders();
            }
            catch (Exception ex)
            {
                log.Error("Error uninstalling all mods", ex);
            }
        }

        private void SortModList(string column)
        {
            try
            {
                // Toggle sort direction if previously sorted by this column
                if (SortBy == column)
                    SortDirection = SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                else
                {
                    // Set sort column and initial sort direction
                    SortBy = column;
                    SortDirection = column == "LastUpdated" ? ListSortDirection.Descending : ListSortDirection.Ascending;
                }
                log.InfoFormat("Sorting by {0}, direction: {1}", SortBy, SortDirection);

                MyModsView.SortDescriptions.Clear();
                MyModsView.SortDescriptions.Add(new SortDescription(SortBy, SortDirection));
            }
            catch (Exception ex)
            {
                log.Error("Error sorting mod list", ex);
            }
        }

        private void LaunchTrovesaurus(string url = null)
        {
            try { TrovesaurusApi.LaunchTrovesaurus(url); }
            catch (Exception ex) { log.Error("Error launching Trovesaurus page", ex); }
        }

        public void TroveUriInstallModPack(string uri)
        {
            log.InfoFormat("Installing mod pack: [{0}] from Trove URI argument", uri);
            TroveModPack pack = new TroveModPack(uri);
            TroveModPackViewModel packVm = null;
            if (string.IsNullOrEmpty(pack.PackId))
            {
                // Install Ad Hoc mod pack
                packVm = new TroveModPackViewModel(pack);
                ModPacks.Add(packVm);
            }
            else
            {
                // Find Trovesaurus mod pack
                packVm = ModPacks.FirstOrDefault(p => p.DataObject.PackId == pack.PackId);
                if (packVm == null)
                {
                    log.ErrorFormat("Could not find Trovesaurus mod pack ID {0}", pack.PackId);
                    return;
                }
            }
            ModPacksView.MoveCurrentTo(packVm);
            LoadModPack(packVm);
        }

        private void LoadModPack(TroveModPackViewModel currentItem)
        {
            log.InfoFormat("Loading mod pack: {0}", currentItem.DataObject.Name);
            foreach (var mod in currentItem.DataObject.Mods)
            {
                var modVm = MyMods.FirstOrDefault(m => m.DataObject.Id == mod.Id);
                if (modVm == null)
                {
                    modVm = MainWindowViewModel.Instance.GetMoreMods.TrovesaurusMods.FirstOrDefault(m => m.DataObject.Id == mod.Id);
                    if (modVm == null) modVm = new TroveModViewModel(mod);
                    modVm.InstallCommand.Execute(null);
                }
                modVm.ModPack = currentItem;
                modVm.DataObject.PackName = currentItem.DisplayName;
            }
        }

        private void CopyModPackLink(TroveModPackViewModel currentItem)
        {
            currentItem.DataObject.CopyModPackUri();
        }

        private void RemoveModPack(TroveModPackViewModel currentItem)
        {
            log.InfoFormat("Removing mod pack: {0}", currentItem.DataObject.Name);
            foreach (var mod in MyMods.Where(m => m.ModPack == currentItem))
            {
                mod.ModPack = null;
                mod.DataObject.PackName = null;
            }
            ModPacks.Remove(currentItem);
        }

        private void SaveModPack(string modPackName)
        {
            bool newPack = false;
            if (string.IsNullOrEmpty(modPackName))
            {
                log.Warn("No mod pack name specified to save");
                return;
            }
            log.InfoFormat("Saving enabled standalone mods as mod pack {0}", modPackName);

            TroveModPackViewModel pack = ModPacks.FirstOrDefault(p => p.DataObject.Name == modPackName && p.DataObject.Source == TroveModPack.LocalSource);
            if (pack == null)
            {
                pack = new TroveModPackViewModel(new TroveModPack());
                pack.DataObject.Name = modPackName;
                newPack = true;
            }
            else pack.DataObject.Mods.Clear();

            foreach (var mod in MyMods.Where(m => m.DataObject.Enabled && m.ModPack == null))
            {
                if (string.IsNullOrEmpty(mod.DataObject.Id))
                {
                    log.WarnFormat("Only mods downloaded from Trovesaurus can be included in mod packs, skipping mod: {0}", mod.DisplayName);
                    continue;
                }
                pack.DataObject.Mods.Add(mod.DataObject);
                mod.ModPack = pack;
                mod.DataObject.PackName = null;
            }

            if (pack.DataObject.Mods.Count > 0)
            {
                if (newPack) ModPacks.Add(pack);
                ModPacksView.MoveCurrentTo(pack);
            }
            else
                log.ErrorFormat("No enabled standalone mods from Trovesaurus were added to mod pack {0}", modPackName);
        }
        #endregion
    }
}
