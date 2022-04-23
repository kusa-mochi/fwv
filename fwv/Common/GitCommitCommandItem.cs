using System;
using System.Collections.Generic;
using System.Text;

using fwv.Common;

namespace fwv.Common
{
    public class GitCommitCommandItem : GitCommandItemBase
    {
        public string Message { get; set; } = null;

        public override object Clone()
        {
            throw new NotImplementedException();
        }

        public GitCommitCommandItem(string workingDirectory, string message = null)
        {
            Command = GitCommand.Commit;
            WorkingDirectory = workingDirectory;
            Message = message;
        }
    }
}
