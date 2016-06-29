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

            trayIcon.Click += (s, e) =>
            {
                Show();
                WindowState = WindowState.Normal;
                ShowInTaskbar = true;
            };

            if (MainWindowViewModel.Instance.Settings.StartMinimized)
            {
                WindowState = WindowState.Minimized;
                UpdateTrayIconMinimized();
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized) UpdateTrayIconMinimized();
            base.OnStateChanged(e);
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            trayIcon.Visible = false;
        }

        private void LogMessagesField_TextChanged(object sender, TextChangedEventArgs e)
        {
            RichTextBox rtb = sender as RichTextBox;
            rtb.ScrollToEnd();
        }
    }
}
