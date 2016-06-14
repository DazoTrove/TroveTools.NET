using log4net;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using TroveTools.NET.Properties;
using TroveTools.NET.ViewModel;

namespace TroveTools.NET.View
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SettingsView()
        {
            InitializeComponent();
        }

        private void btnAddLocation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Title = Strings.Settings_AddLocationDialog_Title;
                dialog.Filter = string.Format("{0}|Trove.exe", Strings.Settings_AddLocationDialog_Filter);
                dialog.CheckFileExists = true;

                if (dialog.ShowDialog() == true)
                {
                    var vm = DataContext as SettingsViewModel;
                    vm.AddLocationCommand.Execute(Path.GetDirectoryName(dialog.FileName));
                }
            }
            catch (Exception ex)
            {
                log.Error("Error adding location", ex);
            }
        }
    }
}
