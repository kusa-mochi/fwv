using System;
using System.Collections.Generic;
using System.Text;

namespace fwv.Common
{
    public class GitCloneCommandItem : GitCommandItemBase
    {
        public string RemoteUrl;
        public string WorkingDirectoryPath;

        public override object Clone()
        {
            return new GitCloneCommandItem(RemoteUrl, WorkingDirectoryPath);
        }

        public GitCloneCommandItem(string remoteUrl, string workingDirectoryPath)
        {
            Command = GitCommand.Clone;
            RemoteUrl = remoteUrl;
            WorkingDirectoryPath = workingDirectoryPath;
        }
    }
}
