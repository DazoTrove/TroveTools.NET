using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using TroveTools.NET.Framework;
using TroveTools.NET.DataAccess;
using TroveTools.NET.Model;
using TroveTools.NET.Properties;

namespace TroveTools.NET.ViewModel
{
    class MainWindowViewModel : ViewModelBase, IViewCommandSource
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly MainWindowViewModel _instance;

        private DelegateCommand _LoadDataCommand, _ClosingCommand;
        private bool _dataLoaded = false;
        private string _savedTroveUri = null;

        #region Constructors
        static MainWindowViewModel()
        {
            _instance = new MainWindowViewModel();
        }

        private MainWindowViewModel()
        {
            DisplayName = string.Format(Strings.MainWindowViewModel_DisplayName, ApplicationDetails.GetCurrentVersion());

            LogAppender = NotifyAppender.Appender;

            Settings = new SettingsViewModel();
            MyMods = new MyModsViewModel();
            GetMoreMods = new GetMoreModsViewModel();
            About = new AboutViewModel();
            Trovesaurus = new TrovesaurusViewModel();

            Workspaces = new ObservableCollection<ViewModelBase>() { Trovesaurus, Settings, MyMods, GetMoreMods, About };
        }
        #endregion // Constructors

        #region Public Properties
        /// <summary>
        /// Returns the singleton instance of the MainWindowViewModel class
        /// </summary>
        public static MainWindowViewModel Instance
        {
            get { return _instance; }
        }

        public SettingsViewModel Settings { get; }

        public MyModsViewModel MyMods { get; }

        public GetMoreModsViewModel GetMoreMods { get; }

        public AboutViewModel About { get; }

        public TrovesaurusViewModel Trovesaurus { get; }

        public NotifyAppender LogAppender { get; }

        public ObservableCollection<ViewModelBase> Workspaces { get; }

        public string HtmlFieldStylesheet
        {
            get { return Resources.HtmlFieldStylesheet; }
        }
        #endregion

        #region IViewCommandSource
        private ViewCommandManager _ViewCommandManager = new ViewCommandManager();

        /// <summary>
        /// Gets the ViewCommandManager instance.
        /// </summary>
        public ViewCommandManager ViewCommandManager { get { return _ViewCommandManager; } }
        #endregion IViewCommandSource

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
            try
            {
                // Load data for each of the workspaces
                Settings.LoadData();
                Trovesaurus.LoadData();
                GetMoreMods.LoadData();
                MyMods.LoadData();

                _dataLoaded = true;

                // Process saved Trove URI to handle case when Trove URI link clicked before application was fully loaded
                if (!string.IsNullOrEmpty(_savedTroveUri)) ProcessTroveUri(_savedTroveUri);

                // Process a URI from the command line args if there are any
                var args = ApplicationDetails.GetTroveUri();
                if (!string.IsNullOrEmpty(args)) ProcessTroveUri(args);
            }
            catch (Exception ex) { log.Error("Error loading main window data", ex); }
            finally { _dataLoaded = true; }
        }

        public void Closing(object param = null)
        {
            Settings.Closing();
            MyMods.Closing();
            Trovesaurus.Closing();
        }

        public void ProcessTroveUri(string troveUri)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                ViewCommandManager.InvokeLoaded("RestoreWindow");
                if (string.IsNullOrEmpty(troveUri)) return;

                if (_dataLoaded)
                {
                    log.InfoFormat("Processing Trove URI: {0}", troveUri);
                    var args = ApplicationDetails.GetApplicationArguments(troveUri);
                    if (args != null)
                    {
                        SetActiveWorkspace(MyMods);
                        if (args.LinkType == ApplicationDetails.AppArgs.LinkTypes.Mod) TroveUriInstallMod(args.ModId, args.FileId);
                        if (args.LinkType == ApplicationDetails.AppArgs.LinkTypes.ModPack) MyMods.TroveUriInstallModPack(args.Uri);
                    }
                }
                else _savedTroveUri = troveUri;
            }
            else Application.Current.Dispatcher.Invoke(() => ProcessTroveUri(troveUri));
        }

        public void TroveUriInstallMod(string modId, string fileId)
        {
            log.InfoFormat("Installing mod id [{0}], file id [{1}] from Trove URI argument", modId, fileId);
            var modVm = GetMoreMods.TrovesaurusMods.FirstOrDefault(mod => mod.DataObject.Id == modId);
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
