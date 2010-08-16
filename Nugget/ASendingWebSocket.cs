using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nugget
{
    public abstract class ASendingWebSocket
    {
        public WebSocketConnection Connection { get; set; }
        public void Send(string data)
        {
            Connection.Send(data);
        }

    }
}
