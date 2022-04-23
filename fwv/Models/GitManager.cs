using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Timers;

using fwv.Common;
using fwv.Exceptions;

namespace fwv.Models
{
    /// <summary>
    /// singleton class to wrap git.exe.
    /// to get an instance, call GetInstance() method.
    /// </summary>
    internal class GitManager
    {
        #region Properties

        private bool _CanRunGitCommand = true;
        internal bool CanRunGitCommand
        {
            get
            {
                return _CanRunGitCommand;
            }
            set
            {
                _logManager.AppendLog($"set CanRunGitCommand {value}");
                _CanRunGitCommand = value;
            }
        }

        internal string WorkingDirectory { get; set; } = null;

        #endregion

        #region Internal Methods

        internal void EnqueueCommand(GitCommandItemBase command)
        {
            _gitCommandQueue.Enqueue(command);
            _logManager.AppendLog($"a git \"{command.Command.ToString()}\" command was enqueued.");
        }


        internal CommandOutput RunWindowsCommand(string command)
        {
            return RunCommand("cmd.exe", command);
        }

        /// <summary>
        /// test run for git.exe.
        /// this function run "git" command without any subcommands and options.
        /// the standard output and error are shown on messageboxes.
        /// if an error occured, an error message is shown on a messagebox.
        /// </summary>
        internal CommandOutput TestRun()
        {
            if (!CanRunGitCommand)
            {
                _logManager.AppendErrorLog("GitManager instance is busy now.");
                return new CommandOutput { StandardOutput = "", StandardError = "" };
            }

            _logManager.AppendLog("begin.");
            CommandOutput result;
            try
            {
                result = RunGitCommand();
                System.Windows.MessageBox.Show(result.StandardOutput);
                System.Windows.MessageBox.Show(result.StandardError);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("error occured !!!");
                System.Windows.MessageBox.Show(ex.Message);
                result = new CommandOutput { StandardError = ex.Message, StandardOutput = "" };
            }
            _logManager.AppendLog("fin.");
            return result;
        }

        internal CommandOutput Clone(string url, string directoryPath)
        {
            return RunGitCommand($"clone {url} \"{directoryPath}\"");
        }

        internal CommandOutput Add(string filter = "*")
        {
            return RunGitCommand($"add {filter}");
        }

        internal CommandOutput Commit(string message = "files were modified.")
        {
            if (string.IsNullOrEmpty(message))
            {
                message = "files were modified.";
            }
            return RunGitCommand($"commit -m \"{message}\"");
        }

        internal CommandOutput Push(string branch = "main")
        {
            return RunGitCommand($"push origin {branch}");
        }

        internal CommandOutput Init(bool isBare = false, string initialBranch = "main")
        {
            string args = isBare ? $" --bare --shared --initial-branch={initialBranch}" : "";
            return RunGitCommand($"init{args}");
        }

        internal CommandOutput GetUserName()
        {
            return RunGitCommand("config --global user.name");
        }

        internal CommandOutput GetEmailAddress()
        {
            return RunGitCommand("config --global user.email");
        }

        internal CommandOutput SetUserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return new CommandOutput
                {
                    StandardOutput = "",
                    StandardError = "invalid user name is input to GitManager."
                };
            }

