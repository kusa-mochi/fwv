using System;
using System.Collections.Generic;
using System.Text;

using fwv.Common;

namespace fwv.Common
{
    public class GitInitCommandItem : GitCommandItemBase
    {
        public bool IsBare { get; set; } = false;

        public override object Clone()
        {
            throw new NotImplementedException();
        }

        public GitInitCommandItem(string workingDirectory, bool isBare = false)
        {
            Command = GitCommand.Init;
            WorkingDirectory = workingDirectory;
            IsBare = isBare;
        }
    }
}
