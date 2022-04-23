using System.Windows;
using MaterialDesignExtensions.Controls;

using fwv.Common;

namespace fwv.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MaterialWindow
    {
        public MainWindow()
        {
            _log.AppendLog("initializing..");
            InitializeComponent();
            _log.AppendLog("initialized.");
        }

        private LogManager _log = LogManager.GetInstance();
    }
}
