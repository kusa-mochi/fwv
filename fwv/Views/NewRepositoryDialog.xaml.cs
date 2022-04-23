using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;
using fwv.Common;
using fwv.ViewModels;

namespace fwv.Views
{
    /// <summary>
    /// Interaction logic for NewRepositoryDialog.xaml
    /// </summary>
    public partial class NewRepositoryDialog : UserControl
    {
        public NewRepositoryDialog()
        {
            _log.AppendLog("initializing..");
            InitializeComponent();
            _vm = new NewRepositoryDialogViewModel();
            this.DataContext = _vm;
            _log.AppendLog("initialized.");
        }

        private void SelectLocalBareRepository(object sender, RoutedEventArgs e)
        {
            using (CommonOpenFileDialog dlg = new CommonOpenFileDialog()
            {
                Title = "Choose a directory",
                IsFolderPicker = true,
                RestoreDirectory = true,
                Multiselect = false
            })
            {
                CommonFileDialogResult result = dlg.ShowDialog();
                if (result != CommonFileDialogResult.Ok) return;

                string dirPath = dlg.FileName;
                _vm.LocalBareRepositoryPath = dirPath;
            }
        }

        private void SelectWorkingDirectory(object sender, RoutedEventArgs e)
        {
            using (CommonOpenFileDialog dlg = new CommonOpenFileDialog()
            {
                Title = "Choose a directory",
                IsFolderPicker = true,
                RestoreDirectory = true,
                Multiselect = false
            })
            {
                CommonFileDialogResult result = dlg.ShowDialog();
                if (result != CommonFileDialogResult.Ok) return;

                string dirPath = dlg.FileName;
                _vm.WorkingDirectoryPath = dirPath;
            }
        }

        private NewRepositoryDialogViewModel _vm = null;
        private LogManager _log = LogManager.GetInstance();
    }
}
