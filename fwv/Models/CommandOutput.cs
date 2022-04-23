using System;
using System.Collections.Generic;
using System.Text;

namespace fwv.Models
{
    public struct CommandOutput
    {
        public string StandardOutput;
        public string StandardError;

        public static CommandOutput operator +(CommandOutput c1, CommandOutput c2)
        {
            return new CommandOutput
            {
                StandardOutput = c1.StandardOutput + "\n" + c2.StandardOutput,
                StandardError = c1.StandardError + "\n" + c2.StandardError
            };
        }
    }
}
