using System.Windows;
using System.Windows.Controls;
using TroveTools.NET.ViewModel;

namespace TroveTools.NET.View
{
    /// <summary>
    /// Interaction logic for GetMoreModsView.xaml
    /// </summary>
    public partial class GetMoreModsView : UserControl
    {
        public GetMoreModsView()
        {
            InitializeComponent();
        }

        public void ListBoxItem_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Control control = sender as Control;
            TroveModViewModel vm = control.DataContext as TroveModViewModel;
            vm.InstallCommand.Execute(null);
        }
        
        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindowView.HideToolbarOverflow(sender as ToolBar);
        }
    }
}
