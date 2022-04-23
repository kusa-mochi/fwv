using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace fwv.Common
{
    /// <summary>
    /// singleton class for logging app.
    /// to get an instance, call GetInstance() method.
    /// </summary>
    public class LogManager : IDisposable
    {
        public void AppendLog(string s)
        {
            AppendText(s, _logWriter);
        }

        public void AppendErrorLog(string s)
        {
            AppendText(s, _errorLogWriter);
        }

        private void AppendText(string s, StreamWriter sw)
        {
            DateTime now = DateTime.Now;
            StackFrame caller = new StackFrame(2, false);
            string callerTypeName = caller.GetMethod().DeclaringType.FullName;
            string callerMethodName = caller.GetMethod().Name;

            string logHeader = $"[{now:yyyy/MM/dd HH:mm:ss}][{callerTypeName}.{callerMethodName}]";
            sw.WriteLine($"{logHeader} {s}");
            sw.Flush();
        }

        private StreamWriter InitializeLogFile(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);

            // if the directory for the log files is not exist.
            if (!fileInfo.Directory.Exists)
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }

            // if the file is not exist,
            if (!File.Exists(filePath))
            {
                // create a new file.
                File.Create(filePath).Dispose();
            }

            return File.AppendText(filePath);
        }

        private void FinalizeLogFile(StreamWriter sw)
        {
            sw?.Dispose();
        }

        #region distribution of IDisposable

        public void Dispose()
        {
            FinalizeLogFile(_logWriter);
            FinalizeLogFile(_errorLogWriter);
        }

        #endregion

        #region Constructor

        /// <summary>
        /// static method for singleton pattern.
        /// </summary>
        /// <returns>LogManager instane</returns>
        public static LogManager GetInstance()
        {
            return _logManager;
        }

        private LogManager()
        {
            _logWriter = InitializeLogFile(_logFilePath);
            _errorLogWriter = InitializeLogFile(_errorLogFilePath);
        }

        #endregion

        #region Destructor

        ~LogManager()
        {
            _logManager?.Dispose();
        }

        #endregion

        private static LogManager _logManager = new LogManager();
        private string _logFilePath = "Log\\log.txt";
        private string _errorLogFilePath = "Log\\errorLog.txt";
        private StreamWriter _logWriter = null;
        private StreamWriter _errorLogWriter = null;
    }
}
