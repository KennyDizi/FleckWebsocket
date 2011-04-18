using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nugget.Server;

namespace Nugget.Framework
{

    public abstract class WebSocket : ASendingWebSocket, IWebSocket//, IReceivingWebSocket<T>
    {
        public abstract void Incoming(string data);
        public abstract void Disconnected();
        public abstract void Connected(ClientHandshake handshake);

    }

    //public abstract class WebSocket : WebSocket<string> { } // default to string
}