            return RunGitCommand($"config --global user.name \"{userName}\"");
        }

        internal CommandOutput SetEmailAddress(string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
            {
                return new CommandOutput
                {
                    StandardOutput = "",
                    StandardError = "invalid email address is input to GitManager."
                };
            }

            return RunGitCommand($"config --global user.email \"{emailAddress}\"");
        }

        internal CommandOutput GetRemoteUrl()
        {
            return RunGitCommand("remote get-url origin");
        }

        internal CommandOutput Log(int maxNum = 0, bool nameOnly = false, string dateFormat = "%Y/%m/%d %H:%M:%S")
        {
            string command = "log";
            command += maxNum <= 0 ? "" : $" -n {maxNum}";
            command += nameOnly ? " --name-only" : "";
            command += $" --date=format:\"{dateFormat}\"";
            return RunGitCommand(command);
        }

        internal CommandOutput Fetch(string branch = "main", bool origin = true)
        {
            if (string.IsNullOrWhiteSpace(branch))
            {
                return new CommandOutput
                {
                    StandardOutput = "",
                    StandardError = "argument \"branch\" must not be null or empty."
                };
            }

            string command = "fetch";
            command += origin ? " origin" : "";
            command += $" {branch}";

            return RunGitCommand(command);
        }

        internal CommandOutput Merge(string branch = "main", bool origin = true, bool noCommit = false, bool noFf = false)
        {
            if (string.IsNullOrWhiteSpace(branch))
            {
                return new CommandOutput
                {
                    StandardOutput = "",
                    StandardError = "argument \"branch\" must not be null or empty."
                };
            }

            string targetBranch = $"{(origin ? "origin/" : "")}{branch}";

            string command = "merge";
            command += noCommit ? " --no-commit" : "";
            command += noFf ? " --no-ff" : "";
            command += $" {targetBranch}";
            return RunGitCommand(command);
        }

        internal CommandOutput Pull(string branch = "main", bool origin = true)
        {
            if (string.IsNullOrWhiteSpace(branch))
            {
                return new CommandOutput
                {
                    StandardOutput = "",
                    StandardError = "argument \"branch\" must not be null or empty."
                };
            }

            return Fetch(branch, origin) + Merge(branch, origin);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// run a git command.
        /// if you would like to run "git clone xxxxx yyyyy",
        /// you only have to call RunGitCommand("clone xxxxx yyyyy").
        /// </summary>
        /// <param name="gitArguments">git sub command and options</param>
        /// <returns>standard output and error from git.exe</returns>
        private CommandOutput RunGitCommand(string gitArguments = "")
        {
            return RunCommand(_exe, gitArguments);
        }

        private CommandOutput RunCommand(string fileName, string args)
        {
            _logManager.AppendLog($"runnig command.. \"{fileName} {args}\"");
            if (!CanRunGitCommand)
            {
                string logMessage = $"GitManager is busy now. command \"{fileName} {args}\" was not executed.";
                _logManager.AppendErrorLog(logMessage);
                return new CommandOutput { StandardOutput = "", StandardError = logMessage };
            }

            CanRunGitCommand = false;
            if (WorkingDirectory == null)
            {
                CanRunGitCommand = true;
                _logManager.AppendErrorLog("running command failed because the WorkingDirectory property is not set.");
                throw new InvalidOperationException("WorkingDirectory property must be set before running commands.");
            }

            var output = new StringBuilder();
            var error = new StringBuilder();

            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = args,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = WorkingDirectory
            };

            using (Process proc = Process.Start(startInfo))
            {
                proc.EnableRaisingEvents = true;

                proc.OutputDataReceived += (sender, e) =>
                {
                    output.AppendLine(e.Data);
                };
                proc.ErrorDataReceived += (sender, e) =>
                {
                    error.AppendLine(e.Data);
                };

                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                proc.WaitForExit();
                proc.CancelOutputRead();
                proc.CancelErrorRead();
            }

            _logManager.AppendLog(output.ToString());
            _logManager.AppendErrorLog(error.ToString());

            CanRunGitCommand = true;

            return new CommandOutput { StandardOutput = output.ToString(), StandardError = error.ToString() };
        }

        private void RunCommandQueue(object sender, ElapsedEventArgs e)
        {
            if (_gitCommandQueue.Count == 0)
            {
                _logManager.AppendLog("there is no command to execute in the queue.");
                CanRunGitCommand = true;
                return;
            }
            if (!CanRunGitCommand)
            {
                _logManager.AppendErrorLog("GitManager is busy now. Dequeue() was not called.");
                return;
            }

            while (_gitCommandQueue.Count > 0)
            {
                GitCommandItemBase queueItem = _gitCommandQueue.Dequeue();
                _logManager.AppendLog($"a git \"{queueItem.Command.ToString()}\" command was dequeued.");
                WorkingDirectory = queueItem.WorkingDirectory;
                switch (queueItem.Command)
                {
                    case GitCommand.Init:
                        {
                            GitInitCommandItem item = queueItem as GitInitCommandItem;
                            Init(item.IsBare);
                        }
                        break;
                    case GitCommand.Clone:
                        {
                            GitCloneCommandItem item = queueItem as GitCloneCommandItem;
                            Clone(item.RemoteUrl, item.WorkingDirectoryPath);
                        }
                        break;
                    case GitCommand.Add:
                        {
                            GitAddCommandItem item = queueItem as GitAddCommandItem;
                            Add();
                        }
                        break;
                    case GitCommand.Commit:
                        {
                            GitCommitCommandItem item = queueItem as GitCommitCommandItem;
                            Commit();
                        }
                        break;
                    case GitCommand.Push:
                        {
                            GitPushCommandItem item = queueItem as GitPushCommandItem;
                            Push();
                        }
                        break;
                    case GitCommand.Pull:
                        throw new NotImplementedException();
                    default:
                        {
                            string errorMessage = "invalid git command in queue.";
                            _logManager.AppendErrorLog(errorMessage);
                            throw new InvalidOperationException(errorMessage);
                        }
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// static method for singleton pattern.
        /// </summary>
        /// <returns>GitManager instance</returns>
        internal static GitManager GetInstance()
        {
            return _gitManager;
        }

        private GitManager()
        {
            // start a timer to check git command queue at the intervals.
            Timer timer = new Timer
            {
                Interval = Properties.Settings.Default.WatchInterval,
                AutoReset = true,
                Enabled = true
            };
            timer.Elapsed += RunCommandQueue;
            timer.Start();
        }

        #endregion

        #region Fields

        private static GitManager _gitManager = new GitManager();
        private string _exe = "git";
        private LogManager _logManager = LogManager.GetInstance();
        private Queue<GitCommandItemBase> _gitCommandQueue = new Queue<GitCommandItemBase>();

        #endregion
    }
}
