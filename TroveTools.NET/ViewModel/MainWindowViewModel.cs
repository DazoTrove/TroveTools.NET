using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Deployment.Application;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;
using TroveTools.NET.Framework;
using TroveTools.NET.Properties;

namespace TroveTools.NET.ViewModel
{
    class MainWindowViewModel : ViewModelBase
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly MainWindowViewModel instance = new MainWindowViewModel();

        DelegateCommand _loadDataCommand;

        #region Constructors
        static MainWindowViewModel() { } // Static constructor for singleton instance pattern

        private MainWindowViewModel()
        {
            DisplayName = string.Format(Strings.MainWindowViewModel_DisplayName, GetCurrentVersion());

            LogAppender = NotifyAppender.Appender;

            Settings = new SettingsViewModel();
            MyMods = new MyModsViewModel();
            GetMoreMods = new GetMoreModsViewModel();
            About = new AboutViewModel();
            Trovesaurus = new TrovesaurusViewModel();

            Workspaces = new ObservableCollection<ViewModelBase>() { /*Trovesaurus,*/ Settings, MyMods, GetMoreMods, About };
        }

        private object GetCurrentVersion()
        {
            if (ApplicationDeployment.IsNetworkDeployed) return ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
        #endregion // Constructors

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

        #region Commands
        public DelegateCommand LoadDataCommand
        {
            get
            {
                if (_loadDataCommand == null) _loadDataCommand = new DelegateCommand(LoadData);
                return _loadDataCommand;
            }
        }
        #endregion

        public void LoadData(object param = null)
        {
            // Load data for each of the workspaces
            Settings.LoadData();
            GetMoreMods.LoadData();
            MyMods.LoadData();

            TroveUriInstallMod(Environment.GetCommandLineArgs());
        }

        /// <summary>
        /// Parses command line arguments to install the given mod and file from Trovesaurus
        /// </summary>
        public void TroveUriInstallMod(string[] args)
        {
            if (args.Length > 1)
            {
                // Install mod from link (ex: trove://6;12)
                Match m = Regex.Match(args[1], @"trove:(?://)?(?<ModId>\d+);(?<FileId>\d+)", RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    string id = m.Groups["ModId"].Value;
                    string fileId = m.Groups["FileId"].Value;
                    var modVm = GetMoreMods.TrovesaurusMods.Where(mod => mod.DataObject.Id == id).FirstOrDefault();
                    modVm.InstallCommand.Execute(fileId);
                }
                else
                    log.WarnFormat("Unknown command line argument: [{0}]", args[1]);
            }
        }

        public ObservableCollection<ViewModelBase> Workspaces { get; }

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
    }
}
