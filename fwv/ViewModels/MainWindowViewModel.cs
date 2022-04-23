using Prism.Mvvm;
using Prism.Regions;

using fwv.Common;
using fwv.Models;

namespace fwv.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        public IRegionManager RegionManager { get; }

        public MainWindowViewModel(IRegionManager regionManager)
        {
            _log.AppendLog("initializing..");

            this.RegionManager = regionManager;

            // initialize content region.
            this.RegionManager.RegisterViewWithRegion("ContentRegion", typeof(fwv.Views.RepositoryList));

            _log.AppendLog("initialized.");
        }

        public void NavigateTo(string viewName)
        {
            this.RegionManager.RequestNavigate("ContentRegion", viewName);
        }

        private LogManager _log = LogManager.GetInstance();
    }
}
