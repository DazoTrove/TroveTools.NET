using log4net;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
//using System.Windows.Forms;
using TroveTools.NET.Framework;
using TroveTools.NET.Properties;
using TroveTools.NET.ViewModel;

using NotifyIcon = System.Windows.Forms.NotifyIcon;
using ToolTipIcon = System.Windows.Forms.ToolTipIcon;
using MouseButtons = System.Windows.Forms.MouseButtons;
using ContextMenuStrip = System.Windows.Forms.ContextMenuStrip;
using ToolStripItem = System.Windows.Forms.ToolStripItem;
using ToolStripMenuItem = System.Windows.Forms.ToolStripMenuItem;
using ToolStripSeparator = System.Windows.Forms.ToolStripSeparator;
using System.Windows.Data;

/*using Application = System.Windows.Application;
using RichTextBox = System.Windows.Controls.RichTextBox;
using ToolBar = System.Windows.Controls.ToolBar;
using Button = System.Windows.Controls.Button;*/

namespace TroveTools.NET.View
{
    /// <summary>
    /// Interaction logic for MainWindowView.xaml
    /// </summary>
    public partial class MainWindowView : Window
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const int TipTimeout = 5000;

        private System.Windows.Forms.NotifyIcon trayIcon;
        private Action tipAction = null;
        private bool showBalloonTip = true;
        private bool forceClose = false;

        static MainWindowView()
        {
            ViewCommandManager.SetupMetadata<MainWindowView>();
        }

        public MainWindowView()
        {
            InitializeComponent();

            trayIcon = new NotifyIcon();
            trayIcon.Visible = MainWindowViewModel.Instance.Settings.MinimizeToTray;

            // Wire up an event to watch for changes to the Minimize to Tray setting
            MainWindowViewModel.Instance.Settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "MinimizeToTray") trayIcon.Visible = MainWindowViewModel.Instance.Settings.MinimizeToTray;
            };

            var icon = new Icon(Application.GetResourceStream(new Uri("pack://application:,,,/Resources/TroveTools.ico")).Stream);
            trayIcon.Icon = icon;
            trayIcon.BalloonTipIcon = ToolTipIcon.Info;
            trayIcon.BalloonTipText = Strings.MainWindowView_MinimizeBalloonTipText;
            trayIcon.BalloonTipTitle = Strings.MainWindowView_MinimizeBalloonTipTitle;
            trayIcon.Text = Strings.MainWindowView_MinimizeBalloonTipTitle;

            tipAction = RestoreWindow;
            trayIcon.BalloonTipClicked += (s, e) => tipAction?.Invoke();
            trayIcon.MouseClick += (s, e) => { if (e.Button == MouseButtons.Left) RestoreWindow(); };

            // Setup tray icon context menu
            var open = new ToolStripMenuItem(Strings.MainWindowView_OpenTroveTools, icon.ToBitmap());
            open.Font = new Font(open.Font, System.Drawing.FontStyle.Bold);
            open.Click += (s, e) => RestoreWindow();

            var quitImage = System.Drawing.Image.FromStream(Application.GetResourceStream(new Uri("pack://application:,,,/Resources/glyphicons-208-remove.png")).Stream);
            var quit = new ToolStripMenuItem(Strings.MainWindowView_QuitTroveTools, quitImage);
            quit.Click += QuitTroveTools;

            var menu = new ContextMenuStrip();
            menu.Items.AddRange(new ToolStripItem[] { open, new ToolStripSeparator(), quit });

            trayIcon.ContextMenuStrip = menu;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized) UpdateTrayIconMinimized();
            if (WindowState == WindowState.Normal) RestoreWindow();
            base.OnStateChanged(e);
        }

        [ViewCommand]
        public void RestoreWindow()
        {
            WindowState = WindowState.Normal;
            Activate();
            ShowInTaskbar = true;
        }

        [ViewCommand]
        public void ShowTrayTip(string balloonTipText, Action action = null)
        {
            if (action != null) tipAction = action;
            trayIcon.ShowBalloonTip(TipTimeout, trayIcon.BalloonTipTitle, balloonTipText, trayIcon.BalloonTipIcon);
        }

        private void QuitTroveTools(object sender = null, EventArgs e = null)
        {
            forceClose = true;
            Close();
        }

        private void UpdateTrayIconMinimized()
        {
            if (MainWindowViewModel.Instance.Settings.MinimizeToTray)
            {
                trayIcon.Visible = true;
                ShowInTaskbar = false;
                if (showBalloonTip)
                {
                    ShowTrayTip(Strings.MainWindowView_MinimizeBalloonTipText, RestoreWindow);
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

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Settings.Default.Save();
            if (trayIcon.Visible && forceClose == false)
            {
                e.Cancel = true;
                WindowState = WindowState.Minimized;
            }
            else
            {
                trayIcon.Visible = false;
                if (MainWindowViewModel.Instance.ClosingCommand.CanExecute(null)) MainWindowViewModel.Instance.ClosingCommand.Execute(null);
            }
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

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.ContextMenu.IsEnabled = true;
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.Placement = PlacementMode.Bottom;
            button.ContextMenu.IsOpen = true;
        }
    }
}
