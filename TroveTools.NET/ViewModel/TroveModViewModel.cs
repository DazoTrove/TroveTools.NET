using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        private DelegateCommand _UpdateCommand, _LaunchModSiteCommand, _CopyModUriCommand, _ConvertToTmodCommand, _AutoConvertToTmodCommand;
        private DelegateCommand<string> _InstallCommand, _LaunchPathCommand;

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

        private TroveModPackViewModel _ModPack;
        public TroveModPackViewModel ModPack
        {
            get { return _ModPack; }
            set
            {
                _ModPack = value;
                RaisePropertyChanged("ModPack");
            }
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

        public DelegateCommand ConvertToTmodCommand
        {
            get
            {
                if (_ConvertToTmodCommand == null) _ConvertToTmodCommand = new DelegateCommand(p => ConvertToTmod(), p => !DataObject.TmodFormat);
                return _ConvertToTmodCommand;
            }
        }

        public DelegateCommand AutoConvertToTmodCommand
        {
            get
            {
                if (_AutoConvertToTmodCommand == null) _AutoConvertToTmodCommand = new DelegateCommand(p => ConvertToTmod(true), p => !DataObject.TmodFormat);
                return _AutoConvertToTmodCommand;
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

        public DelegateCommand<string> LaunchPathCommand
        {
            get
            {
                if (_LaunchPathCommand == null) _LaunchPathCommand = new DelegateCommand<string>(LaunchPath);
                return _LaunchPathCommand;
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

        public DelegateCommand CopyModUriCommand
        {
            get
            {
                if (_CopyModUriCommand == null) _CopyModUriCommand = new DelegateCommand(CallCopyModUri);
                return _CopyModUriCommand;
            }
        }
        #endregion

        #region Private methods
        private void ConvertToTmod(bool autoConvert = false)
        {
            try
            {
                if (DataObject.Enabled)
                {
                    var tools = MainWindowViewModel.Instance.ModderTools;
                    tools.CurrentMod = this;
                    MainWindowViewModel.Instance.SetActiveWorkspace(tools);

                    if (autoConvert) tools.BuildTmodCommand.Execute(null);
                }
                else log.WarnFormat("Mod {0} must be enabled before converting to TMOD format", DisplayName);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error converting {0} to TMOD format", DisplayName), ex);
            }
        }

        private void CallUpdateMod(object param = null)
        {
            dynamic mod = this;
            mod.UpdateMod();
        }

        internal static void LaunchPath(string path)
        {
            try
            {
                if (File.Exists(path) || Directory.Exists(path))
                    Process.Start(path);
                else
                    log.ErrorFormat("Path not found: [{0}]", path);
            }
            catch (Exception ex) { log.Error(string.Format("Error launching path: [{0}]", path), ex); }
        }

        private void InstallModFromTrovesaurus(string fileId)
        {
            try
            {
                dynamic mod = null;

                var existingMod = MainWindowViewModel.Instance.MyMods.MyMods.FirstOrDefault(m => m.DataObject.Id == DataObject.Id);
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

        private void CallCopyModUri(object param = null)
        {
            DataObject.CopyModUri();
        }
        #endregion
    }
}
