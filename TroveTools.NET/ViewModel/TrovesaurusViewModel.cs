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
using TroveTools.NET.Framework;
using TroveTools.NET.Model;
using TroveTools.NET.Properties;

namespace TroveTools.NET.ViewModel
{
    class TrovesaurusViewModel : ViewModelBase
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private DelegateCommand<string> _LaunchTrovesaurusCommand, _LaunchTrovesaurusMailCommand, _LaunchServerStatusCommand, _LaunchTrovesaurusLivestreamsCommand,
            _LaunchTrovesaurusNewCommand, _LaunchTrovesaurusNewsTagCommand, _LaunchTrovesaurusCalendarCommand;
        private DelegateCommand _RefreshDataCommand;
        private CollectionViewSource _NewsView = new CollectionViewSource(), _CalendarView = new CollectionViewSource(), _StreamsView = new CollectionViewSource();
        private DispatcherTimer _StatusTimer = null;
        private DateTime lastUpdated = DateTime.MinValue;

        public TrovesaurusViewModel()
        {
            DisplayName = Strings.TrovesaurusViewModel_DisplayName;

            _NewsView.Source = NewsItems;
            _CalendarView.Source = CalendarItems;
            _StreamsView.Source = OnlineStreams;
        }

        public void LoadData(object param = null)
        {
            // Load Trovesaurus data from API
            try
            {
                log.Info("Loading Trovesaurus news, calendar, and streams data");
                if (_StatusTimer == null) StartStatusTimer();
                CheckStatus();

                TrovesaurusApi.RefreshNewsList();
                NewsItems.Clear();
                foreach (var item in TrovesaurusApi.NewsList) NewsItems.Add(item);

                TrovesaurusApi.RefreshCalendarList();
                CalendarItems.Clear();
                foreach (var item in TrovesaurusApi.CalendarList) CalendarItems.Add(item);

                TrovesaurusApi.RefreshStreamList();
                OnlineStreams.Clear();
                foreach (var item in TrovesaurusApi.StreamList) OnlineStreams.Add(item);

                CalendarView.SortDescriptions.Clear();
                CalendarView.SortDescriptions.Add(new SortDescription("EndDateTime", ListSortDirection.Ascending));

                log.Info("Trovesaurus news, calendar, and streams data loaded successfully");
            }
            catch (Exception ex)
            {
                log.Error("Error loading Trovesaurus news, calendar or streams", ex);
            }
        }

        public void Closing()
        {
            StopStatusTimer();
        }

        public void StartStatusTimer()
        {
            try
            {
                if (MainWindowViewModel.Instance.Settings.TrovesaurusCheckMail && string.IsNullOrEmpty(MainWindowViewModel.Instance.Settings.TrovesaurusAccountLinkKey))
                {
                    log.Error("Trovesaurus mail checking requires that a valid account link key is entered");
                    MainWindowViewModel.Instance.Settings.TrovesaurusCheckMail = false;
                }
                TimeSpan checkInterval = new TimeSpan(0, 1, 0);

                if (MainWindowViewModel.Instance.Settings.TrovesaurusCheckMail && MainWindowViewModel.Instance.Settings.TrovesaurusServerStatus)
                    log.InfoFormat("Starting Trovesaurus server status and mail check timer, checking every {0}", checkInterval.ToUserFriendlyString());
                else if (MainWindowViewModel.Instance.Settings.TrovesaurusCheckMail)
                    log.InfoFormat("Starting Trovesaurus mail check timer, checking every {0}", checkInterval.ToUserFriendlyString());
                else if (MainWindowViewModel.Instance.Settings.TrovesaurusServerStatus)
                    log.InfoFormat("Starting Trovesaurus server status timer, checking every {0}", checkInterval.ToUserFriendlyString());

                if (_StatusTimer == null)
                {
                    _StatusTimer = new DispatcherTimer();
                    _StatusTimer.Tick += (s, e) => CheckStatus();
                }
                _StatusTimer.Interval = checkInterval;
                _StatusTimer.Start();
            }
            catch (Exception ex)
            {
                log.Error("Error starting Trovesaurus server status and mail check timer", ex);
            }
        }

        public void StopStatusTimer()
        {
            _StatusTimer?.Stop();
        }

        private void CheckStatus()
        {
            try
            {
                if (MainWindowViewModel.Instance.Settings.TrovesaurusCheckMail) CheckMail();
                if (MainWindowViewModel.Instance.Settings.TrovesaurusServerStatus) RefreshServerStatus();
            }
            catch (Exception ex) { log.Error("Trovesaurus check status error", ex); }
        }

        private void RefreshServerStatus()
        {
            try
            {
                if (lastUpdated < DateTime.Now.AddSeconds(-25))
                {
                    lastUpdated = DateTime.Now;
                    ServerStatus = TrovesaurusApi.GetServerStatus();
                }
            }
            catch (Exception ex) { log.Error("Error refreshing server status", ex); }
        }

