using System;
using System.Collections.Generic;
using System.Text;
using Prism.Services.Dialogs;

namespace fwv.Common
{
    public class DialogInputParameters : DialogParameters
    {
        public void AddRange(Dictionary<string, object> dic)
        {
            foreach (KeyValuePair<string, object> item in dic)
            {
                Add(item.Key, item.Value);
            }
        }

        public DialogInputParameters()
        {
        }
    }
}
