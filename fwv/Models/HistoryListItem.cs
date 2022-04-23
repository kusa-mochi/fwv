using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace fwv.Models
{
    public class HistoryListItem
    {
        public string AuthorName { get; set; } = string.Empty;
        public DateTime TimeStamp { get; set; } = DateTime.MinValue;
        public ObservableCollection<string> ModifiedObjects { get; set; } = new ObservableCollection<string>();
    }
}