        private void CheckMail()
        {
            try
            {
                MailCount = TrovesaurusApi.GetMailCount();
                log.InfoFormat("Checked Trovesaurus mail: {0} new Trovesaurus mail message{1}", MailCount, MailCount == 1 ? "" : "s");
            }
            catch (Exception ex) { log.Error("Error checking Trovesaurus mail", ex); }
        }

        public ObservableCollection<TrovesaurusNewsItem> NewsItems { get; } = new ObservableCollection<TrovesaurusNewsItem>();

        public ObservableCollection<TrovesaurusCalendarItem> CalendarItems { get; } = new ObservableCollection<TrovesaurusCalendarItem>();

        public ObservableCollection<TrovesaurusOnlineStream> OnlineStreams { get; } = new ObservableCollection<TrovesaurusOnlineStream>();

        public ICollectionView NewsView
        {
            get { return _NewsView.View; }
        }

        public ICollectionView CalendarView
        {
            get { return _CalendarView.View; }
        }

        public ICollectionView StreamsView
        {
            get { return _StreamsView.View; }
        }

        private int _MailCount;
        public int MailCount
        {
            get { return _MailCount; }
            set
            {
                _MailCount = value;
                RaisePropertyChanged("MailCount");
                RaisePropertyChanged("MailCountDisplay");
            }
        }

        public string MailCountDisplay
        {
            get { return string.Format("{0} unread message{1}", MailCount, MailCount == 1 ? "" : "s"); }
        }

        private TroveServerStatus _ServerStatus;
        public TroveServerStatus ServerStatus
        {
            get
            {
                if (_ServerStatus == null) RefreshServerStatus();
                return _ServerStatus;
            }
            set
            {
                _ServerStatus = value;
                if (value != null) RaisePropertyChanged("ServerStatus");
            }
        }

        public DelegateCommand RefreshDataCommand
        {
            get
            {
                if (_RefreshDataCommand == null) _RefreshDataCommand = new DelegateCommand(LoadData);
                return _RefreshDataCommand;
            }
        }

        public DelegateCommand<string> LaunchTrovesaurusCommand
        {
            get
            {
                if (_LaunchTrovesaurusCommand == null) _LaunchTrovesaurusCommand = new DelegateCommand<string>(LaunchTrovesaurus);
                return _LaunchTrovesaurusCommand;
            }
        }

        public DelegateCommand<string> LaunchServerStatusCommand
        {
            get
            {
                if (_LaunchServerStatusCommand == null) _LaunchServerStatusCommand = new DelegateCommand<string>(p => LaunchTrovesaurus(TrovesaurusApi.ServerStatusPage));
                return _LaunchServerStatusCommand;
            }
        }

        public DelegateCommand<string> LaunchTrovesaurusMailCommand
        {
            get
            {
                if (_LaunchTrovesaurusMailCommand == null) _LaunchTrovesaurusMailCommand = new DelegateCommand<string>(p => LaunchTrovesaurus(TrovesaurusApi.MailboxUrl));
                return _LaunchTrovesaurusMailCommand;
            }
        }

        public DelegateCommand<string> LaunchTrovesaurusLivestreamsCommand
        {
            get
            {
                if (_LaunchTrovesaurusLivestreamsCommand == null) _LaunchTrovesaurusLivestreamsCommand = new DelegateCommand<string>(p => LaunchTrovesaurus(TrovesaurusApi.OnlineStreamsPageUrl));
                return _LaunchTrovesaurusLivestreamsCommand;
            }
        }

        public DelegateCommand<string> LaunchTrovesaurusNewCommand
        {
            get
            {
                if (_LaunchTrovesaurusNewCommand == null) _LaunchTrovesaurusNewCommand = new DelegateCommand<string>(p => LaunchTrovesaurus(TrovesaurusApi.NewsPageUrl));
                return _LaunchTrovesaurusNewCommand;
            }
        }

        public DelegateCommand<string> LaunchTrovesaurusNewsTagCommand
        {
            get
            {
                if (_LaunchTrovesaurusNewsTagCommand == null) _LaunchTrovesaurusNewsTagCommand = new DelegateCommand<string>(LaunchTrovesaurusNewsTag);
                return _LaunchTrovesaurusNewsTagCommand;
            }
        }

        public DelegateCommand<string> LaunchTrovesaurusCalendarCommand
        {
            get
            {
                if (_LaunchTrovesaurusCalendarCommand == null) _LaunchTrovesaurusCalendarCommand = new DelegateCommand<string>(p => LaunchTrovesaurus(TrovesaurusApi.CalendarPageUrl));
                return _LaunchTrovesaurusCalendarCommand;
            }
        }

        private void LaunchTrovesaurusNewsTag(string tag)
        {
            try { TrovesaurusApi.LaunchTrovesaurusNewsTag(tag); }
            catch (Exception ex) { log.Error("Error launching Trovesaurus news tag page", ex); }
        }

        private void LaunchTrovesaurus(string url = null)
        {
            try { TrovesaurusApi.LaunchTrovesaurus(url); }
            catch (Exception ex) { log.Error("Error launching Trovesaurus page", ex); }
        }
    }
}
