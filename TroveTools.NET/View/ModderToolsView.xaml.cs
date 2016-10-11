using System;
using System.Windows.Controls;
using TroveTools.NET.Properties;
using TroveTools.NET.ViewModel;
using log4net;
using Microsoft.Win32;
using System.IO;

namespace TroveTools.NET.View
{
    /// <summary>
    /// Interaction logic for ModderToolsView.xaml
    /// </summary>
    public partial class ModderToolsView : UserControl
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ModderToolsView()
        {
            InitializeComponent();
        }

        private void AddFileButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var vm = DataContext as ModderToolsViewModel;
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Title = Strings.MyMods_OpenFileDialog_Title;
                dialog.Filter = "All Files|*.*";
                dialog.CheckFileExists = true;
                dialog.Multiselect = true;
                dialog.InitialDirectory = vm.AddFileLocation;

                if (dialog.ShowDialog() == true)
                {
                    foreach (string file in dialog.FileNames)
                    {
                        vm.AddFileCommand.Execute(file);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error adding files", ex);
            }
        }

        private void PreviewBrowseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var vm = DataContext as ModderToolsViewModel;
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Title = Strings.MyMods_OpenFileDialog_Title;
                dialog.Filter = "400 by 230 pixel image (PNG / JPG) or blueprint file|*.png;*.jpg;*.blueprint";
                dialog.CheckFileExists = true;
                dialog.Multiselect = false;
                dialog.InitialDirectory = vm.PreviewLocation;

                if (dialog.ShowDialog() == true)
                {
                    vm.UpdatePreviewCommand.Execute(dialog.FileName);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error updating preview file", ex);
            }
        }
    }
}
