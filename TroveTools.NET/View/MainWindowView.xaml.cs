using log4net;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using TroveTools.NET.Properties;
using TroveTools.NET.ViewModel;

namespace TroveTools.NET.View
{
    /// <summary>
    /// Interaction logic for MainWindowView.xaml
    /// </summary>
    public partial class MainWindowView : Window
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private System.Windows.Forms.NotifyIcon trayIcon;
        private bool showBalloonTip = true;

        public MainWindowView()
        {
            InitializeComponent();

            trayIcon = new System.Windows.Forms.NotifyIcon();
            trayIcon.Visible = MainWindowViewModel.Instance.Settings.MinimizeToTray;

            // Wire up an event to watch for changes to the Minimize to Tray setting
            MainWindowViewModel.Instance.Settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "MinimizeToTray") trayIcon.Visible = MainWindowViewModel.Instance.Settings.MinimizeToTray;
            };

            trayIcon.Icon = new Icon(Application.GetResourceStream(new Uri("pack://application:,,,/Resources/TroveTools.ico")).Stream);
            trayIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            trayIcon.BalloonTipText = Strings.MainWindowView_MinimizeBalloonTipText;
            trayIcon.BalloonTipTitle = Strings.MainWindowView_MinimizeBalloonTipTitle;
            trayIcon.Text = Strings.MainWindowView_MinimizeBalloonTipTitle;

            trayIcon.Click += (s, e) => RestoreWindow();
            trayIcon.DoubleClick += (s, e) => RestoreWindow();
            trayIcon.BalloonTipClicked += (s, e) => RestoreWindow();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized) UpdateTrayIconMinimized();
            base.OnStateChanged(e);
        }

        private void RestoreWindow()
        {
            Show();
            WindowState = WindowState.Normal;
            ShowInTaskbar = true;
        }

        private void UpdateTrayIconMinimized()
        {
            if (MainWindowViewModel.Instance.Settings.MinimizeToTray)
            {
                Hide();
                trayIcon.Visible = true;
                ShowInTaskbar = false;
                if (showBalloonTip)
                {
                    trayIcon.ShowBalloonTip(5000);
                    showBalloonTip = false;
                }
            }
            else trayIcon.Visible = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (MainWindowViewModel.Instance.Settings.StartMinimized)
            {
                log.Info("Starting minimized");
                WindowState = WindowState.Minimized;
                UpdateTrayIconMinimized();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            trayIcon.Visible = false;
            Settings.Default.Save();
        }

        private void LogMessagesField_TextChanged(object sender, TextChangedEventArgs e)
        {
            RichTextBox rtb = sender as RichTextBox;
            rtb.ScrollToEnd();
        }

        public static void HideToolbarOverflow(ToolBar toolBar)
        {
            // Hide overflow button
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null) overflowGrid.Visibility = Visibility.Collapsed;

            var mainPanelBorder = toolBar.Template.FindName("MainPanelBorder", toolBar) as FrameworkElement;
            if (mainPanelBorder != null) mainPanelBorder.Margin = new Thickness(0);
        }
    }
}
