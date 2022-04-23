using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace fwv.Models
{
    public class RepositoryListItem
    {
        public string Hash
        {
            get
            {
                string inputString = $"{RepositoryUrl}{LocalDirectoryPath}";
                string hash = HashGenerator.CreateMD5(inputString);
                return hash;
            }
        }
        public bool IsModified { get; set; }
        public string RepositoryUrl { get; set; }
        public string LocalDirectoryPath { get; set; }
    }
}
