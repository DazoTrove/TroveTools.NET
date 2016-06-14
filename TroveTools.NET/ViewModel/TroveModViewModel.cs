using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TroveTools.NET.Converter;
using TroveTools.NET.Framework;
using TroveTools.NET.Model;

namespace TroveTools.NET.ViewModel
{
    class TroveModViewModel : ViewModelBase<TroveMod>
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private DelegateCommand _UpdateCommand, _LaunchModSiteCommand;
        private DelegateCommand<string> _InstallCommand;

        #region Constructors
        public TroveModViewModel() : base(new TroveMod()) { }

        public TroveModViewModel(TroveMod dataObject) : base(dataObject) { }

        public TroveModViewModel(string filePath) : base(new TroveMod(filePath)) { }
        #endregion // Constructors

        public override string DisplayName
        {
            get { return DataObject.Name; }
        }

        public bool IsInstalled
        {
            get { return MainWindowViewModel.Instance.MyMods.MyMods.Any(m => m.DataObject.Id == DataObject.Id); }
        }

        /// <summary>
        /// Returns the version name and date of the latest download
        /// </summary>
        public string LatestDownload
        {
            get
            {
                var latest = DataObject.LatestDownload;
                if (latest != null) return latest.ToString();
                return null;
            }
        }

        #region Commands
        public DelegateCommand UpdateCommand
        {
            get
            {
                if (_UpdateCommand == null) _UpdateCommand = new DelegateCommand(CallUpdateMod);
                return _UpdateCommand;
            }
        }

        public DelegateCommand LaunchModSiteCommand
        {
            get
            {
                if (_LaunchModSiteCommand == null) _LaunchModSiteCommand = new DelegateCommand(p => TrovesaurusApi.LaunchModSite(DataObject.Id), p => !string.IsNullOrEmpty(DataObject.Id));
                return _LaunchModSiteCommand;
            }
        }

        public DelegateCommand<string> InstallCommand
        {
            get
            {
                if (_InstallCommand == null) _InstallCommand = new DelegateCommand<string>(InstallModFromTrovesaurus);
                return _InstallCommand;
            }
        }
        #endregion

        #region Private methods
        private void InstallModFromTrovesaurus(string fileId)
        {
            try
            {
                dynamic mod = null;

                var existingMod = MainWindowViewModel.Instance.MyMods.MyMods.Where(m => m.DataObject.Id == DataObject.Id).FirstOrDefault();
                if (existingMod != null)
                {
                    // Update Mod from Trovesaurus
                    mod = existingMod;
                    mod.UpdatePropertiesFromTrovesaurus(DataObject);
                }
                else
                {
                    // Install Mod 
                    mod = this;
                    MainWindowViewModel.Instance.MyMods.MyMods.Add(this);
                }
                
                if (string.IsNullOrEmpty(fileId))
                    mod.UpdateMod();
                else
                    mod.UpdateMod(fileId);
                mod.Enabled = true;
                MainWindowViewModel.Instance.SetActiveWorkspace(MainWindowViewModel.Instance.MyMods);
                MainWindowViewModel.Instance.MyMods.MyModsView.MoveCurrentTo(mod);
            }
            catch (Exception ex)
            {
                log.Error("Error installing mod from Trovesaurus mod list", ex);
            }
        }

        private void CallUpdateMod(object param = null)
        {
            dynamic mod = this;
            mod.UpdateMod();
        }
        #endregion
    }
}
