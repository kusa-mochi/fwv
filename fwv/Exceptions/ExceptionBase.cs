using System;
using System.Collections.Generic;
using System.Text;

using fwv.Common;

namespace fwv.Exceptions
{
    public class ExceptionBase : Exception
    {
        protected LogManager _logManager = LogManager.GetInstance();

        public ExceptionBase() : base()
        {

        }

        public ExceptionBase(string message) : base(message)
        {

        }
    }
}
