using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;

using fwv.Common;
using fwv.Models;

namespace fwv.ViewModels
{
    public class RepositoryListViewModel : BindableBase
    {
        #region Fields

        private IRegionManager _regionManager = null;
        private IDialogService _dialogService = null;
        private GitManager _git = GitManager.GetInstance();
        private FileWatcher _fileWatcher = new FileWatcher();
        private LogManager _log = LogManager.GetInstance();

        #endregion

        #region Properties

        #region UserName

        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set
            {
                SetProperty(ref _userName, value);
                TopMessage = $"Hi, {_userName}";
            }
        }

        #endregion

        #region EmailAddress

        private string _emailAddress;
        public string EmailAddress
        {
            get { return _emailAddress; }
            set { SetProperty(ref _emailAddress, value); }
        }

        #endregion

        #region TopMessage

        private string _topMessage;
        public string TopMessage
        {
            get { return _topMessage; }
            set { SetProperty(ref _topMessage, value); }
        }

        #endregion

        #region Repositories

        private RepositoryCollection _repositories = new RepositoryCollection();
        public RepositoryCollection Repositories
        {
            get { return _repositories; }
            set { SetProperty(ref _repositories, value); }
        }

        #endregion

        #region ActiveItem

        private RepositoryListItem _activeItem = null;
        public RepositoryListItem ActiveItem
        {
            get { return _activeItem; }
            set { SetProperty(ref _activeItem, value); }
        }

        #endregion

        #endregion

        #region Commands

        #region OnLoadedCommand

        private DelegateCommand _onLoadedCommand;
        public DelegateCommand OnLoadedCommand =>
            _onLoadedCommand ?? (_onLoadedCommand = new DelegateCommand(ExecuteOnLoadedCommand));

        void ExecuteOnLoadedCommand()
        {
            ValidateUserName();
            LoadSettings();
            UpdateLocalFiles();
            StartWatching();
        }

        #endregion

        #region CreateRepositoryCommand

