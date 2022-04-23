using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace fwv.Models
{
    public class ModifiedEventArgs : EventArgs
    {
        public string FullPath { get; set; }
        public string Name { get; set; }
        public string WatcherHash { get; set; }

        public ModifiedEventArgs(string directory, string name, string watcherHash)
        {
            FullPath = $"{directory}{name}";
            Name = name;
            WatcherHash = watcherHash;
        }
    }

    public delegate void ModifiedEventHandler(object sender, ModifiedEventArgs args);
}
