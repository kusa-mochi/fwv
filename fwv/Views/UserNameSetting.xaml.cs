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

using fwv.Common;

namespace fwv.Views
{
    /// <summary>
    /// Interaction logic for UserNameSetting.xaml
    /// </summary>
    public partial class UserNameSetting : UserControl
    {
        public UserNameSetting()
        {
            _log.AppendLog("initializing..");
            InitializeComponent();
            _log.AppendLog("initialized.");
        }

        private LogManager _log = LogManager.GetInstance();
    }
}
