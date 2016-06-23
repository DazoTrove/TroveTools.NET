using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;
using TroveTools.NET.Framework;
using TroveTools.NET.Model;
using TroveTools.NET.Properties;

namespace TroveTools.NET.ViewModel
{
    class MainWindowViewModel : ViewModelBase
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly MainWindowViewModel instance = new MainWindowViewModel();

        private DelegateCommand _LoadDataCommand, _ClosingCommand;

        #region Constructors
        static MainWindowViewModel() { } // Static constructor for singleton instance pattern

        private MainWindowViewModel()
        {
            DisplayName = string.Format(Strings.MainWindowViewModel_DisplayName, ApplicationDetails.GetCurrentVersion());

            LogAppender = NotifyAppender.Appender;

            Settings = new SettingsViewModel();
            MyMods = new MyModsViewModel();
            GetMoreMods = new GetMoreModsViewModel();
            About = new AboutViewModel();
            Trovesaurus = new TrovesaurusViewModel();

            Workspaces = new ObservableCollection<ViewModelBase>() { /*Trovesaurus,*/ Settings, MyMods, GetMoreMods, About };
        }
        #endregion // Constructors

        #region Public Properties
        /// <summary>
        /// Returns the singleton instance of the MainWindowViewModel class
        /// </summary>
        public static MainWindowViewModel Instance
        {
            get { return instance; }
        }

        public SettingsViewModel Settings { get; }

        public MyModsViewModel MyMods { get; }

        public GetMoreModsViewModel GetMoreMods { get; }

        public AboutViewModel About { get; }

        public TrovesaurusViewModel Trovesaurus { get; }

        public NotifyAppender LogAppender { get; }

        public ObservableCollection<ViewModelBase> Workspaces { get; }
        #endregion

        #region Commands
        public DelegateCommand LoadDataCommand
        {
            get
            {
                if (_LoadDataCommand == null) _LoadDataCommand = new DelegateCommand(LoadData);
                return _LoadDataCommand;
            }
        }

        public DelegateCommand ClosingCommand
        {
            get
            {
                if (_ClosingCommand == null) _ClosingCommand = new DelegateCommand(Closing);
                return _ClosingCommand;
            }
        }
        #endregion

        #region Public Methods
        public void LoadData(object param = null)
        {
            // Load data for each of the workspaces
            Settings.LoadData();
            GetMoreMods.LoadData();
            MyMods.LoadData();

            var args = ApplicationDetails.GetApplicationArguments();
            if (args != null) TroveUriInstallMod(args.ModId, args.FileId);
        }

        public void Closing(object param = null)
        {
            Settings.Closing();
        }

        public void TroveUriInstallMod(string modId, string fileId)
        {
            log.InfoFormat("Installing mod id [{0}], file id [{1}] from Trove URI argument", modId, fileId);
            var modVm = GetMoreMods.TrovesaurusMods.Where(mod => mod.DataObject.Id == modId).FirstOrDefault();
            if (modVm != null)
                modVm.InstallCommand.Execute(fileId);
            else
                log.WarnFormat("Mod ID [{0}] not found", modId);
        }

        public void SetActiveWorkspace(ViewModelBase workspace)
        {
            if (workspace != null) CollectionViewSource.GetDefaultView(Workspaces).MoveCurrentTo(workspace);
        }

        public void SetActiveWorkspace(Type workspaceType)
        {
            SetActiveWorkspace(Workspaces.FirstOrDefault(vm => vm.GetType() == workspaceType));
        }

        public ViewModelBase CurrentWorkspace
        {
            get { return CollectionViewSource.GetDefaultView(Workspaces).CurrentItem as ViewModelBase; }
        }
        #endregion
    }
}
