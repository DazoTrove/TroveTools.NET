using System;
using System.Windows.Controls;
using TroveTools.NET.Properties;
using TroveTools.NET.ViewModel;
using log4net;
using System.IO;
using Ookii.Dialogs.Wpf;
using Microsoft.Win32;

namespace TroveTools.NET.View
{
    /// <summary>
    /// Interaction logic for ModderToolsView.xaml
    /// </summary>
    public partial class ModderToolsView : UserControl
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private ModderToolsViewModel vm = null;

        public ModderToolsView()
        {
            InitializeComponent();
        }

        internal ModderToolsViewModel ViewModel
        {
            get
            {
                if (vm == null) vm = DataContext as ModderToolsViewModel;
                return vm;
            }
        }

        private void AddFileButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Title = Strings.ModderTools_AddFileDialog_Title;
                dialog.Filter = "All Files|*.*";
                dialog.CheckFileExists = true;
                dialog.Multiselect = true;
                dialog.InitialDirectory = ViewModel.PrimaryLocationPath;

                if (dialog.ShowDialog() == true)
                {
                    foreach (string file in dialog.FileNames)
                    {
                        ViewModel.AddFileCommand.Execute(file);
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
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Title = Strings.ModderTools_PreviewDialog_Title;
                dialog.Filter = "400 by 230 pixel image (PNG / JPG) or blueprint file|*.png;*.jpg;*.blueprint";
                dialog.CheckFileExists = true;
                dialog.Multiselect = false;
                dialog.InitialDirectory = ViewModel.PreviewLocation;

                if (dialog.ShowDialog() == true)
                {
                    ViewModel.UpdatePreviewCommand.Execute(dialog.FileName);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error updating preview file", ex);
            }
        }

        private void LoadYamlButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Title = Strings.ModderTools_OpenYamlDialog_Title;
                dialog.Filter = "YAML file|*.yaml";
                dialog.CheckFileExists = true;
                dialog.Multiselect = false;
                dialog.InitialDirectory = ViewModel.ModsFolder;

                if (dialog.ShowDialog() == true)
                {
                    ViewModel.LoadYamlCommand.Execute(dialog.FileName);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error loading YAML file", ex);
            }
        }

        private void SaveYamlButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Title = Strings.ModderTools_SaveYamlDialog_Title;
                dialog.Filter = "YAML file|*.yaml";
                dialog.CheckFileExists = false;
                dialog.InitialDirectory = ViewModel.ModsFolder;
                dialog.FileName = Path.GetFileName(ViewModel.YamlPath);

                if (dialog.ShowDialog() == true)
                {
                    ViewModel.SaveYamlCommand.Execute(dialog.FileName);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error saving YAML file", ex);
            }
        }

        private void ExtractedFolderButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
                dialog.Description = Strings.ModderTools_ExtractFolderDialog_Title;
                dialog.UseDescriptionForTitle = true;
                dialog.ShowNewFolderButton = true;
                dialog.SelectedPath = ViewModel.ExtractedPath;

                if (dialog.ShowDialog() == true)
                {
                    ViewModel.ExtractedPath = dialog.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error selecting extract folder", ex);
            }
        }
    }
}
