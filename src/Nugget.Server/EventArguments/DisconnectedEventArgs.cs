using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nugget.Server
{
    public class DisconnectedEventArgs : EventArgs
    {
        public string Reason { get; set; }
        public DisconnectedEventArgs(string reason)
        {
            Reason = reason;
        }
    }
}
