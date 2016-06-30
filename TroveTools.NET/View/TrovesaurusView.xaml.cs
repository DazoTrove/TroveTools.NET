using System.Windows;
using System.Windows.Controls;

namespace TroveTools.NET.View
{
    /// <summary>
    /// Interaction logic for TrovesaurusView.xaml
    /// </summary>
    public partial class TrovesaurusView : UserControl
    {
        public TrovesaurusView()
        {
            InitializeComponent();
        }

        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindowView.HideToolbarOverflow(sender as ToolBar);
        }
    }
}
