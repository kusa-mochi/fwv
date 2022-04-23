using System;
using System.Collections.Generic;
using System.Text;

namespace fwv.Common
{
    public abstract class GitCommandItemBase : ICloneable
    {
        public GitCommand Command;
        public string WorkingDirectory;

        /// <summary>
        /// this is not a git command "clone", but a method impremntation of ICloneable interface.
        /// </summary>
        /// <returns></returns>
        public abstract object Clone();
    }
}
