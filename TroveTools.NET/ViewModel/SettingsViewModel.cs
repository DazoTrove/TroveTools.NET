using log4net;
using Newtonsoft.Json;
using System;
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
    class SettingsViewModel : ViewModelBase
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private DelegateCommand<TroveLocationViewModel> _removeLocationCommand;
        private DelegateCommand<string> _addLocationCommand;
        private DelegateCommand _DetectLocationsCommand;
        private ObservableCollection<TimeSpan> _AutoUpdateIntervals = new ObservableCollection<TimeSpan>();
        private CollectionViewSource _AutoUpdateIntervalsView = new CollectionViewSource();
        private bool canSaveData = true;

        #region Constructor

        public SettingsViewModel()
        {
            DisplayName = Strings.SettingsViewModel_DisplayName;
            _AutoUpdateIntervalsView.Source = _AutoUpdateIntervals;
        }
        #endregion // Constructor

        #region Public Methods
        public void LoadData()
        {
            canSaveData = false;
            try
            {
                log.Info("Loading location settings");

                // Setup auto-saving of locations when the collection or items in the collection change
                Locations.CollectionChanged += (s, e) =>
                {
                    if (e.OldItems != null) foreach (INotifyPropertyChanged item in e.OldItems) item.PropertyChanged -= SaveLocationSettings;
                    if (e.NewItems != null) foreach (INotifyPropertyChanged item in e.NewItems) item.PropertyChanged += SaveLocationSettings;

                    SaveLocationSettings();
                };

                // Load locations from model and create view model objects
                foreach (TroveLocation loc in TroveLocation.Locations)
                {
                    Locations.Add(new TroveLocationViewModel(loc));
                }

                // Start Trove game status timer on load if setting is enabled
                if (UpdateTroveGameStatus) TroveGameStatus.StartTimer(TrovesaurusAccountLinkKey);

                // Setup auto update interval options
#if DEBUG
                _AutoUpdateIntervals.Add(new TimeSpan(0, 0, 30));
                _AutoUpdateIntervals.Add(new TimeSpan(0, 1, 0));
#endif
                _AutoUpdateIntervals.Add(new TimeSpan(0, 5, 0));
                _AutoUpdateIntervals.Add(new TimeSpan(0, 10, 0));
                _AutoUpdateIntervals.Add(new TimeSpan(0, 15, 0));
                _AutoUpdateIntervals.Add(new TimeSpan(0, 30, 0));
                _AutoUpdateIntervals.Add(new TimeSpan(0, 45, 0));
                _AutoUpdateIntervals.Add(new TimeSpan(1, 0, 0));
                _AutoUpdateIntervals.Add(new TimeSpan(2, 0, 0));
                _AutoUpdateIntervals.Add(new TimeSpan(3, 0, 0));
                _AutoUpdateIntervals.Add(new TimeSpan(4, 0, 0));
                _AutoUpdateIntervals.Add(new TimeSpan(5, 0, 0));
                _AutoUpdateIntervals.Add(new TimeSpan(6, 0, 0));
                _AutoUpdateIntervals.Add(new TimeSpan(12, 0, 0));
                _AutoUpdateIntervals.Add(new TimeSpan(1, 0, 0, 0));

                if (_AutoUpdateIntervals.Contains(SettingsDataProvider.AutoUpdateInterval)) AutoUpdateIntervalsView.MoveCurrentTo(SettingsDataProvider.AutoUpdateInterval);
                AutoUpdateIntervalsView.CurrentChanged += (s, e) =>
                {
                    SettingsDataProvider.AutoUpdateInterval = (TimeSpan)AutoUpdateIntervalsView.CurrentItem;
                    if (AutoUpdateMods) MainWindowViewModel.Instance.MyMods.StartUpdateTimer(SettingsDataProvider.AutoUpdateInterval);
                };

                canSaveData = true;
                SaveLocationSettings();
            }
            catch (Exception ex)
            {
                log.Error("Error loading location settings", ex);
            }
            finally
            {
                canSaveData = true;
            }
        }

        public void Closing()
        {
            if (UpdateTroveGameStatus) TroveGameStatus.StopTimer();
        }
        #endregion

        #region Properties
        public ObservableCollection<TroveLocationViewModel> Locations { get; } = new ObservableCollection<TroveLocationViewModel>();

        public bool TroveUriEnabled
        {
            get { return RegistrySettings.IsTroveUrlProtocolRegistered; }
            set
            {
                if (value)
                    RegistrySettings.RegisterTroveUrlProtocol();
                else
                    RegistrySettings.UnregisterTroveUrlProtocol();

                RaisePropertyChanged("TroveUriEnabled");
            }
        }

        public bool RunAtStartup
        {
            get { return RegistrySettings.RunAtStartup; }
            set
            {
                RegistrySettings.RunAtStartup = value;
                RaisePropertyChanged("RunAtStartup");
            }
        }

        public bool StartMinimized
        {
            get { return SettingsDataProvider.StartMinimized; }
            set
            {
                SettingsDataProvider.StartMinimized = value;
                RaisePropertyChanged("StartMinimized");
            }
        }

        public bool MinimizeToTray
        {
            get { return SettingsDataProvider.MinimizeToTray; }
            set
            {
                SettingsDataProvider.MinimizeToTray = value;
                RaisePropertyChanged("MinimizeToTray");
            }
        }

        public bool AutoUpdateMods
        {
            get { return SettingsDataProvider.AutoUpdateMods; }
            set
            {
                SettingsDataProvider.AutoUpdateMods = value;
                if (value)
                    MainWindowViewModel.Instance.MyMods.StartUpdateTimer(SettingsDataProvider.AutoUpdateInterval);
                else
                    MainWindowViewModel.Instance.MyMods.StopUpdateTimer();
                RaisePropertyChanged("AutoUpdateMods");
            }
        }

        public bool UpdateTroveGameStatus
        {
            get { return SettingsDataProvider.UpdateTroveGameStatus; }
            set
            {
                SettingsDataProvider.UpdateTroveGameStatus = value;
                RaisePropertyChanged("UpdateTroveGameStatus");

                if (value)
                    TroveGameStatus.StartTimer(TrovesaurusAccountLinkKey);
                else
                    TroveGameStatus.StopTimer();
            }
        }

        public bool TrovesaurusCheckMail
        {
            get { return SettingsDataProvider.TrovesaurusCheckMail; }
            set
            {
                SettingsDataProvider.TrovesaurusCheckMail = value;
                RaisePropertyChanged("TrovesaurusCheckMail");
            }
        }

        public bool TrovesaurusServerStatus
        {
            get { return SettingsDataProvider.TrovesaurusServerStatus; }
            set
            {
                SettingsDataProvider.TrovesaurusServerStatus = value;
                RaisePropertyChanged("TrovesaurusServerStatus");
            }
        }

        public string TrovesaurusAccountLinkKey
        {
            get { return SettingsDataProvider.TrovesaurusAccountLinkKey; }
            set
            {
                SettingsDataProvider.TrovesaurusAccountLinkKey = value;
                RaisePropertyChanged("TrovesaurusAccountLinkKey");
            }
        }

        public ICollectionView AutoUpdateIntervalsView
        {
            get { return _AutoUpdateIntervalsView.View; }
        }

        #endregion

        #region Commands
        public DelegateCommand<string> AddLocationCommand
        {
            get
            {
                if (_addLocationCommand == null) _addLocationCommand = new DelegateCommand<string>(AddLocation);
                return _addLocationCommand;
            }
        }

        public DelegateCommand<TroveLocationViewModel> RemoveLocationCommand
        {
            get
            {
                if (_removeLocationCommand == null)
                {
                    _removeLocationCommand = new DelegateCommand<TroveLocationViewModel>(
                      currentItem => Locations.Remove(currentItem),
                      currentItem => currentItem != null);
                }
                return _removeLocationCommand;
            }
        }
        public DelegateCommand DetectLocationsCommand
        {
            get
            {
                if (_DetectLocationsCommand == null) _DetectLocationsCommand = new DelegateCommand(DetectLocations);
                return _DetectLocationsCommand;
            }
        }
        #endregion

        #region Private Methods
        private void SaveLocationSettings(object sender = null, PropertyChangedEventArgs e = null)
        {
            if (canSaveData)
            {
                try
                {
                    log.Info("Saving location settings");
                    TroveLocation.Locations = GetLocationsList();
                }
                catch (Exception ex) { log.Error("Error saving location settings", ex); }
            }
        }

        private void AddLocation(string folder)
        {
            string locationName = string.Format("Trove {0}", Path.GetFileName(folder));
            Locations.Add(new TroveLocationViewModel(locationName, folder));
        }

        private List<TroveLocation> GetLocationsList()
        {
            return (from loc in Locations
                    select loc.DataObject).ToList();
        }

        private void DetectLocations(object param = null)
        {
            var locations = GetLocationsList();
            TroveLocation.DetectLocations(locations);
            var ic = StringComparison.OrdinalIgnoreCase;
            
            foreach (var loc in locations)
            {
                // Add any locations detected that are not already present
                if (!Locations.Any(l => l.DataObject.LocationPath.Equals(loc.LocationPath, ic)))
                    Locations.Add(new TroveLocationViewModel(loc));
            }
        }
        #endregion
    }
}
