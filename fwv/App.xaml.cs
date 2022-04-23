using System.Windows;
using Prism.Ioc;
using fwv.Views;
using fwv.ViewModels;

namespace fwv
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry reg)
        {
            reg.RegisterForNavigation<RepositoryList>();
            reg.RegisterDialog<UserNameSetting, UserNameSettingViewModel>();
            reg.RegisterDialog<NewRepositoryDialog, NewRepositoryDialogViewModel>();
            reg.RegisterDialog<HistoryDialog, HistoryDialogViewModel>();
        }
    }
}
