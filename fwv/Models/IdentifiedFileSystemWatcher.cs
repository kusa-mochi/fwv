using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace fwv.Models
{
    public class IdentifiedFileSystemWatcher : FileSystemWatcher
    {
        /// <summary>
        /// a hash to recognize which directory is being watched.
        /// </summary>
        public string Hash { get; set; } = null;

        public IdentifiedFileSystemWatcher(string directoryPath) : base(directoryPath)
        {

        }
    }
}
