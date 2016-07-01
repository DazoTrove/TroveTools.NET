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
        private DelegateCommand _RefreshCommand, _UpdateAllCommand, _UninstallAllCommand, _LaunchModsFolderCommand;
        private DelegateCommand<TroveModViewModel> _RemoveModCommand;
        private DelegateCommand<string> _AddModCommand, _SearchMyModsCommand, _SortCommand;
        private CollectionViewSource _MyModsView = new CollectionViewSource();
        private DispatcherTimer _UpdateTimer = null;
        private bool canSaveData = true;

        #region Constructor

        public MyModsViewModel()
        {
            DisplayName = Strings.MyModsViewModel_DisplayName;
            _MyModsView.Source = MyMods;
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

                // Setup auto-saving of my mods when the collection or items in the collection change
                MyMods.CollectionChanged += (s, e) =>
                {
                    if (e.OldItems != null) foreach (INotifyPropertyChanged item in e.OldItems) item.PropertyChanged -= SaveMyMods;
                    if (e.NewItems != null) foreach (INotifyPropertyChanged item in e.NewItems) item.PropertyChanged += SaveMyMods;

                    SaveMyMods();
                };

                // Load mods from model and create view model objects
                foreach (TroveMod mod in TroveMod.MyMods)
                {
                    dynamic modVM = new TroveModViewModel(mod);
                    MyMods.Add(modVM);
                    modVM.CheckForUpdates();
                }

                // If auto update setting is enabled, update all mods on startup
                if (MainWindowViewModel.Instance.Settings.AutoUpdateMods)
                {
                    UpdateAllMods();
                    StartUpdateTimer(SettingsDataProvider.AutoUpdateInterval);
                }

                canSaveData = true;
                SaveMyMods();

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

        public DelegateCommand<string> SearchMyModsCommand
        {
            get
            {
                if (_SearchMyModsCommand == null) _SearchMyModsCommand = new DelegateCommand<string>(SearchMyMods, s => !string.IsNullOrEmpty(s));
                return _SearchMyModsCommand;
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

        private void UpdateAllMods(object param = null)
        {
            try
            {
                log.Info("Checking all mods for updates and installing updates if available");

                // Refresh mod list
                MainWindowViewModel.Instance.GetMoreMods.RefreshCommand.Execute(null);

                // Update all mods
                foreach (dynamic mod in MyMods)
                {
                    mod.CheckForUpdates();
                    if (mod.CanUpdateMod) mod.UpdateMod();
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

        private void SearchMyMods(string param)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                log.Error("Error searching my mods", ex);
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
        #endregion
    }
}
