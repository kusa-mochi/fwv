using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

using fwv.Common;
using fwv.Models;

namespace fwv.ViewModels
{
    public class UserNameSettingViewModel : BindableBase, IDialogAware
    {
        #region Properties

        #region UserName

        private string _userName = string.Empty;
        public string UserName
        {
            get { return _userName; }
            set
            {
                SetProperty(ref _userName, value);
                OkCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #endregion

        #region Private Methods

        private string CreateDummyEmailAddress()
        {

            return HashGenerator.CreateRandomEmailAddress(64);
        }

        #endregion

        #region Commands

        private DelegateCommand _okCommand;
        public DelegateCommand OkCommand =>
            _okCommand ?? (_okCommand = new DelegateCommand(ExecuteOkCommand, CanExecuteOkCommand));
        void ExecuteOkCommand()
        {
            string emailAddress = CreateDummyEmailAddress();
            DialogResultParameters p = new DialogResultParameters();
            p.AddRange(new Dictionary<string, object>
            {
                { "UserName", UserName },
                { "EmailAddress", emailAddress }
            });
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK, p));
        }
        bool CanExecuteOkCommand()
        {
            bool isValidUserName = !string.IsNullOrWhiteSpace(UserName) && !Regex.IsMatch(UserName, @"\s");
            return isValidUserName;
        }

        #endregion

        #region Implementation of IDialogAware

        public string Title => "User Name Setting";

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
