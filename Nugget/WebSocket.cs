using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nugget
{
    abstract class WebSocket
    {
        private WebSocketConnection connection;

        public void Send(string data)
        {
            connection.Send(data);
        }
    }
}
