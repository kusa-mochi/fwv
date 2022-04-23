using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using fwv.Common;

namespace fwv.Models
{
    public class FileWatcher : IDisposable
    {
        public event ModifiedEventHandler Modified;

        public FileWatcher()
        {
        }

        ~FileWatcher()
        {
            Dispose();
        }

        public void AddDirectory(string hash, string directoryPath)
        {
            if (string.IsNullOrEmpty(hash)) return;
            if (string.IsNullOrEmpty(directoryPath)) return;

            IdentifiedFileSystemWatcher watcher = new IdentifiedFileSystemWatcher(directoryPath);
            watcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite;
            watcher.Hash = hash;
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;
            watcher.Changed += OnChanged;
            watcher.Created += OnCreated;
            watcher.Deleted += OnDeleted;
            watcher.Renamed += OnRenamed;
            _watchers.Add(hash, watcher);
        }

        public void RemoveDirectory(string hash)
        {
            if (string.IsNullOrEmpty(hash)) return;
            if (_watchers.Count == 0)
            {
                _logManager.AppendErrorLog("removing a directory watcher failed because there is no watcher.");
                _logManager.AppendLog($"hash: {hash}");
                return;
            }

            bool getResult = _watchers.TryGetValue(hash, out IdentifiedFileSystemWatcher watcher);
            if (getResult) watcher?.Dispose();

            bool removeResult = _watchers.Remove(hash);
            if (removeResult)
            {
                _logManager.AppendLog("a directory watcher was removed.");
                _logManager.AppendLog($"hash: {hash}");
            }
            else
            {
                _logManager.AppendErrorLog("removing a directory watcher failed.");
                _logManager.AppendErrorLog($"hash: {hash}");
            }
        }

        private bool IsIgnoreObject(string oldPath, string newPath)
        {
            bool ret = oldPath.Contains("\\.git") || newPath.Contains("\\.git");
            return ret;
        }

        private void OnModified(object sender, WatcherChangeTypes changeType, string oldPath, string newPath, string newName)
        {
            if (sender == null)
            {
                throw new ArgumentNullException("sender");
            }
            if (string.IsNullOrEmpty(oldPath))
            {
                throw new ArgumentNullException("oldPath");
            }
            if (string.IsNullOrEmpty(newPath))
            {
                throw new ArgumentNullException("newPath");
            }

            // if changes are in a ".git" folder.
            if (IsIgnoreObject(oldPath, newPath))
            {
                // do nothing.
                return;
            }

            switch (changeType)
            {
                case WatcherChangeTypes.Created:
                    _logManager.AppendLog($"\"{newPath}\" was created.");
                    break;
                case WatcherChangeTypes.Deleted:
                    _logManager.AppendLog($"\"{oldPath}\" was deleted.");
                    break;
                case WatcherChangeTypes.Changed:
                    _logManager.AppendLog($"\"{newPath}\" was changed.");
                    break;
                case WatcherChangeTypes.Renamed:
                    _logManager.AppendLog($"\"{oldPath}\" was renamed to \"{newPath}\".");
                    break;
                case WatcherChangeTypes.All:
                    break;
                default:
                    break;
            }

            IdentifiedFileSystemWatcher watcher = sender as IdentifiedFileSystemWatcher;
            Modified.Invoke(sender, new ModifiedEventArgs(newPath, newName, watcher.Hash));
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            OnModified(sender, WatcherChangeTypes.Changed, e.FullPath, e.FullPath, e.Name);
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            OnModified(sender, WatcherChangeTypes.Created, e.FullPath, e.FullPath, e.Name);
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            OnModified(sender, WatcherChangeTypes.Deleted, e.FullPath, e.FullPath, e.Name);
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            OnModified(sender, WatcherChangeTypes.Renamed, e.OldFullPath, e.FullPath, e.Name);
        }

        public void Dispose()
        {
            foreach (KeyValuePair<string, IdentifiedFileSystemWatcher> pair in _watchers)
            {
                IdentifiedFileSystemWatcher watcher = pair.Value;
                watcher?.Dispose();
            }
        }

        private Dictionary<string, IdentifiedFileSystemWatcher> _watchers = new Dictionary<string, IdentifiedFileSystemWatcher>();
        private LogManager _logManager = LogManager.GetInstance();
    }
}
