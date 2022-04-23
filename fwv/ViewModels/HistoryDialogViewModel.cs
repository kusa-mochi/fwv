using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using Prism.Mvvm;
using Prism.Services.Dialogs;

using fwv.Common;
using fwv.Models;

namespace fwv.ViewModels
{
    public class HistoryDialogViewModel : BindableBase, IDialogAware
    {
        #region Properties

        private ObservableCollection<HistoryListItem> _histories = new ObservableCollection<HistoryListItem>();
        public ObservableCollection<HistoryListItem> Histories
        {
            get { return _histories; }
            set { SetProperty(ref _histories, value); }
        }

        #endregion

        #region Private Methods

        private List<HistoryListItem> ConvertLogToHistoryList(string logString)
        {
            List<HistoryListItem> output = new List<HistoryListItem>();

            using (StringReader reader = new StringReader(logString))
            {
                _readingState = NextReadingHistoryState.Commit;
                string line = "";
                Regex commitRegex = new Regex(@"commit\s+(?<id>.+)");
                Regex mergeRegex = new Regex(@"Merge:\s*(.*)");
                Regex authorRegex = new Regex(@"Author:\s*(?<name>[^\s]+)\s*.*");
                Regex dateRegex = new Regex(@"Date:\s*(?<date>.+)");
                Match match = null;
                string commitId = null;
                string authorName = null;
                DateTime date = DateTime.MinValue;
                List<string> fileList = null;
                bool isMergeCommit = false;

                while (reader.Peek() > -1)
                {
                    line = reader.ReadLine();
                    switch (_readingState)
                    {
                        case NextReadingHistoryState.None:
                            // do nothing.
                            break;
                        case NextReadingHistoryState.Commit:
                            {
                                match = commitRegex.Match(line);
                                commitId = match.Groups["id"].Value;
                                _readingState = NextReadingHistoryState.Merge;
                                break;
                            }
                        case NextReadingHistoryState.Merge:
                            {
                                match = mergeRegex.Match(line);
                                isMergeCommit = match.Success;
                                if (match.Success)
                                {
                                    _readingState = NextReadingHistoryState.Author;
                                    break;
                                }

                                match = authorRegex.Match(line);
                                authorName = match.Groups["name"].Value;
                                _readingState = NextReadingHistoryState.Date;
                                break;
                            }
                        case NextReadingHistoryState.Author:
                            {
                                match = authorRegex.Match(line);
                                authorName = match.Groups["name"].Value;
                                _readingState = NextReadingHistoryState.Date;
                                break;
                            }
                        case NextReadingHistoryState.Date:
                            {
                                match = dateRegex.Match(line);
                                string dateString = match.Groups["date"].Value;
                                date = DateTime.ParseExact(dateString, "yyyy/MM/dd HH:mm:ss", null);

                                // skip one empty line.
                                reader.ReadLine();

                                _readingState = NextReadingHistoryState.CommitMessage;
                                break;
                            }
                        case NextReadingHistoryState.CommitMessage:
                            {
                                for (string message = line; !string.IsNullOrEmpty(message); message = reader.ReadLine()) ;

                                _readingState = isMergeCommit ? NextReadingHistoryState.Commit : NextReadingHistoryState.FileList;
                                break;
                            }
                        case NextReadingHistoryState.FileList:
                            {

                                fileList = new List<string>();

                                for (string filePath = line; !string.IsNullOrWhiteSpace(filePath); filePath = reader.ReadLine())
                                {
                                    fileList.Add(filePath);
                                }

                                Histories.Add(new HistoryListItem
                                {
                                    TimeStamp = date,
                                    AuthorName = authorName,
                                    ModifiedObjects = new ObservableCollection<string>(fileList)
                                });

                                _readingState = NextReadingHistoryState.Commit;
                                break;
                            }
                        default:
                            {
                                string errorMessage = $"invalid reading history state: {_readingState}";
                                _log.AppendErrorLog(errorMessage);
                                throw new InvalidOperationException(errorMessage);
                            }
                    }

                    if (_readingState == NextReadingHistoryState.None) break;
                }
            }

            return output;
        }

        #endregion

        #region Constructor

        public HistoryDialogViewModel()
        {
        }

        #endregion

        #region Fields

        private GitManager _git = GitManager.GetInstance();
        private LogManager _log = LogManager.GetInstance();

        private enum NextReadingHistoryState
        {
            None,
            Commit,
            Merge,
            Author,
            Date,
            CommitMessage,
            FileList
        }
        private NextReadingHistoryState _readingState = NextReadingHistoryState.Commit;

        #endregion

        #region Implementation of IDialogAware

        public string Title => "History";

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
            _log.AppendLog("initializing..");

            DialogInputParameters dialogParams = parameters as DialogInputParameters;

            _git.WorkingDirectory = dialogParams.GetValue<string>("WorkingDirectory");
            CommandOutput rawGitLog = _git.Log(100, true);
            if (!string.IsNullOrWhiteSpace(rawGitLog.StandardError))
            {
                return;
            }

            List<HistoryListItem> historyList = ConvertLogToHistoryList(rawGitLog.StandardOutput);
            Histories.AddRange(historyList);

            _log.AppendLog("initialized.");
        }

        #endregion
    }
}
