using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nugget.Server
{
    public class WebSocketConnectedEventArgs : EventArgs
    {
        public WebSocketConnection Connection { get; set; }

        public WebSocketConnectedEventArgs(WebSocketConnection connection)
        {
            Connection = connection;
        }
    }
}
