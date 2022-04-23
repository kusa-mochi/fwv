using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

using fwv.Common;
using fwv.Models;

namespace fwv.ViewModels
{
    /// <summary>
    /// Derived class ObservableCollection<RepositoryListItem> with serialization method.
    /// </summary>
    public class RepositoryCollection : ObservableCollection<RepositoryListItem>
    {
        public RepositoryCollection(string str = "")
        {
            _log.AppendLog("begin");

            Deserialize(str);

            _log.AppendLog("end");
        }

        private void Deserialize(string str = "")
        {
            _log.AppendLog("begin");

            if (str == null) throw new ArgumentNullException("str");
            ClearItems();

            using (StringReader stringReader = new StringReader(str))
            {
                string line = "";
                while ((line = stringReader.ReadLine()) != null)
                {
                    string[] data = line.Split(_dataSeparator);
                    RepositoryListItem item = new RepositoryListItem
                    {
                        IsModified = false,
                        RepositoryUrl = data[0],
                        LocalDirectoryPath = data[1]
                    };

                    Add(item);
                }
            }

            _log.AppendLog("end");
        }

        public string Serialize()
        {
            _log.AppendLog("begin");

            StringBuilder stringBuilder = new StringBuilder();
            foreach (RepositoryListItem item in this)
            {
                // If there is no data in item,
                if (
                    item == null ||
                    string.IsNullOrWhiteSpace(item.RepositoryUrl) ||
                    string.IsNullOrWhiteSpace(item.LocalDirectoryPath)
                    )
                {
                    _log.AppendErrorLog("there is no data in item of the RepositoryListItem.");
                    continue;
                }

                stringBuilder.AppendLine($"{item.RepositoryUrl}{_dataSeparator}{item.LocalDirectoryPath}");
            }

            _log.AppendLog("end");

            return stringBuilder.ToString();
        }

        private LogManager _log = LogManager.GetInstance();
        private string _dataSeparator = "|";
    }
}