        private DelegateCommand _CreateRepositoryCommand;
        public DelegateCommand CreateRepositoryCommand =>
            _CreateRepositoryCommand ?? (_CreateRepositoryCommand = new DelegateCommand(ExecuteCreateRepositoryCommand));
        void ExecuteCreateRepositoryCommand()
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
                _git.WorkingDirectory = dirPath;
                _git.Init(true);
            }
        }

        #endregion

        #region OpenNewRepositoryDialogCommand

        private DelegateCommand _openNewRepositoryDialogCommand;
        public DelegateCommand OpenNewRepositoryDialogCommand =>
            _openNewRepositoryDialogCommand ?? (_openNewRepositoryDialogCommand = new DelegateCommand(ExecuteOpenNewRepositoryDialogCommand));
        void ExecuteOpenNewRepositoryDialogCommand()
        {
            _dialogService.ShowDialog(typeof(fwv.Views.NewRepositoryDialog).Name, result =>
            {
                IDialogParameters p = result.Parameters;

                switch (result.Result)
                {
                    case ButtonResult.OK:
                        Enum.TryParse(p.GetValue<string>("RepositoryPlace"), out RepositoryPlace repositoryPlace);
                        string repositoryUrl = repositoryPlace switch
                        {
                            RepositoryPlace.Remote => p.GetValue<string>("RemoteRepositoryUrl"),
                            RepositoryPlace.Local => p.GetValue<string>("LocalBareRepositoryPath"),
                            _ => throw new Exception("invalid result param \"RepositoryPlace\"")
                        };
                        string workingDirectory = p.GetValue<string>("WorkingDirectoryPath");

                        // TODO: check if the destination directory is empty.

                        _git.WorkingDirectory = workingDirectory;
                        CommandOutput gitResult = _git.Clone(repositoryUrl, workingDirectory);

                        // TODO: if cloning is done successfully.

                        RepositoryListItem newItem = new RepositoryListItem
                        {
                            IsModified = false,
                            RepositoryUrl = repositoryUrl,
                            LocalDirectoryPath = workingDirectory
                        };

                        Repositories.Add(newItem);
                        SaveSettings();

                        // start watching.
                        _fileWatcher.AddDirectory(newItem.Hash, newItem.LocalDirectoryPath);
                        break;
                    case ButtonResult.Cancel:
                        break;
                    default:
                        break;
                }
            });
        }

        #endregion

        #region RemoveRepositoryCommand

        private DelegateCommand<string> _removeRepositoryCommand;
        public DelegateCommand<string> RemoveRepositoryCommand =>
            _removeRepositoryCommand ?? (_removeRepositoryCommand = new DelegateCommand<string>(ExecuteRemoveRepositoryCommand));
        void ExecuteRemoveRepositoryCommand(string parameter)
        {
            foreach (RepositoryListItem item in Repositories)
            {
                if (parameter == item.Hash)
                {
                    // TODO: remove local directories/files.

                    Repositories.Remove(item);
                    SaveSettings();
                    break;
                }
            }
        }

        #endregion

        #region OpenHistoryDialogCommand

        private DelegateCommand<string> _OpenHistoryDialogCommand;
        public DelegateCommand<string> OpenHistoryDialogCommand =>
            _OpenHistoryDialogCommand ?? (_OpenHistoryDialogCommand = new DelegateCommand<string>(ExecuteOpenHistoryDialogCommand));
        void ExecuteOpenHistoryDialogCommand(string commandParam)
        {
            DialogInputParameters dialogParams = new DialogInputParameters();
            dialogParams.AddRange(
                new Dictionary<string, object>
                {
                    { "WorkingDirectory", commandParam }
                }
                );
            _dialogService.ShowDialog(typeof(fwv.Views.HistoryDialog).Name, dialogParams, result =>
            {
            });
        }

        #endregion

        #region AddRepositoryCommand

        private DelegateCommand _addRepositoryCommand;
        public DelegateCommand AddRepositoryCommand =>
            _addRepositoryCommand ?? (_addRepositoryCommand = new DelegateCommand(ExecuteAddRepositoryCommand));
        void ExecuteAddRepositoryCommand()
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
                dirPath = TrimEndRepositoryUrl(dirPath);

                _git.WorkingDirectory = dirPath;
                CommandOutput commandOutput = _git.GetRemoteUrl();
                if (!string.IsNullOrWhiteSpace(commandOutput.StandardError))
                {
                    _log.AppendErrorLog(commandOutput.StandardError);
                    System.Windows.MessageBox.Show("Error: This is not a Git repository.");
                    return;
                }
                string remoteUrl = commandOutput.StandardOutput;

                RepositoryListItem newItem = new RepositoryListItem
                {
                    IsModified = false,
                    RepositoryUrl = remoteUrl,
                    LocalDirectoryPath = dirPath
                };

                Repositories.Add(newItem);
                SaveSettings();

                // start watching.
                _fileWatcher.AddDirectory(newItem.Hash, newItem.LocalDirectoryPath);
            }
        }

        #endregion

        #endregion

        #region Private Methods

        private void OnFilesModified(object sender, ModifiedEventArgs args)
        {
            // a hash to recognize which directory is modified.
            string watcherHash = args.WatcherHash;

            string workingDirectory = "";
            foreach (RepositoryListItem item in Repositories)
            {
                if (item.Hash == watcherHash)
                {
                    workingDirectory = item.LocalDirectoryPath;
                }
            }

            _git.EnqueueCommand(new GitAddCommandItem(workingDirectory));
            _git.EnqueueCommand(new GitCommitCommandItem(workingDirectory));
            _git.EnqueueCommand(new GitPushCommandItem(workingDirectory));
        }

        private string TrimEndRepositoryUrl(string url)
        {
            string output = string.Empty;
            string urlEnd = url.Substring(url.Length - 4);
            output = urlEnd == ".git" ? url.Substring(0, url.Length - 4) : url;

            return output;
        }

        /// <summary>
        /// Validate current user name.
        /// If no name is registered, the dialog is shown to guide user to register its name.
        /// </summary>
        private void ValidateUserName()
        {
            _log.AppendLog("executing..");

            _git.WorkingDirectory = string.Empty;
            string currentUserName = _git.GetUserName().StandardOutput;
            string currentEmailAddress = _git.GetEmailAddress().StandardOutput;

            // if a user name is not set to git global setting,
            if (string.IsNullOrEmpty(currentUserName) || string.IsNullOrEmpty(currentEmailAddress))
            {
                _log.AppendLog("user config is not registered yet.");

                // show a dialog for setting user name.
                _dialogService.ShowDialog(typeof(fwv.Views.UserNameSetting).Name, result =>
                {
                    IDialogParameters p = result.Parameters;

                    switch (result.Result)
                    {
                        case ButtonResult.OK:
                            {
                                string userName = result.Parameters.GetValue<string>("UserName");
                                string emailAddress = result.Parameters.GetValue<string>("EmailAddress");
                                _git.SetUserName(userName);
                                _git.SetEmailAddress(emailAddress);
                                UserName = userName;
                                EmailAddress = emailAddress;
                                break;
                            }
                        default:
                            {
                                fwv.App.Current.Shutdown(1);
                                break;
                            }
                    }
                });
            }
            else
            {
                _log.AppendLog($"user config is already registered: {currentUserName}, {currentEmailAddress}");
                UserName = currentUserName;
                EmailAddress = currentEmailAddress;
            }

            _log.AppendLog("executed.");
        }

        private void LoadSettings()
        {
            // load user settings.
            string loadData = Properties.Settings.Default.Repositories;
            loadData = loadData.Replace("\r", "").Replace("\n", "");
            var tmpCollection = new RepositoryCollection(loadData);
            Repositories.Clear();
            foreach (var item in tmpCollection)
            {
                var listItem = new RepositoryListItem
                {
                    IsModified = false,
                    RepositoryUrl = item.RepositoryUrl,
                    LocalDirectoryPath = item.LocalDirectoryPath
                };

                Repositories.Add(listItem);

                // start watching.
                this._fileWatcher.AddDirectory(listItem.Hash, listItem.LocalDirectoryPath);
            }
        }

        private void SaveSettings()
        {
            string saveData = Repositories.Serialize();

            // save user data: repositories list.
            Properties.Settings.Default.Repositories = saveData;
            Properties.Settings.Default.Save();
        }

        private void UpdateLocalFiles()
        {
            // TODO: pull on all regitered git repositories.
        }

        private void StartWatching()
        {
            // TODO: start file watching for all registered local git repositories.
        }

        #endregion

        #region Constructors

        public RepositoryListViewModel(IRegionManager regionManager, IDialogService dialogService)
        {
            _log.AppendLog("initializing..");
            this._regionManager = regionManager;
            this._dialogService = dialogService;
            this._fileWatcher.Modified += OnFilesModified;
            _log.AppendLog("initialized.");
        }

        #endregion

        #region Destructor

        ~RepositoryListViewModel()
        {
        }

        #endregion
    }
}