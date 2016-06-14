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
using TroveTools.NET.DataAccess;
using TroveTools.NET.Framework;
using TroveTools.NET.Model;
using TroveTools.NET.Properties;

namespace TroveTools.NET.ViewModel
{
    class SettingsViewModel : ViewModelBase
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        DelegateCommand<TroveLocationViewModel> _removeLocationCommand;
        DelegateCommand<string> _addLocationCommand;
        bool canSaveData = true;

        #region Constructor

        public SettingsViewModel()
        {
            DisplayName = Strings.SettingsViewModel_DisplayName;
        }
        #endregion // Constructor

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

        private void SaveLocationSettings(object sender = null, PropertyChangedEventArgs e = null)
        {
            if (canSaveData)
            {
                try
                {
                    log.Info("Saving location settings");
                    TroveLocation.Locations = (from loc in Locations
                                               select loc.DataObject).ToList();
                }
                catch (Exception ex) { log.Error("Error saving location settings", ex); }
            }
        }

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
        #endregion

        private void AddLocation(string folder)
        {
            string locationName = string.Format("Trove {0}", Path.GetFileName(folder));
            Locations.Add(new TroveLocationViewModel(locationName, folder));
        }
    }
}
