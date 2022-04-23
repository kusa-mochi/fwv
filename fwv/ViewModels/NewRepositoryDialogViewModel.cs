using System;
using System.Collections.Generic;
using System.Text;

using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

using fwv.Common;

namespace fwv.ViewModels
{
    public class NewRepositoryDialogViewModel : BindableBase, IDialogAware
    {
        #region Properties

        private RepositoryPlace _repositoryPlace = RepositoryPlace.Remote;
        public RepositoryPlace RepositoryPlace
        {
            get { return _repositoryPlace; }
            set
            {
                switch (value)
                {
                    case RepositoryPlace.Remote:
                        IsRemoteEnabled = true;
                        IsLocalEnabled = false;
                        break;
                    case RepositoryPlace.Local:
                        IsRemoteEnabled = false;
                        IsLocalEnabled = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("value");
                }
                SetProperty(ref _repositoryPlace, value);
            }
        }

        private bool _isRemoteEnabled = true;
        public bool IsRemoteEnabled
        {
            get { return _isRemoteEnabled; }
            set { SetProperty(ref _isRemoteEnabled, value); }
        }

        private bool _isLocalEnabled = false;
        public bool IsLocalEnabled
        {
            get { return _isLocalEnabled; }
            set { SetProperty(ref _isLocalEnabled, value); }
        }

        private string _remoteRepositoryUrl = "";
        public string RemoteRepositoryUrl
        {
            get { return _remoteRepositoryUrl; }
            set { SetProperty(ref _remoteRepositoryUrl, value); }
        }

        private string _localBareRepositoryPath = "";
        public string LocalBareRepositoryPath
        {
            get { return _localBareRepositoryPath; }
            set { SetProperty(ref _localBareRepositoryPath, value); }
        }

        private string _workingDirectoryPath = "";
        public string WorkingDirectoryPath
        {
            get { return _workingDirectoryPath; }
            set { SetProperty(ref _workingDirectoryPath, value); }
        }

        #endregion

        #region Commands

        #region OkCommand

        private DelegateCommand _okCommand;
        public DelegateCommand OkCommand =>
            _okCommand ?? (_okCommand = new DelegateCommand(ExecuteOkCommand));

        void ExecuteOkCommand()
        {
            DialogResultParameters p = new DialogResultParameters();
            p.AddRange(new Dictionary<string, object> {
                { "RepositoryPlace", RepositoryPlace.ToString() },
                { "RemoteRepositoryUrl", RemoteRepositoryUrl },
                { "LocalBareRepositoryPath", LocalBareRepositoryPath },
                { "WorkingDirectoryPath", WorkingDirectoryPath }
            });
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK, p));
        }

        #endregion

        #region CancelCommand

        private DelegateCommand _cancelCommand;
        public DelegateCommand CancelCommand =>
            _cancelCommand ?? (_cancelCommand = new DelegateCommand(ExecuteCancelCommand));

        void ExecuteCancelCommand()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
        }

        #endregion

        #endregion

        #region Implementation of IDialogAware

        public string Title => "New Folder Settings";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }

        #endregion
    }
}
